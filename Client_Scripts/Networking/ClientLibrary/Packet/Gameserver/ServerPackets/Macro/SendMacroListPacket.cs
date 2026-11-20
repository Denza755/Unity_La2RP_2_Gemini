using System.Collections.Generic;

public class SendMacroListPacket : ServerPacket
{
    public int Revision { get; private set; }
    public int Count { get; private set; }
    public bool HasMacro { get; private set; }
    public Macro MacroData { get; private set; }

    public SendMacroListPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        Revision = ReadI();
        ReadC(); // unknown
        Count = ReadC();
        int hasMacro = ReadC();
        HasMacro = hasMacro != 0;

        if (HasMacro)
        {
            int id = ReadI();
            string name = ReadS();
            string descr = ReadS();
            string acronym = ReadS();
            int icon = ReadC();
            int cmdCount = ReadC();

            List<MacroCmd> commands = new List<MacroCmd>();
            for (int i = 0; i < cmdCount; i++)
            {
                int entry = ReadC();
                int type = ReadC();
                int d1 = ReadI();
                int d2 = ReadC();
                string cmd = ReadS();
                commands.Add(new MacroCmd(entry, type, d1, d2, cmd));
            }

            MacroData = new Macro(id, icon, name, descr, acronym, commands);
        }
    }
}
