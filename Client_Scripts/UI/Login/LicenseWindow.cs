using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LicenseWindow : L2Window
{
    private static LicenseWindow _instance;
    public static LicenseWindow Instance { get { return _instance; } }

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

    private void Update()
    {
        if (!_isWindowHidden)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                AudioManager.Instance.PlayUISound("click_01");
                DisagreeButtonPressed();
            }
            else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                AudioManager.Instance.PlayUISound("click_01");
                AgreeButtonPressed();
            }
        }
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Login/LicenseWindow/LicenseWindow");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);

        yield return new WaitForEndOfFrame();

        VisualElement agreeBtnContainer = GetElementById("AgreeButton");
        Button agreeButton = (Button)agreeBtnContainer.Q<Button>("L2Button");
        agreeButton.Query<VisualElement>().ForEach(child => child.pickingMode = PickingMode.Ignore);
        agreeButton.pickingMode = PickingMode.Position;
        agreeButton.AddManipulator(new ButtonClickSoundManipulator(agreeButton));
        agreeButton.clicked += AgreeButtonPressed;
        agreeButton.RegisterCallback<ClickEvent>(evt => AgreeButtonPressed());
        agreeBtnContainer.RegisterCallback<ClickEvent>(evt => AgreeButtonPressed());

        VisualElement disagreeBtnContainer = GetElementById("DisagreeButton");
        Button disagreeButton = (Button)disagreeBtnContainer.Q<Button>("L2Button");
        disagreeButton.Query<VisualElement>().ForEach(child => child.pickingMode = PickingMode.Ignore);
        disagreeButton.pickingMode = PickingMode.Position;
        disagreeButton.AddManipulator(new ButtonClickSoundManipulator(disagreeButton));
        disagreeButton.clicked += DisagreeButtonPressed;
        disagreeButton.RegisterCallback<ClickEvent>(evt => DisagreeButtonPressed());
        disagreeBtnContainer.RegisterCallback<ClickEvent>(evt => DisagreeButtonPressed());

        var highBtn = (RepeatButton)GetElementById("unity-high-button");
        var lowBtn = (RepeatButton)GetElementById("unity-low-button");
        highBtn.AddManipulator(new ButtonClickSoundManipulator(highBtn));
        lowBtn.AddManipulator(new ButtonClickSoundManipulator(lowBtn));

        L2LoginUI.Instance.WindowLoadComplete();
    }

    private bool _agreePending = false;

    private void AgreeButtonPressed()
    {
        if (_agreePending)
        {
            return;
        }

        if (LoginClient.Instance == null || !LoginClient.Instance.IsConnected || LoginClient.Instance.ClientPacketHandler == null)
        {
            Debug.LogWarning("Cannot request server list: LoginClient is not connected.");
            return;
        }

        _agreePending = true;
        LoginClient.Instance.ClientPacketHandler.SendRequestServerList();
    }

    private void DisagreeButtonPressed()
    {
        _agreePending = false;
        LoginClient.Instance.Disconnect();
    }

    public override void ShowWindow()
    {
        _agreePending = false;
        base.ShowWindow();

        if (GameManager.Instance.AutoLogin)
        {
            StartCoroutine(AutoLogin());
        }
    }

    private IEnumerator AutoLogin()
    {
        yield return new WaitForSeconds(1f);
        AgreeButtonPressed();

    }
}
