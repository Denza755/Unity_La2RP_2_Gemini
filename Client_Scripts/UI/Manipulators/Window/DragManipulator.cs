using UnityEngine;
using UnityEngine.UIElements;

public class DragManipulator : PointerManipulator
{
    private VisualElement _root;
    private Vector2 _startMousePosition;
    private Vector2 _startPosition;

    private L2Window _window;

    private bool _rightAnchor = true;
    private bool _bottomAnchor = true;
    public bool dragged = false;

    public DragManipulator(VisualElement target, VisualElement root, L2Window window)
    {
        this.target = target;
        this._root = root;
        _window = window;
        UpdateAnchorType();
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler, TrickleDown.TrickleDown);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    public void PointerDownHandler(PointerDownEvent evt)
    {
        if (evt.button == 0)
        {
            dragged = false;
            _startMousePosition = evt.position;

            float currentLeft = _root.resolvedStyle.left;
            float currentTop = _root.resolvedStyle.top;
            if (float.IsNaN(currentLeft) || currentLeft == 0)
            {
                currentLeft = _root.layout.position.x;
            }
            if (float.IsNaN(currentTop) || currentTop == 0)
            {
                currentTop = _root.layout.position.y;
            }

            _startPosition = new Vector2(currentLeft, currentTop);
            target.CapturePointer(evt.pointerId);
        }
    }

    public void PointerMoveHandler(PointerMoveEvent evt)
    {
        if (target.HasPointerCapture(evt.pointerId))
        {
            dragged = true;
            Vector2 delta = new Vector2(evt.position.x - _startMousePosition.x, evt.position.y - _startMousePosition.y);
            DragWindow(delta);
        }
        evt.StopPropagation();
    }

    public void PointerUpHandler(PointerUpEvent evt)
    {
        if (target.HasPointerCapture(evt.pointerId))
        {
            target.ReleasePointer(evt.pointerId);
            if (!dragged && _window != null)
            {
                _window.OnClick();
            }
        }
    }

    // Apply drag smoothly using left and top offsets
    private void DragWindow(Vector2 delta)
    {
        float parentWidth = (_root.parent != null && _root.parent.layout.width > 0) ? _root.parent.layout.width : Screen.width;
        float parentHeight = (_root.parent != null && _root.parent.layout.height > 0) ? _root.parent.layout.height : Screen.height;

        float width = _root.layout.width > 0 ? _root.layout.width : 250f;
        float height = _root.layout.height > 0 ? _root.layout.height : 300f;

        float newLeft = Mathf.Clamp(_startPosition.x + delta.x, 0, Mathf.Max(0, parentWidth - width));
        float newTop = Mathf.Clamp(_startPosition.y + delta.y, 0, Mathf.Max(0, parentHeight - height));

        _root.style.position = Position.Absolute;
        _root.style.left = newLeft;
        _root.style.top = newTop;
        _root.style.right = StyleKeyword.Null;
        _root.style.bottom = StyleKeyword.Null;
    }

    // Update the anchor based on the window position ratio
    private void UpdateAnchorType()
    {
        _rightAnchor = _root.layout.position.x + (_root.layout.width / 2f) >= Screen.width / 2f;
        _bottomAnchor = _root.layout.position.y + (_root.layout.height / 2f) >= Screen.height / 2f;
    }
}
