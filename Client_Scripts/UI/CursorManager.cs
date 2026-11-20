using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private CursorType _lastCursorType = CursorType.Default;
    public enum CursorType
    {
        Default,
        Attack,
        Talk,
        Pickup
    }

    [SerializeField] private Texture2D _defaultCursorTexture;
    [SerializeField] private Texture2D _attackCursorTexture;
    [SerializeField] private Texture2D _talkCursorTexture;
    [SerializeField] private Texture2D _pickupCursorTexture;
    private string _cursorFolder = "Data/UI/Assets/Cursor/";

    private static CursorManager _instance;
    public static CursorManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        _defaultCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Default");
        _attackCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Attack");
        _talkCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Talk");
        _pickupCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Pickup");

        // Force standard Windows system cursor everywhere
        UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ChangeCursor(CursorType cursorType)
    {
        if (_lastCursorType == cursorType)
        {
            return;
        }

        _lastCursorType = cursorType;

        switch (cursorType)
        {
            case CursorType.Default:
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                break;
            case CursorType.Attack:
                if (_attackCursorTexture == null)
                {
                    _attackCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Attack");
                }
                UnityEngine.Cursor.SetCursor(_attackCursorTexture, Vector2.zero, CursorMode.Auto);
                break;
            case CursorType.Talk:
                if (_talkCursorTexture == null)
                {
                    _talkCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Talk");
                }
                UnityEngine.Cursor.SetCursor(_talkCursorTexture, Vector2.zero, CursorMode.Auto);
                break;
            case CursorType.Pickup:
                if (_pickupCursorTexture == null)
                {
                    _pickupCursorTexture = Resources.Load<Texture2D>(_cursorFolder + "Pickup");
                }
                UnityEngine.Cursor.SetCursor(_pickupCursorTexture, Vector2.zero, CursorMode.Auto);
                break;
        }
    }
}
