using System.Collections.Generic;

[System.Serializable]
public class Macro
{
    public int Id;
    public int Icon;
    public string Name;
    public string Description;
    public string Acronym;
    public List<MacroCmd> Commands;

    public Macro()
    {
        Id = 0;
        Icon = 0;
        Name = "";
        Description = "";
        Acronym = "";
        Commands = new List<MacroCmd>();
    }

    public Macro(int id, int icon, string name, string description, string acronym, List<MacroCmd> commands)
    {
        Id = id;
        Icon = icon;
        Name = name ?? "";
        Description = description ?? "";
        Acronym = acronym ?? "";
        Commands = commands ?? new List<MacroCmd>();
    }
}
