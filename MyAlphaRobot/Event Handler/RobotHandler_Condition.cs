using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyAlphaRobot
{
    public partial class RobotHandler
    {
        public abstract class Condition
        {
            public enum TYPE : byte
            {
                mpu6050 = 1, touch = 2, psx_button = 3, battery = 4, gpio = 5
            }

            public bool isReady = false;
            public abstract TYPE Id();
            public abstract override string ToString();
            public abstract XmlElement ToXml(XmlDocument root);
            public abstract byte[] ToBytes();

            public static Condition FromXmlNode(XmlNode node)
            {
                string objType = node.Attributes["type"].Value;
                Condition cond = null;

                switch (objType)
                {
                    case BLOCKLY.COND.TOUCH.KEY:
                        return new CondTouch(node);

                    case BLOCKLY.COND.MPU6050.KEY:
                        return new CondMpu6050(node);

                    case BLOCKLY.COND.PSX_BUTTON.KEY:
                        return new CondPsxButton(node);

                    case BLOCKLY.COND.BATTERY_READING.KEY:
                        return new CondBattery(node, true);

                    case BLOCKLY.COND.BATTERY_LEVEL.KEY:
                        return new CondBattery(node, false);

                    case BLOCKLY.COND.GPIO.KEY:
                        return new CondGpio(node);
                }
                return cond;
            }


            public static Condition FromBytes(byte[] data, int offset)
            {
                byte objType = data[offset + BLOCKLY.COND.OFFSET.DEVICE];

                switch (objType)
                {
                    case (byte)TYPE.mpu6050:
                        return new CondMpu6050(data, offset);

                    case (byte)TYPE.touch:
                        return new CondTouch(data, offset);

                    case (byte)TYPE.psx_button:
                        return new CondPsxButton(data, offset);

                    case (byte)TYPE.battery:
                        return new CondBattery(data, offset);

                    /*
                    case (byte)TYPE.battery_reading:
                        return new CondBatteryReading(data, offset);

                    case (byte)TYPE.battery_level:
                        return new CondBatteryLevel(data, offset);
                    */
                    case (byte)TYPE.gpio:
                        return new CondGpio(data, offset);
                }

                return null;
            }

            protected static XmlElement GetXmlCondition(XmlDocument root)
            {
                XmlElement evtAction = root.CreateElement(string.Empty, "value", string.Empty);
                evtAction.SetAttribute("name", "condition");
                return evtAction;
            }

        }

        public class CondMpu6050 : Condition
        {
            public override TYPE Id() { return TYPE.mpu6050; }
            public byte axis, axisCheck;
            public Int16 axisValue;

            public CondMpu6050(XmlNode node)
            {
                try
                {
                    axis = byte.Parse(GetFieldValue(node, BLOCKLY.COND.MPU6050.PARM_AXIS));
                    axisCheck = byte.Parse(GetFieldValue(node, BLOCKLY.COND.MPU6050.PARM_AXIS_CHECK));
                    axisValue = Int16.Parse(GetFieldValue(node, BLOCKLY.COND.MPU6050.PARM_AXIS_VALUE));
                    isReady = true;
                }
                catch { }
            }

            public CondMpu6050(byte[] data, int offset)
            {
                axis = data[offset + BLOCKLY.COND.OFFSET.TARGET];
                axisCheck = data[offset + BLOCKLY.COND.OFFSET.CHECK];
                axisValue = BitConverter.ToInt16(data, offset + BLOCKLY.COND.OFFSET.VALUE);
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "MPU6050: ?";
                return string.Format("MPU6050: {0},{1},{2}", axis, axisCheck, axisValue);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.MPU6050.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.MPU6050.PARM_AXIS, axis.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.MPU6050.PARM_AXIS_CHECK, axisCheck.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.MPU6050.PARM_AXIS_VALUE, axisValue.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = axis;
                data[BLOCKLY.COND.OFFSET.CHECK] = axisCheck;
                byte[] value = BitConverter.GetBytes(axisValue);
                data[BLOCKLY.COND.OFFSET.VALUE] = value[0];
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = value[1];
                return data;
            }

        }

        public class CondTouch : Condition
        {
            public override TYPE Id() { return TYPE.touch; }

            public byte status;

            public CondTouch(XmlNode node)
            {
                try
                {
                    status = byte.Parse(GetFieldValue(node, BLOCKLY.COND.TOUCH.PARM_STATUS));
                    isReady = true;
                }
                catch { }
            }

            public CondTouch(byte[] data, int offset)
            {
                status = data[offset + BLOCKLY.COND.OFFSET.VALUE];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Touch: ?";
                return string.Format("Touch: {0}", status);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.TOUCH.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.TOUCH.PARM_STATUS, status.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = 0;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.MATCH;
                data[BLOCKLY.COND.OFFSET.VALUE] = status;
                data[BLOCKLY.COND.OFFSET.VALUE+1] = 0;
                return data;
            }
        }

        public class CondPsxButton : Condition
        {
            public override TYPE Id() { return TYPE.psx_button; }
            UInt16 button;

            public CondPsxButton(XmlNode node)
            {
                try
                {
                    // button = UInt16.Parse(GetFieldValue(node, BLOCKLY.COND.PSX_BUTTON.PARM_BUTTON));
                    string fieldValue = GetFieldValue(node, BLOCKLY.COND.PSX_BUTTON.PARM_BUTTON);
                    button = Convert.ToUInt16(fieldValue, 16);
                    isReady = true;
                }
                catch { }
            }

            public CondPsxButton(byte[] data, int offset)
            {
                button = BitConverter.ToUInt16(data, offset + BLOCKLY.COND.OFFSET.VALUE);
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "PSX Button: ?";
                return string.Format("PSX Button: {0}", button);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.PSX_BUTTON.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.PSX_BUTTON.PARM_BUTTON, button.ToString("X4")));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = 0;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.BUTTON;
                byte[] value = BitConverter.GetBytes(button);
                data[BLOCKLY.COND.OFFSET.VALUE] = value[0];
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = value[1];
                return data;
            }

        }

        public class CondBattery : Condition
        {
            public override TYPE Id() { return TYPE.battery; }
            public byte targetType;
            public UInt16 targetValue;

            public CondBattery(XmlNode node, bool reading)
            {
                try
                {
                    if (reading)
                    {
                        targetType = 0;
                        targetValue = UInt16.Parse(GetFieldValue(node, BLOCKLY.COND.BATTERY_READING.PARM_READING));
                    } else
                    {
                        targetType = 1;
                        targetValue = byte.Parse(GetFieldValue(node, BLOCKLY.COND.BATTERY_LEVEL.PARM_LEVEL));
                    }
                    isReady = true;
                }
                catch { }
            }

            public CondBattery(byte[] data, int offset)
            {
                targetType = data[offset + BLOCKLY.COND.OFFSET.TARGET];
                targetValue = data[offset + BLOCKLY.COND.OFFSET.VALUE];
                isReady = true;
            }

            public override string ToString()
            {
                String label = "Battery " + (targetType == 0 ? "Reading" : "Level");
                if (!isReady) return (label + ": ?");
                return string.Format("{0}: {1}", label, targetValue);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block;
                string key, parm;
                if (targetType == 0) {
                    key = BLOCKLY.COND.BATTERY_READING.KEY;
                    parm = BLOCKLY.COND.BATTERY_READING.PARM_READING;
                } else
                {
                    key = BLOCKLY.COND.BATTERY_LEVEL.KEY;
                    parm = BLOCKLY.COND.BATTERY_LEVEL.PARM_LEVEL;
                }
                block = GetXmlBlock(root, key);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, parm, targetValue.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = targetType;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.LESS;
                byte[] value = BitConverter.GetBytes(targetValue);
                data[BLOCKLY.COND.OFFSET.VALUE] = value[0];
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = value[1];
                return data;
            }
        }


        /*
        public class CondBatteryReading : Condition
        {
            public override TYPE Id() { return TYPE.battery_reading; }
            public UInt16 batteryReading;

            public CondBatteryReading(XmlNode node)
            {
                try
                {
                    batteryReading = UInt16.Parse(GetFieldValue(node, BLOCKLY.COND.BATTERY_READING.PARM_READING));
                    isReady = true;
                }
                catch { }
            }

            public CondBatteryReading(byte[] data, int offset)
            {
                batteryReading = data[offset + BLOCKLY.COND.OFFSET.VALUE];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Battery Reading: ?";
                return string.Format("Battery Reading: {0}", batteryReading);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.BATTERY_READING.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.BATTERY_READING.PARM_READING, batteryReading.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = 0;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.LESS;
                byte[] value = BitConverter.GetBytes(batteryReading);
                data[BLOCKLY.COND.OFFSET.VALUE] = value[0];
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = value[1];
                return data;
            }

        }

        public class CondBatteryLevel : Condition
        {
            public override TYPE Id() { return TYPE.battery_level; }
            public byte batteryLevel;

            public CondBatteryLevel(XmlNode node)
            {
                try
                {
                    batteryLevel = byte.Parse(GetFieldValue(node, BLOCKLY.COND.BATTERY_LEVEL.PARM_LEVEL));
                    isReady = true;
                }
                catch { }

            }

            public CondBatteryLevel(byte[] data, int offset)
            {
                batteryLevel = data[offset + BLOCKLY.COND.OFFSET.VALUE];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Battery Level: ?";
                return string.Format("Battery Level: {0}", batteryLevel);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.BATTERY_LEVEL.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.BATTERY_LEVEL.PARM_LEVEL, batteryLevel.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = 0;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.LESS;
                data[BLOCKLY.COND.OFFSET.VALUE] = batteryLevel;
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = 0;
                return data;
            }
        }
        */

        public class CondGpio : Condition
        {
            public override TYPE Id() { return TYPE.gpio; }
            public byte pin, status;

            public CondGpio(XmlNode node)
            {
                try
                {
                    pin = byte.Parse(GetFieldValue(node, BLOCKLY.COND.GPIO.PARM_PIN));
                    status = byte.Parse(GetFieldValue(node, BLOCKLY.COND.GPIO.PARM_STATUS));
                    isReady = true;
                }
                catch { }
            }

            public CondGpio(byte[] data, int offset)
            {
                pin = data[offset + BLOCKLY.COND.OFFSET.TARGET];
                status = data[offset + BLOCKLY.COND.OFFSET.VALUE];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "GPIO: ?";
                return string.Format("GPIO: {0},{1}", pin, status);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtCond = GetXmlCondition(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.COND.GPIO.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.GPIO.PARM_PIN, pin.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.COND.GPIO.PARM_STATUS, status.ToString()));
                }
                evtCond.AppendChild(block);
                return evtCond;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.COND.SIZE];
                data[BLOCKLY.COND.OFFSET.DEVICE] = (byte)this.Id();
                data[BLOCKLY.COND.OFFSET.ID] = 0;
                data[BLOCKLY.COND.OFFSET.TARGET] = pin;
                data[BLOCKLY.COND.OFFSET.CHECK] = BLOCKLY.COND.CHECK_MODE.MATCH;
                data[BLOCKLY.COND.OFFSET.VALUE] = status;
                data[BLOCKLY.COND.OFFSET.VALUE + 1] = 0;
                return data;
            }

        }
    }
}
