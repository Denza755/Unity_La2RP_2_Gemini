public class RequestMakeMacroPacket : ClientPacket
{
    public RequestMakeMacroPacket(Macro macro) : base((byte)GameClientPacketType.RequestMakeMacro)
    {
        WriteI(macro.Id);
        WriteS(macro.Name ?? "");
        WriteS(macro.Description ?? "");
        WriteS(macro.Acronym ?? "");
        WriteB((byte)macro.Icon);

        int count = macro.Commands != null ? macro.Commands.Count : 0;
        if (count > 12) count = 12;
        WriteB((byte)count);

        for (int i = 0; i < count; i++)
        {
            MacroCmd cmd = macro.Commands[i];
            WriteB((byte)(i + 1));
            WriteB((byte)cmd.Type);
            WriteI(cmd.D1);
            WriteB((byte)cmd.D2);
            WriteS(cmd.Command ?? "");
        }

        BuildPacket();
    }
}
