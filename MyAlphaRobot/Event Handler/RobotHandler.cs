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
        public const byte version = 1;
        public enum MODE : byte
        {
            idle = 0, busy = 1
        }

        public enum ACTION : byte
        {
            before = 1, after = 2
        }

        public string type;
        public int x, y;
        public List<Event> events = new List<Event>();

        public RobotHandler()
        {

        }

        public RobotHandler(XmlNode node, string type, int x, int y)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            events = new List<Event>();
            GoNextNode(node);
        }

        public RobotHandler(byte[] data, string type, int x, int y)
        {
            this.type = type;
            this.x = x;
            this.y = y;
            events = new List<Event>();
            int count = data.Length / BLOCKLY.EVENT.SIZE;
            for (int i = 0; i < count; i++)
            {
                int offset = i * BLOCKLY.EVENT.SIZE;
                events.Add(Event.FromBytes(data, offset));
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < events.Count; i++)
            {
                sb.Append(string.Format("{0:D3}: {1}\n", i, events[i].ToString()));
            }
            return sb.ToString();
        }

        public XmlElement ToXml(XmlDocument root)
        {
            XmlElement xml = root.CreateElement(string.Empty, "block", string.Empty);
            xml.SetAttribute("type", type);
            xml.SetAttribute("id", Guid.NewGuid().ToString());
            xml.SetAttribute("deletable", "false");
            xml.SetAttribute("movable", "false");
            xml.SetAttribute("x", x.ToString());
            xml.SetAttribute("y", y.ToString());

            if (events.Count > 0)
            {
                List<XmlElement> xmlEvents = new List<XmlElement>();
                for (int i = 0; i < events.Count; i++)
                {
                    xmlEvents.Add(events[i].ToXml(root));
                }
                for (int i = xmlEvents.Count - 2; i >= 0; i--)
                {
                    XmlElement nextNode = root.CreateElement(string.Empty, "next", string.Empty);
                    nextNode.AppendChild(xmlEvents[i + 1]);
                    xmlEvents[i].AppendChild(nextNode);
                }

                XmlElement baseNode = root.CreateElement(string.Empty, "next", string.Empty);
                baseNode.AppendChild(xmlEvents[0]);
                xml.AppendChild(baseNode);
            }

            return xml;
        }

        public byte[] ToBytes()
        {
            byte[] data = new byte[events.Count * BLOCKLY.EVENT.SIZE];
            for (byte idx = 0; idx < events.Count; idx++ )
            {
                int offset = idx * BLOCKLY.EVENT.SIZE;
                byte[] evt = events[idx].ToBytes();
                data[offset] = idx;
                for (int i = 0; i < BLOCKLY.EVENT.SIZE; i++)
                {
                    data[offset + i] = evt[i];
                }
            }
            return data;
        }


        private void GoNextNode(XmlNode node)
        {
            XmlNode nextNode;
            // if (MyUtil.XML.GetSingleNode(node, "next", out nextNode))
            if (MyUtil.XML.GetSingleNode(node, "next", out nextNode))
            {
                if (nextNode.ChildNodes.Count == 1)
                {
                    BuildEventList(nextNode.ChildNodes[0]);
                }
                else
                {
                    // TODO: unexpected multiple next node
                }
            }
        }

        private void BuildEventList(XmlNode node)
        {
            events.Add(Event.FromXmlNode(node));
            /*
            string objType = node.Attributes["type"].Value;
            XmlNode condNode, actionNode;

            condNode = node.SelectSingleNode(string.Format("value[@name=\"{0}\"]", BLOCKLY.EVENT.PARM.CONDITION));
            actionNode = node.SelectSingleNode(string.Format("value[@name=\"{0}\"]", BLOCKLY.EVENT.PARM.ACTION));

            switch (objType) {
                case BLOCKLY.EVENT.PRE_CONDITION:
                    events.Add(new EventPreCond());
                    break;
                case BLOCKLY.EVENT.HANDLER:
                    events.Add(new EventHandler());
                    break;
                default:
                    events.Add(null);
                    break;
            }
            */
            GoNextNode(node);
        }

        public static string GetFieldValue(XmlNode node, string fieldName)
        {
            XmlNodeList fields = node.SelectNodes(string.Format("field[@name=\"{0}\"]", fieldName));
            if (fields.Count != 1) return "";
            if (fields[0].ChildNodes.Count == 0)
            {
                throw new Exception(string.Format("Missing field: {0}", fieldName));
            }
            return fields[0].ChildNodes[0].Value;
        }

        public static XmlElement GetXmlBlock(XmlDocument root, string type)
        {
            XmlElement block = root.CreateElement(string.Empty, "block", string.Empty);
            block.SetAttribute("type", type);
            block.SetAttribute("id", Guid.NewGuid().ToString());
            return block;
        }

        public static XmlElement GetXmlField(XmlDocument root, string name, string value)
        {
            XmlElement field = root.CreateElement(string.Empty, "field", string.Empty);
            field.SetAttribute("name", name);
            field.AppendChild(root.CreateTextNode(value));
            return field;
        }


    }
}
