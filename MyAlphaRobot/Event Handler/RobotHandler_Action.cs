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
        public abstract class Action
        {
            public enum TYPE : byte
            {
                play_action = 1, stop_action = 2, head_led = 3, mp3_play_mp3 = 4, mp3_play_file = 5, mp3_stop = 6, gpio = 7, system_action = 8, servo = 9
            }

            public bool isReady = false;

            public abstract TYPE Id();
            public abstract override string ToString();
            public abstract XmlElement ToXml(XmlDocument root);
            public abstract byte[] ToBytes();

            public static Action FromXmlNode(XmlNode node)
            {
                string objType = node.Attributes["type"].Value;

                switch (objType)
                {
                    case BLOCKLY.ACTION.PLAY_ACTION.KEY:
                        return new ActionPlayAction(node);

                    case BLOCKLY.ACTION.STOP_ACTION.KEY:
                        return new ActionStopAction(node);

                    case BLOCKLY.ACTION.HEAD_LED.KEY:
                        return new ActionHeadLed(node);

                    case BLOCKLY.ACTION.MP3_PLAY_MP3.KEY:
                        return new ActionMp3PlayMp3(node);

                    case BLOCKLY.ACTION.MP3_PLAY_FILE.KEY:
                        return new ActionMp3PlayFile(node);

                    case BLOCKLY.ACTION.MP3_STOP.KEY:
                        return new ActionMp3Stop(node);

                    case BLOCKLY.ACTION.GPIO.KEY:
                        return new ActionGpio(node);

                    case BLOCKLY.ACTION.SYSTEM_ACTION.KEY:
                        return new ActionSystemAction(node);

                    case BLOCKLY.ACTION.SERVO.KEY:
                        return new ActionServo(node);

                }
                return null;
            }

            public static Action FromBytes(byte[] data, int offset)
            {
                byte objType = data[offset + BLOCKLY.ACTION.OFFSET.TYPE];
                switch (objType)
                {
                    case (byte) TYPE.play_action:
                        return new ActionPlayAction(data, offset);

                    case (byte)TYPE.stop_action:
                        return new ActionStopAction(data, offset);

                    case (byte)TYPE.head_led:
                        return new ActionHeadLed(data, offset);

                    case (byte)TYPE.mp3_play_mp3:
                        return new ActionMp3PlayMp3(data, offset);

                    case (byte)TYPE.mp3_play_file:
                        return new ActionMp3PlayFile(data, offset);

                    case (byte)TYPE.mp3_stop:
                        return new ActionMp3Stop(data, offset);

                    case (byte)TYPE.gpio:
                        return new ActionGpio(data, offset);

                    case (byte)TYPE.system_action:
                        return new ActionSystemAction(data, offset);

                    case (byte)TYPE.servo:
                        return new ActionServo(data, offset);

                }
                return null;
            }

            protected static XmlElement GetXmlAction(XmlDocument root)
            {
                XmlElement evtAction = root.CreateElement(string.Empty, "value", string.Empty);
                evtAction.SetAttribute("name", "action");
                return evtAction;
            }

        }

        public class ActionPlayAction : Action
        {
            public override TYPE Id() { return TYPE.play_action; }
            public byte action_id;

            public ActionPlayAction(XmlNode node)
            {
                try
                {
                    action_id = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.PLAY_ACTION.PARM_ACTION_ID));
                    isReady = true;
                }
                catch
                {
                }
            }

            public ActionPlayAction(byte[] data, int offset)
            {
                action_id = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Play Action: ?";
                return string.Format("Play Action: {0}", action_id);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.PLAY_ACTION.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.PLAY_ACTION.PARM_ACTION_ID, action_id.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte) this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = action_id;
                return data;
            }
        }

        public class ActionStopAction : Action
        {
            public override TYPE Id() { return TYPE.stop_action; }

            public ActionStopAction(XmlNode node)
            {
                isReady = true;
            }

            public ActionStopAction(byte[] data, int offset)
            {
                isReady = true;
            }


            public override string ToString()
            {
                if (!isReady) return "Stop Action: ?";
                return string.Format("Stop Action");
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.STOP_ACTION.KEY);
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                return data;
            }
        }

        public class ActionHeadLed : Action
        {
            public override TYPE Id() { return TYPE.head_led; }
            public byte status;

            public ActionHeadLed(XmlNode node)
            {
                try
                {
                    status = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.HEAD_LED.PARM_STATUS));
                    isReady = true;
                }
                catch
                {
                }
            }

            public ActionHeadLed(byte[] data, int offset)
            {
                status = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Head LED: ?";
                return string.Format("Head LED: {0}", status);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.HEAD_LED.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.HEAD_LED.PARM_STATUS, status.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = status;
                return data;
            }
        }

        public class ActionMp3PlayMp3 : Action
        {
            public override TYPE Id() { return TYPE.mp3_play_mp3; }
            public UInt16 mp3_file;

            public ActionMp3PlayMp3(XmlNode node)
            {
                try
                {
                    mp3_file = UInt16.Parse(GetFieldValue(node, BLOCKLY.ACTION.MP3_PLAY_FILE.PARM_MP3_FILE));
                    isReady = true;
                }
                catch
                {
                }
            }
            public ActionMp3PlayMp3(byte[] data, int offset)
            {
                mp3_file = BitConverter.ToUInt16(data, offset + BLOCKLY.ACTION.OFFSET.PARM_16b);
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "MP3 Play MP3: ?";
                return string.Format("MP3 Play MP3: {0}", mp3_file);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.MP3_PLAY_MP3.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.MP3_PLAY_MP3.PARM_MP3_FILE, mp3_file.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();

                byte[] parm1 = BitConverter.GetBytes(mp3_file);

                data[BLOCKLY.ACTION.OFFSET.PARM_16b] = parm1[0];
                data[BLOCKLY.ACTION.OFFSET.PARM_16b + 1] = parm1[1];
                return data;
            }
        }

        public class ActionMp3PlayFile : Action
        {
            public override TYPE Id() { return TYPE.mp3_play_file; }
            public byte mp3_folder, mp3_file;

            public ActionMp3PlayFile(XmlNode node)
            {
                try
                {
                    mp3_folder = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.MP3_PLAY_FILE.PARM_MP3_FOLDER));
                    mp3_file = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.MP3_PLAY_FILE.PARM_MP3_FILE));
                    isReady = true;
                }
                catch
                {
                }
            }
            public ActionMp3PlayFile(byte[] data, int offset)
            {
                mp3_folder = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                mp3_file = data[offset + BLOCKLY.ACTION.OFFSET.PARM_2];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "MP3 Play File: ?";
                return string.Format("MP3 Play File: {0}, {1}", mp3_folder, mp3_file);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.MP3_PLAY_FILE.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.MP3_PLAY_FILE.PARM_MP3_FOLDER, mp3_folder.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.MP3_PLAY_FILE.PARM_MP3_FILE, mp3_file.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = (byte) mp3_folder;
                data[BLOCKLY.ACTION.OFFSET.PARM_2] = (byte) mp3_file;
                return data;
            }

        }

        public class ActionMp3Stop : Action
        {
            public override TYPE Id() { return TYPE.mp3_stop; }

            public ActionMp3Stop(XmlNode node)
            {
                isReady = true;
            }

            public ActionMp3Stop(byte[] data, int offset)
            {
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "MP3 Stop: ?";
                return string.Format("MP3 Stop");
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.MP3_STOP.KEY);
                evtAction.AppendChild(block);
                return evtAction;
            }
            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                return data;
            }

        }

        public class ActionGpio : Action
        {
            public override TYPE Id() { return TYPE.gpio; }
            public byte pin, status;

            public ActionGpio(XmlNode node)
            {
                try
                {
                    pin = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.GPIO.PARM_PIN));
                    status = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.GPIO.PARM_STATUS));
                    isReady = true;
                }
                catch { }

            }

            public ActionGpio(byte[] data, int offset)
            {
                pin = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                status = data[offset + BLOCKLY.ACTION.OFFSET.PARM_2];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Set GPIO: ?";
                return string.Format("Set GPIO: {0},{1}", pin, status);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.GPIO.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.GPIO.PARM_PIN, pin.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.GPIO.PARM_STATUS, status.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = pin;
                data[BLOCKLY.ACTION.OFFSET.PARM_2] = status;
                return data;
            }
        }

        public class ActionSystemAction : Action
        {
            public override TYPE Id() { return TYPE.system_action; }
            public byte system_action_id;

            public ActionSystemAction(XmlNode node)
            {
                try
                {
                    system_action_id = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.SYSTEM_ACTION.PARM_SYSTEM_ACTION_ID));
                    isReady = true;
                }
                catch
                {
                }
            }

            public ActionSystemAction(byte[] data, int offset)
            {
                system_action_id = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Play System Action: ?";
                return string.Format("Play System Action: {0}", system_action_id);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.SYSTEM_ACTION.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.SYSTEM_ACTION.PARM_SYSTEM_ACTION_ID, system_action_id.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = system_action_id;
                return data;
            }
        }


        public class ActionServo : Action
        {
            public override TYPE Id() { return TYPE.servo; }
            public byte servo_id, action_time;
            public sbyte action_angle;

            public ActionServo(XmlNode node)
            {
                try
                {

                    servo_id = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.SERVO.PARM_SERVO_IO));
                    action_angle = sbyte.Parse(GetFieldValue(node, BLOCKLY.ACTION.SERVO.PARM_ACTION_ANGLE));
                    action_time = byte.Parse(GetFieldValue(node, BLOCKLY.ACTION.SERVO.PARM_ACTION_TIME));
                    isReady = true;
                }
                catch {
                }
            }

            public ActionServo(byte[] data, int offset)
            {
                servo_id = data[offset + BLOCKLY.ACTION.OFFSET.PARM_1];
                action_angle = (sbyte) data[offset + BLOCKLY.ACTION.OFFSET.PARM_2];
                action_time = data[offset + BLOCKLY.ACTION.OFFSET.PARM_3];
                isReady = true;
            }

            public override string ToString()
            {
                if (!isReady) return "Set Servo ? to ? in ? ms";
                return string.Format("Set Servo {0} to {1} in {2} ms", servo_id, action_angle, action_time);
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement evtAction = GetXmlAction(root);
                XmlElement block = GetXmlBlock(root, BLOCKLY.ACTION.SERVO.KEY);
                if (isReady)
                {
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.SERVO.PARM_SERVO_IO, servo_id.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.SERVO.PARM_ACTION_ANGLE, action_angle.ToString()));
                    block.AppendChild(GetXmlField(root, BLOCKLY.ACTION.SERVO.PARM_ACTION_TIME, action_time.ToString()));
                }
                evtAction.AppendChild(block);
                return evtAction;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.ACTION.SIZE];
                data[BLOCKLY.ACTION.OFFSET.TYPE] = (byte)this.Id();
                data[BLOCKLY.ACTION.OFFSET.PARM_1] = servo_id;
                data[BLOCKLY.ACTION.OFFSET.PARM_2] = (byte) action_angle;
                data[BLOCKLY.ACTION.OFFSET.PARM_3] = action_time;
                return data;
            }
        }




    }
}
