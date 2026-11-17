using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class TargetWindow : L2PopupWindow
{
    private static readonly Color32 DefaultTargetNameColor = new Color32(255, 255, 255, 255);

    private Label _nameLabel;
    private VisualElement _HPBarContainer;
    private VisualElement _HPBar;
    private VisualElement _HPBarBG;
    private bool _hasAppliedNameColor;
    private Color32 _lastAppliedNameColor;
    [SerializeField] private float _targetWindowMinWidth = 175.0f;
    [SerializeField] private float _targetWindowMaxWidth = 300.0f;

    private static TargetWindow _instance;
    public static TargetWindow Instance { get { return _instance; } }

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
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Game/TargetWindow/TargetWindow");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        var statusWindowDragArea = GetElementByClass("drag-area");
        DragManipulator drag = new DragManipulator(statusWindowDragArea, _windowEle, this);
        statusWindowDragArea.AddManipulator(drag);

        var horizontalResizeHandle = GetElementById("SizeControl");
        HorizontalResizeManipulator horizontalResize = new HorizontalResizeManipulator(
            horizontalResizeHandle, _windowEle, _targetWindowMinWidth, _targetWindowMaxWidth);
        horizontalResizeHandle.AddManipulator(horizontalResize);

        var closeBtnHandle = (Button)GetElementById("CloseBtn");
        closeBtnHandle.AddManipulator(new ButtonClickSoundManipulator(closeBtnHandle));
        closeBtnHandle.RegisterCallback<MouseUpEvent>(evt =>
        {
            TargetManager.Instance.ClearTarget();
        });

        horizontalResizeHandle.AddManipulator(horizontalResize);

        _nameLabel = (Label)GetElementById("TargetName");
        if (_nameLabel == null)
        {
            Debug.LogError("Target window target name label is null.");
        }

        _HPBarContainer = GetElementById("HPBar");

        if (_HPBarContainer == null)
        {
            Debug.LogError("Target window _HPBarContainer is null");
        }

        _HPBar = _HPBarContainer.Q<VisualElement>("Bar");
        if (_HPBar == null)
        {
            Debug.LogError("Target window HPBar is null");
        }

        _HPBarBG = _HPBarContainer.Q<VisualElement>("BarBg");
        if (_HPBarBG == null)
        {
            Debug.LogError("Target window _HPBarBG is null");
        }

        _windowEle.style.position = Position.Absolute;
        _windowEle.style.left = Screen.width / 2f - _windowEle.resolvedStyle.width / 2f;
        _windowEle.style.top = 0;

        L2GameUI.Instance.WindowLoadComplete();
    }

    private void FixedUpdate()
    {
        if (_windowEle == null)
        {
            return;
        }

        if (TargetManager.Instance.HasTarget())
        {
            if (_isWindowHidden)
            {
                ShowWindow();
            }

            Entity targetData = TargetManager.Instance.Target;
            _nameLabel.text = targetData.Identity.Name;
            RefreshTargetColor();

            if (targetData.Identity.IsHpShowable)
            {
                if (!_HPBarContainer.ClassListContains("visible"))
                {
                    _HPBarContainer.AddToClassList("visible");
                }

                if (_HPBarBG != null && _HPBar != null && targetData.Stats != null && targetData.Stats.MaxHp > 0)
                {
                    float hpRatio = Mathf.Clamp01((float)targetData.Status.Hp / targetData.Stats.MaxHp);
                    float bgWidth = _HPBarBG.resolvedStyle.width;
                    float barWidth = bgWidth * hpRatio;
                    _HPBar.style.width = barWidth;
                }
            }
            else
            {
                if (_HPBarContainer.ClassListContains("visible"))
                {
                    _HPBarContainer.RemoveFromClassList("visible");
                }
            }
        }
        else if (!_isWindowHidden)
        {
            HideWindow(false);
        }
    }

    public override void ShowWindow()
    {
        base.ShowWindow();
        L2GameUI.Instance.WindowOpened(this);
    }

    public override void HideWindow(bool silent)
    {
        base.HideWindow(silent);

        _hasAppliedNameColor = false;
        TargetManager.Instance.ClearTarget();

        if (!silent)
            AudioManager.Instance.PlayUISound("window_close");

        L2GameUI.Instance.WindowClosed(this);
    }

    public void RefreshTargetColor()
    {
        if (_nameLabel == null || TargetManager.Instance == null || !TargetManager.Instance.HasTarget())
        {
            return;
        }

        Entity targetData = TargetManager.Instance.Target;
        if (TargetManager.Instance.TryGetMonsterLevelDiff(targetData, out int diff))
        {
            SetTargetColor(diff);
        }
        else
        {
            ApplyNameColor(DefaultTargetNameColor);
        }
    }

    public static Color32 GetC1TargetColor(int diff)
    {
        if (diff <= -11)
        {
            return new Color32(65, 105, 255, 255);
        }
        if (diff <= -6)
        {
            return new Color32(100, 220, 255, 255);
        }
        if (diff <= -3)
        {
            return new Color32(130, 255, 130, 255);
        }
        if (diff <= 2)
        {
            return new Color32(255, 255, 255, 255);
        }
        if (diff <= 4)
        {
            return new Color32(255, 175, 175, 255);
        }
        if (diff <= 6)
        {
            return new Color32(255, 125, 125, 255);
        }
        if (diff <= 9)
        {
            return new Color32(255, 75, 75, 255);
        }

        return new Color32(255, 20, 20, 255);
    }

    private void ApplyNameColor(Color32 color)
    {
        if (_nameLabel == null)
        {
            return;
        }

        if (_hasAppliedNameColor &&
            _lastAppliedNameColor.r == color.r &&
            _lastAppliedNameColor.g == color.g &&
            _lastAppliedNameColor.b == color.b &&
            _lastAppliedNameColor.a == color.a)
        {
            return;
        }

        _nameLabel.style.color = (Color)color;
        _lastAppliedNameColor = color;
        _hasAppliedNameColor = true;
    }

    public void SetTargetColor(int diff)
    {
        ApplyNameColor(GetC1TargetColor(diff));
    }
}
