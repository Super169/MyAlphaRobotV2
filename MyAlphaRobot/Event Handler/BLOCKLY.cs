using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{

    public static class BLOCKLY
    {
        public static class HAT
        {
            public const string IDLE = "status_idle";
            public const string BUSY = "status_playing";
            public const string ROBOT_READY = "status_robot_ready";
        }

        public static class EVENT
        {
            public const string PRE_CONDITION = "event_pre_condition";
            public const string HANDLER = "event_handler";

            public static class PARM
            {
                public const string CONDITION = "condition";
                public const string ACTION = "action";
            }

            public const byte SIZE = 12;
            public static class OFFSET
            {
                public const byte SEQ = 0;
                public const byte TYPE = 1;
                public const byte CONDITION = 2;
                public const byte ACTION = 8;
            }

        }

        public static class COND
        {

            public static class GPIO
            {
                public const string KEY = "cond_gpio";
                public const string PARM_PIN = "gpio_pin";
                public const string PARM_STATUS = "gpio_status";
            }

            public static class PSX_BUTTON
            {
                public const string KEY = "cond_psx_button";
                public const string PARM_BUTTON = "psx_button";
            }

            public static class MPU6050
            {
                public const string KEY = "cond_mpu6050";
                public const string PARM_AXIS = "axis";
                public const string PARM_AXIS_CHECK = "axis_check";
                public const string PARM_AXIS_VALUE = "axis_value";
            }

            public static class TOUCH
            {
                public const string KEY = "cond_touch";
                public const string PARM_STATUS = "touch_status";
            }

            public static class BATTERY_READING
            {
                public const string KEY = "cond_battery_reading";
                public const string PARM_READING = "battery_reading";
            }

            public static class BATTERY_LEVEL
            {
                public const string KEY = "cond_battery_level";
                public const string PARM_LEVEL = "battery_level";
            }

            public const byte SIZE = 6;
            public static class OFFSET
            {
                public const byte DEVICE = 0;
                public const byte ID = 1;
                public const byte TARGET = 2;
                public const byte CHECK = 3;
                public const byte VALUE = 4;
            }

            public static class CHECK_MODE
            {
                public const byte MATCH = 1;
                public const byte GREATER = 2;
                public const byte LESS = 3;
                public const byte BUTTON = 4;
            }


        }

        public static class ACTION
        {
            public static class PLAY_ACTION
            {
                public const string KEY = "action_play_action";
                public const string PARM_ACTION_ID = "action_id";
            }

            public static class STOP_ACTION
            {
                public const string KEY = "action_stop_action";
            }

            public static class HEAD_LED
            {
                public const string KEY = "action_head_led";
                public const string PARM_STATUS = "led_status";
            }

            public static class MP3_PLAY_MP3
            {
                public const string KEY = "action_mp3_play_mp3";
                public const string PARM_MP3_FILE = "mp3_file";
            }

            public static class MP3_PLAY_FILE
            {
                public const string KEY = "action_mp3_play_file";
                public const string PARM_MP3_FOLDER = "mp3_folder";
                public const string PARM_MP3_FILE = "mp3_file";
            }

            public static class MP3_STOP
            {
                public const string KEY = "action_mp3_stop";
            }

            public static class GPIO
            {
                public const string KEY = "action_gpio";
                public const string PARM_PIN = "gpio_pin";
                public const string PARM_STATUS = "gpio_status";
            }

            public static class SYSTEM_ACTION
            {
                public const string KEY = "action_system_action";
                public const string PARM_SYSTEM_ACTION_ID = "system_action_id";
            }

            public static class SERVO
            {
                public const string KEY = "action_servo";
                public const string PARM_SERVO_IO = "servo_id";
                public const string PARM_ACTION_ANGLE = "action_angle";
                public const string PARM_ACTION_TIME = "action_time";
}

            public const byte SIZE = 4;
            public static class OFFSET
            {
                public const byte TYPE = 0;
                public const byte PARM_1 = 1;
                public const byte PARM_2 = 2;
                public const byte PARM_3 = 3;
                public const byte PARM_16b = 2;
            }

        }

    }
}
