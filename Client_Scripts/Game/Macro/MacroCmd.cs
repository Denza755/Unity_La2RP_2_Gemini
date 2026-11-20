[System.Serializable]
public class MacroCmd
{
    public int Entry;
    public int Type;
    public int D1;
    public int D2;
    public string Command;

    public MacroCmd(int entry, int type, int d1, int d2, string command)
    {
        Entry = entry;
        Type = type;
        D1 = d1;
        D2 = d2;
        Command = command ?? "";
    }
}
