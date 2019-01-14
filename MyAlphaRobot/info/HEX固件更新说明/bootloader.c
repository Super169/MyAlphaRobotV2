/********************************************************************************
 * 文件名  ：bootloader.c
 * 描述    ：bootloader     
 * 作者    ：泽畔无材  zepanwucai@gmail.com
 *修改时间 ：2013-11-18
**********************************************************************************/

#include "bootloader.h"

int main(void)
{
    u16 tryCnt = 65535;
    u8 ch, page;
    u8 i = 10;  //0.5s
    u8 buf[BLOCK_BYTES];
    u8 verify;
    u8* addr;
    //set hsi = 16M,复位后默认hsi
    CLK->CKDIVR &= 0xE7;//(uint8_t)(~CLK_CKDIVR_HSIDIV); //清空(00)即不分频
    while(!(CLK->ICKR & 0x02));   //等待内部时钟稳定
    //set baudrate = 115200, 复位后默认开启uart，格式默认，只需设置波特率及使能收发
    UART1->BRR2 |= 0x0A;//(uint8_t)((uint8_t)(((BaudRate_Mantissa100 - (BaudRate_Mantissa * 100)) << 4) / 100) & (uint8_t)0x0F) |\
                   //(uint8_t)((BaudRate_Mantissa >> 4) & (uint8_t)0xF0);  
    UART1->BRR1 |= 0x08;//(uint8_t)BaudRate_Mantissa; 
    UART1->CR2 |= 0x0C;      //(uint8_t)(UART1_CR2_TEN | UART1_CR2_REN);  //使能收发    
    UART1->CR5 |= 0x08;          //UART1_CR5_HDSEL;    //单线串口
    UART1->CR1 &= 0xDF;  //(uint8_t)(~UART1_CR1_UARTD); 
    
    //bootloader通信过程
    //1.舵机板已经在运行状态下,连续发送6个或6个以上  0xA5 就复位了舵机板，进入boot程序
    //2.上电后的0.5s内一直等待  0XA5 标志位，如果总线有信号进入，就进入固件更新程序，没有就进入APP程序
    
    while(i)    
    {
        if(UART1->SR & 0x20)    //wait for head 
        {
            ch = (uint8_t)UART1->DR;    
            if(ch == BOOT_HEAD) break;   // BOOT_HEAD->0xa5 , boot握手标志，上电瞬间接收到0xA5表示要更新固件
        }
        tryCnt--;
        if(tryCnt == 0) i--;      //循环等待0.5S
    }
    
    if(i == 0)
    {    //goto app
        goto goApp;
    }
    //else
    {
        //unlock flash,解锁flash
        FLASH->PUKR = FLASH_RASS_KEY1;
        FLASH->PUKR = FLASH_RASS_KEY2;
        
        UART1_SendB(0xa0|INIT_PAGE);       //下位机发送写入的起始页序号，stm8FLASH有128页，序号从0-127，
                                           //INIT_PAGE=0x09，0xa0|INIT_PAGE = 0xA9，说明从FLASH的第10页开始写，也就是从地址 0x8240开始写 （0x8000+10*64B）
        while(1)
        {
          ch = UART1_RcvB();     //下位机接收第一个命令Byte
          switch(ch)           //判断一下这个命令
            {
            case BOOT_GO:          //BOOT_GO -> 0xa9 下位机接收到0XA9， 表示固件更新完成，即页序号从9-127的数据都写入完成，进入APP程序
                goApp:
                FLASH->IAPSR &= (uint8_t)0xFD; //锁住flash
                //goto app
                asm("JP $8240");
                break;
            case BOOT_WRITE:      //BOOT_WRITE -> 0xa7 ，下位机接收到0xa7 表示要写入数据
              page = UART1_RcvB();          //写入的第一个Byte为页地址，stm8FLASH有128页，序号从0-127 ，我所发布的固件都是从0x8240开始写
                                            //可根据前面读取到的APP首页地址决定从哪一页开始写
                addr = (u8*)(FLASH_START + (page << BLOCK_SHIFT));
                verify = 0;
                for(i = 0; i < BLOCK_BYTES; i++)    //连续读取64B的固件数据
                {
                    buf[i] = UART1_RcvB();
                    verify += buf[i];             //暂存这64B数据
                }
                if(verify == UART1_RcvB())  //64B读取完毕后，后面是接收校验码，将前面的64B相加取后8位的Byte
                {
                    FLASH_ProgBlock(addr, buf);  //校验成功就写入这一页的数据到FLASH中
                    
                    UART1_SendB(BOOT_OK);    // BOOT_OK -> 0xa0 , 这一页写入成功后，下位机发送成功标志 0xa0 给上位机
                    break;
                    //else,写入校验失败，可能是flash损坏
                }
            default: //上面校验失败的情况也会到这里来
              UART1_SendB(BOOT_ERR);   //BOOT_ERR -> 0xa1 ,这一页写入失败了，下位机发送失败标志给上位机，上位机重发这一页的数据
                break;
            }
        }
    }
}

void UART1_SendB(u8 ch)
{
    UART1->CR2 = 0x08;
    while (!(UART1->SR & 0x80));
    UART1->DR = ch;    
    for(int i=0;i<180;i++){
      nop();
    }
    UART1->CR2 = 0x04;
}

u8 UART1_RcvB(void)
{
     while(!(UART1->SR & 0x20));
     return ((uint8_t)UART1->DR);
}



//addr must at begin of block
IN_RAM(void FLASH_ProgBlock(uint8_t * addr, uint8_t *Buffer))
{
    u8 i;
    /* Standard programming mode */ /*No need in standard mode */
    FLASH->CR2 |= FLASH_CR2_PRG;
    FLASH->NCR2 &= (uint8_t)(~FLASH_NCR2_NPRG);
    /* Copy data bytes from RAM to FLASH memory */
    for (i = 0; i < BLOCK_BYTES; i++)
    {
        *((PointerAttr uint8_t*) (uint16_t)addr + i) = ((uint8_t)(Buffer[i]));    
    }
}
