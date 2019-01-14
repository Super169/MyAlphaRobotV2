using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MyUtil;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        private void LoadActionFile()
        {
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = CONST.ROBOT_ACTION_FILTER;
            if (openFileDialog.ShowDialog() == true)
            {
                String fileName = openFileDialog.FileName;
                try
                {
                    long size = new FileInfo(fileName).Length;
                    bool validFileSize = false;
                    if (size >= CONST.AI.HEADER.SIZE)
                    {
                        validFileSize = (((size - CONST.AI.HEADER.SIZE) % CONST.AI.POSE.SIZE) == 0);
                    }
                    if (!validFileSize)
                    {
                        UpdateInfo(String.Format("Invalid file size: {0} has {1} bytes.", fileName, size), MyUtil.UTIL.InfoType.error);
                        return;
                    }
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    byte[] actionData = null;
                    // int can already have 2,147,483,647 , no need to split
                    actionData = br.ReadBytes((int)size);
                    br.Close();
                    fs.Close();
                    data.ActionInfo ai = UBT.actionTable.action[actionId];
                    if (ai.ReadFromArray(actionData))
                    {
                        UpdateInfo(String.Format("Rebuild action table from {0}", fileName));
                    }
                    else
                    {
                        UpdateInfo(String.Format("Error building table from {0}", fileName), MyUtil.UTIL.InfoType.error);
                    }
                }
                catch (Exception ex)
                {
                    UpdateInfo(String.Format("Error reading data from {0}: {1}", fileName, ex.Message), MyUtil.UTIL.InfoType.error);
                }
                RefreshActionInfo();
            }
        }

        private void SaveActionFile()
        {
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            UpdateInfo();
            data.ActionInfo ai = UBT.actionTable.action[actionId];
            if (ai.actionFileExists && (!ai.poseLoaded))
            {
                MessageBox.Show("动作资料尚未下载到电脑, 请先 [下载动作]", "无法储存", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = CONST.ROBOT_ACTION_FILTER;
            if (saveFileDialog.ShowDialog() == true)
            {
                String fileName = saveFileDialog.FileName;
                try
                {
                    FileStream fs = File.Create(fileName, 2048, FileOptions.None);
                    BinaryWriter bw = new BinaryWriter(fs);
                    bw.Write(ai.GetData());
                    for (UInt16 poseId = 0; poseId < ai.poseCnt; poseId++)
                    {
                        bw.Write(ai.GetPoseData(poseId));
                    }
                    bw.Close();
                    fs.Close();
                    UpdateInfo(String.Format("Action table saved to {0}", fileName));
                }
                catch (Exception ex)
                {
                    UpdateInfo(String.Format("Error saving to {0}: {1}", fileName, ex.Message), MyUtil.UTIL.InfoType.error);
                }
            }

        }

        private void DownloadActionTable()
        {
            if (MessageConfirm("從機械人重新下載所有動作, 當前資料將會被覆蓋"))
            {
                StartSystemWork();
                if (UBT.DownloadActionTable())
                {
                    UpdateInfo("下載資料完成");
                }
                else
                {
                    UpdateInfo("下載資料失敗");
                }
                RefreshActionInfo();
                EndSystemWork();
            }
        }

        private void ConvertAction()
        {
            UpdateInfo();
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "aesx or hts files|*.aesx;*.hts|aesx file (*.aesx)|*.aesx|hts file (*.hts)|*.hts";
            if (openFileDialog.ShowDialog() == true)
            {
                String fileName = openFileDialog.FileName.Trim();
                String lName = fileName.ToLower();
                CONST.UBT_FILE fileType = CONST.UBT_FILE.UNKNOWN;
                if (lName.EndsWith(".aesx")) fileType = CONST.UBT_FILE.AESX;
                if (lName.EndsWith(".hts")) fileType = CONST.UBT_FILE.HTS;
                if (fileType == CONST.UBT_FILE.UNKNOWN)
                {
                    UpdateInfo(String.Format("Invalid file extension {0}", fileName), MyUtil.UTIL.InfoType.error);
                    return;
                }

                char actionCode = UBT.actionTable.action[actionId].actionCode;
                if (MessageConfirm(String.Format("把 UBTech 檔 {0} 的動作檔截入動作 {1} 中, 當前資料將會被覆蓋", fileName, actionCode)))
                {
                    try
                    {
                        long size = new FileInfo(fileName).Length;
                        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        byte[] actionData = null;
                        // File size should not be over 2M, so not need to handle overflow)
                        actionData = br.ReadBytes((int)size);
                        br.Close();
                        fs.Close();
                        string actionName = openFileDialog.SafeFileName;
                        actionName = actionName.Substring(0, actionName.IndexOf('.'));
                        if (UBT.actionTable.ReadFromUBTechFile(actionData, actionId, actionName, fileType))
                        {
                            UpdateInfo(String.Format("Rebuild action table from {0}", fileName));
                        }
                        else
                        {
                            UpdateInfo(String.Format("Error building action from {0}", fileName), MyUtil.UTIL.InfoType.error);
                        }

                    }
                    catch (Exception ex)
                    {
                        UpdateInfo(String.Format("Error reading data from {0}: {1}", fileName, ex.Message), MyUtil.UTIL.InfoType.error);
                    }
                    RefreshActionInfo();
                }
            }

        }

        private void DownloadAction()
        {
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            char actionCode = UBT.actionTable.action[actionId].actionCode;
            if (MessageConfirm(String.Format("下載動作 {0} 的資料, 當前資料將會被覆蓋", actionCode)))
            {
                StartSystemWork();
                if (UBT.DownloadAction((byte)actionId, true))
                {
                    UpdateInfo(String.Format("動作{0} 成功下載", actionCode));
                }
                else
                {
                    UpdateInfo(String.Format("下載動作{0} 失敗", actionCode), MyUtil.UTIL.InfoType.error);
                }
                RefreshActionInfo();
                EndSystemWork();
            }
        }

        private void UploadAction()
        {
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            char actionCode = UBT.actionTable.action[actionId].actionCode;
            if (MessageConfirm(String.Format("上傳動作 {0} 到機械人, 原有的資料將會被覆蓋", actionCode)))
            {
                StartSystemWork();
                if (UBT.UploadAction(actionId))
                {
                    UpdateInfo(String.Format("機械人的動作{0} 成功更新了, 測試後請儲存到 SPIFFS", actionCode));
                    UBT.actionTable.action[actionId].actionFileExists = true;
                    UBT.actionTable.action[actionId].poseLoaded = true;
                    RefreshActionInfo();
                }
                else
                {
                    UpdateInfo(String.Format("更新機械人動作{0} 失敗", actionCode), MyUtil.UTIL.InfoType.error);
                }
                EndSystemWork();
            }
        }

        private void ClearAction()
        {
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            char actionCode = UBT.actionTable.action[actionId].actionCode;
            if (MessageConfirm(String.Format("清除動作 {0} 的資料", actionCode)))
            {
                UBT.actionTable.action[actionId].Reset();
                RefreshActionInfo();
            }
        }

        private void DeleteAction()
        {
            UpdateInfo();
            int actionId = GetSelectedActionId();
            if (actionId < 0) return;
            char actionCode = UBT.actionTable.action[actionId].actionCode;
            if (MessageConfirm(String.Format("刪除 SPIFFS 上動作 {0} 的檔案", actionCode)))
            {
                byte result = UBT.DeleteActionFile((byte)actionId);
                switch (result)
                {
                    case 0:
                        UpdateInfo("Action file deleted.");
                        UBT.actionTable.action[actionId].actionFileExists = false;
                        UBT.actionTable.action[actionId].Reset();
                        RefreshActionInfo();
                        break;
                    case 12:
                        UpdateInfo("Action file does not exists, delete is not required.");
                        break;
                    default:
                        UpdateInfo("Fail deleting action file.");
                        break;
                }
            }
        }

        private void RefreshActionInfo()
        {
            ucActionList.Refresh();
            ucActionDetail.Refresh(ucActionList.GetSelectedActionId());
        }

        private bool MessageConfirm(String msg)
        {
            MessageBoxResult result = MessageBox.Show(msg, "請確定", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes);
        }

        private int GetSelectedActionId()
        {
            int actionId = ucActionList.GetSelectedActionId();
            if (actionId < 0)
            {
                UpdateInfo("請先選擇動作", MyUtil.UTIL.InfoType.alert);
                return -1;
            }
            return actionId;
        }
    }
}
