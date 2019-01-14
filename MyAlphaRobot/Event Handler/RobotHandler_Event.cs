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
        public abstract class Event
        {
            public enum TYPE
            {
                handler = 1, preCond = 2
            }
            public abstract TYPE Id();
            public override abstract string ToString();
            public abstract XmlElement ToXml(XmlDocument root);
            public abstract byte[] ToBytes();

            public static Event FromXmlNode(XmlNode node)
            {
                string objType = node.Attributes["type"].Value;
                Event evt = null;

                switch (objType)
                {
                    case BLOCKLY.EVENT.PRE_CONDITION:
                        evt = new EventPreCond(node);
                        break;
                    case BLOCKLY.EVENT.HANDLER:
                        evt = new EventHandler(node);
                        break;
                }
                return evt;
            }

            public static Event FromBytes(byte[] data, int offset)
            {
                byte objType = data[offset + BLOCKLY.EVENT.OFFSET.TYPE];

                switch (objType)
                {
                    case (byte)TYPE.handler:
                        return new EventHandler(data, offset);

                    case (byte)TYPE.preCond:
                        return new EventPreCond(data, offset);

                }
                return null;
            }
        }

        public class EventHandler : Event
        {
            public override TYPE Id() { return TYPE.handler; }

            public Condition condition;
            public Action action;

            public EventHandler(XmlNode node)
            {
                XmlNode condNode, actionNode;
                condNode = node.SelectSingleNode(string.Format("value[@name=\"{0}\"]", BLOCKLY.EVENT.PARM.CONDITION));
                actionNode = node.SelectSingleNode(string.Format("value[@name=\"{0}\"]", BLOCKLY.EVENT.PARM.ACTION));
                if ((condNode != null) && (condNode.ChildNodes.Count == 1))
                {
                    condition = Condition.FromXmlNode(condNode.ChildNodes[0]);
                }
                if ((actionNode != null) && (actionNode.ChildNodes.Count == 1))
                {
                    action = Action.FromXmlNode(actionNode.ChildNodes[0]);
                }

            }

            public EventHandler(byte[] data, int offset)
            {
                condition = Condition.FromBytes(data, offset + BLOCKLY.EVENT.OFFSET.CONDITION);
                action = Action.FromBytes(data, offset + BLOCKLY.EVENT.OFFSET.ACTION);
            }

            public override string ToString()
            {
                return string.Format("當 [{0}], 就 [{1}]", 
                                     (condition == null ? "missing" : condition.ToString()), 
                                     (action == null ? "missing" : action.ToString()));
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement block = GetXmlBlock(root, BLOCKLY.EVENT.HANDLER);
                block.AppendChild(condition.ToXml(root));
                block.AppendChild(action.ToXml(root));
                return block;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.EVENT.SIZE];
                data[BLOCKLY.EVENT.OFFSET.SEQ] = 0;
                data[BLOCKLY.EVENT.OFFSET.TYPE] = (byte)this.Id();
                byte[] bCond = condition.ToBytes();
                for (int i = 0; i < BLOCKLY.COND.SIZE; i++)
                {
                    data[BLOCKLY.EVENT.OFFSET.CONDITION + i] = bCond[i];
                }
                byte[] bAction = action.ToBytes();
                for (int i = 0; i < BLOCKLY.ACTION.SIZE; i++)
                {
                    data[BLOCKLY.EVENT.OFFSET.ACTION + i] = bAction[i];
                }
                return data;
            }
        }

        public class EventPreCond : Event
        {
            public override TYPE Id() { return TYPE.preCond; }
            public Condition condition;

            public EventPreCond(XmlNode node)
            {
                XmlNode condNode;
                condNode = node.SelectSingleNode(string.Format("value[@name=\"{0}\"]", BLOCKLY.EVENT.PARM.CONDITION));
                if ((condNode != null) && (condNode.ChildNodes.Count == 1))
                {
                    condition = Condition.FromXmlNode(condNode.ChildNodes[0]);
                }
            }

            public EventPreCond(byte[] data, int offset)
            {
                condition = Condition.FromBytes(data, offset + BLOCKLY.EVENT.OFFSET.CONDITION);
            }

            public override string ToString()
            {
                return string.Format("先決條件: [{0}]", (condition == null ? "missing" : condition.ToString()));
            }

            public override XmlElement ToXml(XmlDocument root)
            {
                XmlElement block = GetXmlBlock(root, BLOCKLY.EVENT.PRE_CONDITION);
                block.AppendChild(condition.ToXml(root));
                return block;
            }

            public override byte[] ToBytes()
            {
                byte[] data = new byte[BLOCKLY.EVENT.SIZE];
                data[BLOCKLY.EVENT.OFFSET.SEQ] = 0;
                data[BLOCKLY.EVENT.OFFSET.TYPE] = (byte)this.Id();
                byte[] bCond = condition.ToBytes();
                for (int i = 0; i < BLOCKLY.COND.SIZE; i++)
                {
                    data[BLOCKLY.EVENT.OFFSET.CONDITION + i] = bCond[i];
                }
                return data;
            }

        }

    }
}
