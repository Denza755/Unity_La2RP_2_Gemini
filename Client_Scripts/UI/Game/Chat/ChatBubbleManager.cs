using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ChatBubbleManager : MonoBehaviour
{
    private class BubbleInstance
    {
        public int ObjectId;
        public Entity Entity;
        public VisualElement BubbleElement;
        public Label TextLabel;
        public float Timer;
    }

    private static ChatBubbleManager _instance;
    public static ChatBubbleManager Instance => _instance;

    private readonly Dictionary<int, BubbleInstance> _bubbles = new();
    private readonly List<int> _keysToRemove = new();

    private Camera _mainCamera;
    private VisualElement _container;
    private const float BUBBLE_LIFETIME = 4.5f;
    private const float FADE_DURATION = 0.5f;

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
        ClearAllBubbles();
        _instance = null;
    }

    public void ShowBubble(int objectId, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        Entity entity = null;
        if (WorldSpawner.Instance != null)
        {
            entity = WorldSpawner.Instance.GetEntity(objectId);
        }

        if (entity == null && (objectId == 0 || (GameClient.Instance != null && objectId == GameClient.Instance.CurrentPlayerId)))
        {
            entity = PlayerEntity.Instance;
        }

        if (entity == null) return;

        if (!EnsureContainer()) return;

        if (_bubbles.TryGetValue(objectId, out BubbleInstance existing))
        {
            existing.TextLabel.text = text;
            existing.Timer = BUBBLE_LIFETIME;
            existing.BubbleElement.style.opacity = 1f;
            return;
        }

        VisualElement bubble = new VisualElement();
        bubble.pickingMode = PickingMode.Ignore;
        bubble.style.position = Position.Absolute;
        bubble.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(-100, LengthUnit.Percent)));
        bubble.style.backgroundColor = new Color(0.96f, 0.96f, 0.96f, 0.94f);
        bubble.style.borderTopWidth = 1f;
        bubble.style.borderBottomWidth = 1f;
        bubble.style.borderLeftWidth = 1f;
        bubble.style.borderRightWidth = 1f;
        bubble.style.borderTopColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        bubble.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        bubble.style.borderLeftColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        bubble.style.borderRightColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        bubble.style.borderTopLeftRadius = 6f;
        bubble.style.borderTopRightRadius = 6f;
        bubble.style.borderBottomLeftRadius = 6f;
        bubble.style.borderBottomRightRadius = 6f;
        bubble.style.paddingTop = 4f;
        bubble.style.paddingBottom = 5f;
        bubble.style.paddingLeft = 8f;
        bubble.style.paddingRight = 8f;
        bubble.style.maxWidth = 260f;
        bubble.style.minWidth = 30f;
        bubble.style.alignItems = Align.Center;
        bubble.style.justifyContent = Justify.Center;

        Label label = new Label(text);
        label.pickingMode = PickingMode.Ignore;
        label.style.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.fontSize = 12f;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        bubble.Add(label);

        // Triangular pointer centered at bottom
        VisualElement arrow = new VisualElement();
        arrow.pickingMode = PickingMode.Ignore;
        arrow.style.position = Position.Absolute;
        arrow.style.bottom = -4f;
        arrow.style.left = new Length(50, LengthUnit.Percent);
        arrow.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), 0));
        arrow.style.width = 7f;
        arrow.style.height = 7f;
        arrow.style.backgroundColor = new Color(0.96f, 0.96f, 0.96f, 0.94f);
        arrow.style.borderRightWidth = 1f;
        arrow.style.borderBottomWidth = 1f;
        arrow.style.borderRightColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        arrow.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f, 0.85f);
        arrow.transform.rotation = Quaternion.Euler(0, 0, 45);
        bubble.Add(arrow);

        _container.Add(bubble);

        BubbleInstance instance = new BubbleInstance
        {
            ObjectId = objectId,
            Entity = entity,
            BubbleElement = bubble,
            TextLabel = label,
            Timer = BUBBLE_LIFETIME
        };

        _bubbles[objectId] = instance;
    }

    private void LateUpdate()
    {
        if (_bubbles.Count == 0) return;

        if (_mainCamera == null)
        {
            if (CameraController.Instance != null)
            {
                _mainCamera = CameraController.Instance.GetComponent<Camera>();
            }
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            if (_mainCamera == null) return;
        }

        if (!EnsureContainer()) return;

        _keysToRemove.Clear();
        float dt = Time.deltaTime;

        foreach (var kvp in _bubbles)
        {
            BubbleInstance bubble = kvp.Value;
            if (bubble.Entity == null || bubble.Entity.gameObject == null || !bubble.Entity.gameObject.activeInHierarchy)
            {
                _keysToRemove.Add(kvp.Key);
                continue;
            }

            bubble.Timer -= dt;
            if (bubble.Timer <= 0f)
            {
                _keysToRemove.Add(kvp.Key);
                continue;
            }

            float opacity = bubble.Timer < FADE_DURATION ? Mathf.Clamp01(bubble.Timer / FADE_DURATION) : 1f;
            bubble.BubbleElement.style.opacity = opacity;

            float heightMultiplier = bubble.Entity.IsSitting ? 0.70f : 1f;
            float colHeight = bubble.Entity.Appearance != null ? bubble.Entity.Appearance.CollisionHeight : 1.1f;
            float offsetHeight = colHeight * 2.2f * heightMultiplier + 0.35f;

            Vector3 worldPos = bubble.Entity.transform.position + Vector3.up * offsetHeight;
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

            if (screenPos.z <= 0)
            {
                bubble.BubbleElement.style.display = DisplayStyle.None;
            }
            else
            {
                bubble.BubbleElement.style.display = DisplayStyle.Flex;
                bubble.BubbleElement.style.left = screenPos.x;
                bubble.BubbleElement.style.top = Screen.height - screenPos.y - 12f;
            }
        }

        foreach (int key in _keysToRemove)
        {
            if (_bubbles.TryGetValue(key, out BubbleInstance bubble))
            {
                if (_container != null && bubble.BubbleElement != null)
                {
                    _container.Remove(bubble.BubbleElement);
                }
                _bubbles.Remove(key);
            }
        }
    }

    private bool EnsureContainer()
    {
        if (_container != null) return true;

        if (L2GameUI.Instance == null || !L2GameUI.Instance.UILoaded || L2GameUI.Instance.RootElement == null)
        {
            return false;
        }

        _container = L2GameUI.Instance.RootElement.Q<VisualElement>("NameplatesContainer");
        if (_container == null)
        {
            _container = L2GameUI.Instance.RootElement;
        }

        return _container != null;
    }

    public void ClearAllBubbles()
    {
        foreach (var kvp in _bubbles)
        {
            if (_container != null && kvp.Value.BubbleElement != null)
            {
                _container.Remove(kvp.Value.BubbleElement);
            }
        }
        _bubbles.Clear();
    }
}
