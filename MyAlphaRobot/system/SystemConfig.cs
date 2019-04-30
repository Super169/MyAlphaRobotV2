namespace MyAlphaRobot
{
    public class SystemConfig
    {
        public enum FIRMWARE : byte
        {
            release = 0, beta = 1, hailzd = 2
        }

        public string robotConfigFile { get; set; }
        public string blocklyPath { get; set; }
        public bool autoCheckVersion { get; set; }
        public bool autoCheckFirmware { get; set; }
        public bool developerMode { get; set; }
        public byte waitRebootSec { get; set; }
        public bool disableBatteryUpdate { get; set; }
        public bool disableMpuUpdate { get; set; }
        public FIRMWARE firmwareType { get; set; }

        public SystemConfig()
        {
        }

    }
}
