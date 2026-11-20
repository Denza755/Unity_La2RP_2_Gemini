public class RequestMakeMacroPacket : ClientPacket
{
    public RequestMakeMacroPacket(Macro macro) : base((byte)GameClientPacketType.RequestMakeMacro)
    {
        WriteI(macro.Id);
        WriteS(macro.Name ?? "");
        WriteS(macro.Description ?? "");
        WriteS(macro.Acronym ?? "");
        WriteC((byte)macro.Icon);

        int count = macro.Commands != null ? macro.Commands.Count : 0;
        if (count > 12) count = 12;
        WriteC((byte)count);

        for (int i = 0; i < count; i++)
        {
            MacroCmd cmd = macro.Commands[i];
            WriteC((byte)(i + 1));
            WriteC((byte)cmd.Type);
            WriteI(cmd.D1);
            WriteC((byte)cmd.D2);
            WriteS(cmd.Command ?? "");
        }

        BuildPacket();
    }
}
