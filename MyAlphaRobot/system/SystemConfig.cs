namespace MyAlphaRobot
{
    public class SystemConfig
    {
        public string robotConfigFile { get; set; }
        public string blocklyPath { get; set; }
        public bool autoCheckVersion { get; set; }
        public bool autoCheckFirmware { get; set; }
        public bool developerMode { get; set; }
        public bool disableBatteryUpdate { get; set; }
        public bool disableMpuUpdate { get; set; }

        public SystemConfig()
        {
        }

    }
}
