using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ActionTable
    {
        public ActionInfo[] action = new ActionInfo[CONST.AI.MAX_ACTION];

        public ActionTable()
        {
            InitObject();
        }

        public void InitObject()
        {
            for (byte actionId = 0; actionId < CONST.AI.MAX_ACTION; actionId++)
            {
                action[actionId] = new ActionInfo(actionId);
            }
        }

        public void Reset()
        {
            for (int actionId = 0; actionId < CONST.AI.MAX_ACTION; actionId++)
            {
                action[actionId].Reset();
            }
        }

        public bool ReadFromUBTechFile(byte[] actionData, int actionId, string actionName, CONST.UBT_FILE fileType)
        {
            if ((actionId < 0) || (actionId > CONST.AI.MAX_ACTION)) return false;
            bool success = false;
            switch (fileType)
            {
                case CONST.UBT_FILE.AESX:
                    if (actionData.Length < CONST.AESX_FILE.MIN_SIZE) return false;
                    success = action[actionId].ReadFromAESX(actionData, actionId, actionName);
                    break;
                case CONST.UBT_FILE.HTS:
                    if (actionData.Length < CONST.HTS_FILE.MIN_SIZE) return false;
                    success = action[actionId].ReadFromHTS(actionData, actionId, actionName);
                    break;

            }
            return success;
        }
    }
}
