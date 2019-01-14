Step to add new action: 
1. Open BLOCKLY.cs
   - Add static class under ACTION
     : KEY = <blockly block name>
	 : PARM.. = <blockly parameter name>

2. Open RobotHandler_Action.cs
   1) Add new type in TYPE, e.g. servo = 9
   2) Try to copy an action class as a base to edit
      a) Rename the action class (e.g. ActionServo), then change the name of related constructor 
	  b) Chnage the Id 
      c) Change parameter variables
      e) Modify constructor (XmlNode) to get the variable value from XML using block name defined in BLOCKLY.cs
      e) Modify constructor (byte[]) to get the variable value from byte array returned from MCU
      f) Modify TOString() to show the action information
      g) Modify ToXml() to add blockly child from variables
      h) Modify ToBytes() to set the value to array for passing to MCU (must match with constructor from array)
   3) Go to Action object
      a) Edit FromXmlNode to add case to return new action type
	  b) Edit FromBytes to handle new TYPE