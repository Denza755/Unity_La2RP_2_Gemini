using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginWindow : L2Window
{
    private VisualElement _logo;
    private TextField _userInput;
    private TextField _passwordInput;

    private static LoginWindow _instance;
    public static LoginWindow Instance { get { return _instance; } }

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
                ExitButtonPressed();
            }
            else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                AudioManager.Instance.PlayUISound("click_01");
                LoginButtonPressed();
            }
        }
    }

    protected override void LoadAssets()
    {
        _windowTemplate = LoadAsset("Data/UI/_Elements/Login/LoginWindow/LoginWindow");
    }

    protected override IEnumerator BuildWindow(VisualElement root)
    {
        InitWindow(root);
        _logo = root.Q<VisualElement>("L2Logo");


        yield return new WaitForEndOfFrame();

        VisualElement loginButtonContainer = GetElementById("LoginButton");
        Button loginButton = loginButtonContainer.Q<Button>("L2Button");
        loginButton.Query<VisualElement>().ForEach(child => child.pickingMode = PickingMode.Ignore);
        loginButton.pickingMode = PickingMode.Position;
        loginButton.AddManipulator(new ButtonClickSoundManipulator(loginButton));
        loginButton.clicked += LoginButtonPressed;
        loginButton.RegisterCallback<ClickEvent>(evt => LoginButtonPressed());
        loginButtonContainer.RegisterCallback<ClickEvent>(evt => LoginButtonPressed());

        VisualElement exitButtonContainer = GetElementById("ExitButton");
        Button exitButton = exitButtonContainer.Q<Button>("L2Button");
        exitButton.Query<VisualElement>().ForEach(child => child.pickingMode = PickingMode.Ignore);
        exitButton.pickingMode = PickingMode.Position;
        exitButton.AddManipulator(new ButtonClickSoundManipulator(exitButton));
        exitButton.clicked += ExitButtonPressed;
        exitButton.RegisterCallback<ClickEvent>(evt => ExitButtonPressed());
        exitButtonContainer.RegisterCallback<ClickEvent>(evt => ExitButtonPressed());

        // VisualElement userInputBg = GetElementById("UserInputBg");
        _userInput = GetElementById("UserInputField").Q<TextField>("L2Input");
        _userInput.RegisterCallback<FocusEvent>((evt) => OnInputFocus(evt, _userInput));
        _userInput.RegisterCallback<BlurEvent>((evt) => OnInputBlur(evt, _userInput));
        _userInput.maxLength = 16;
        _userInput.RegisterValueChangedCallback(OnLoginInputChanged);

        _userInput.AddManipulator(new HighlightedInputFieldManipulator(_userInput, 20));
        _userInput.AddManipulator(new BlinkingCursorManipulator(_userInput));
        _userInput.RegisterCallback<KeyDownEvent>((evt) => OnKeyPressed(evt, _userInput));

        // VisualElement passwordInputBg = GetElementById("PasswordInputBg");
        _passwordInput = GetElementById("PasswordInputField").Q<TextField>("L2Input");
        _passwordInput.RegisterCallback<FocusEvent>((evt) => OnInputFocus(evt, _passwordInput));
        _passwordInput.RegisterCallback<BlurEvent>((evt) => OnInputBlur(evt, _passwordInput));
        _passwordInput.maxLength = 16;

        _passwordInput.AddManipulator(new HighlightedInputFieldManipulator(_passwordInput, 20));
        _passwordInput.AddManipulator(new BlinkingCursorManipulator(_passwordInput));
        _passwordInput.RegisterCallback<KeyDownEvent>((evt) => OnKeyPressed(evt, _passwordInput));

        _userInput.Focus();

        L2LoginUI.Instance.WindowLoadComplete();
    }

    private void OnKeyPressed(KeyDownEvent evt, TextField input)
    {
        KeyCode keyCode = evt.keyCode;
        if (keyCode == KeyCode.Tab)
        {
            if (input == _passwordInput)
            {
                _userInput.Focus();
            }
            else
            {
                _passwordInput.Focus();
            }

            evt.StopPropagation();

        }
        else if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter)
        {
            AudioManager.Instance.PlayUISound("click_01");
            LoginButtonPressed();
            evt.StopPropagation();
        }
    }

    private void OnLoginInputChanged(ChangeEvent<string> evt)
    {
        string username = evt.newValue;

        // Regex pattern: ^[a-zA-Z0-9]{1,16}$
        // This allows only alphanumeric characters, 1-16 characters long
        Regex regex = new Regex(@"^[a-zA-Z0-9]{0,16}$");

        if (!regex.IsMatch(username))
        {
            // If the input doesn't match the regex, revert to the previous value
            _userInput.SetValueWithoutNotify(evt.previousValue);
        }
    }

    private void OnInputFocus(FocusEvent evt, VisualElement input)
    {
        if (!input.ClassListContains("highlighted"))
        {
            input.AddToClassList("highlighted");
        }
    }

    private void OnInputBlur(BlurEvent evt, VisualElement input)
    {
        if (input.ClassListContains("highlighted"))
        {
            input.RemoveFromClassList("highlighted");
        }
    }

    private void LoginButtonPressed()
    {
        string username = _userInput?.value?.Trim() ?? "";
        string password = _passwordInput?.value?.Trim() ?? "";

        if (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(_userInput?.text))
        {
            username = _userInput.text.Trim();
        }
        if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(_passwordInput?.text))
        {
            password = _passwordInput.text.Trim();
        }

        if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
        {
            username = "Shnok";
            password = "1234";
            if (_userInput != null) _userInput.value = username;
            if (_passwordInput != null) _passwordInput.value = password;
        }
        else if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Login or password cannot be empty.");
            return;
        }

        LoginClient.Instance.Account = username.ToLower();
        PlayerPrefs.SetString("LastAccountName", username.ToLower());
        PlayerPrefs.Save();
        LoginClient.Instance.Password = password;
        LoginClient.Instance.Connect();
    }

    private void ExitButtonPressed()
    {
        Application.Quit();
    }

    public void ShowLogo()
    {
        if (_logo != null)
        {
            _logo.style.display = DisplayStyle.Flex;
        }
    }

    public void HideLogo()
    {
        if (_logo != null)
        {
            _logo.style.display = DisplayStyle.None;
        }
    }

    public override void HideWindow(bool silent)
    {
        base.HideWindow(silent);

        HideLogo();
    }

    public override void ShowWindow()
    {
        base.ShowWindow();

        ShowLogo();

        _userInput.Focus();

        if (GameManager.Instance.AutoLogin)
        {
            {
                StartCoroutine(AutoLogin());
            }
        }
    }

    private IEnumerator AutoLogin()
    {
        yield return new WaitForSeconds(1f);
        LoginButtonPressed();

    }
}