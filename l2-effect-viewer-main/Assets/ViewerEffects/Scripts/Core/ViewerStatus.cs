using System;

public sealed class ViewerStatus
{
    public static ViewerStatus Instance { get; } = new ViewerStatus();

    public string Stage = "ожидание";
    public string Detail = "";
    public string SelectedEffect = "";
    public bool Busy;

    public event Action Changed;

    public void Set(string stage, string detail = null)
    {
        Stage = stage;
        if (detail != null)
            Detail = detail;
        Changed?.Invoke();
    }
}
