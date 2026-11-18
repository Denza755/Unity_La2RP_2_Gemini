using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ViewerBootstrap : MonoBehaviour
{
    const float OrbitSensitivity = 0.18f;
    const float ZoomSensitivity = 1.25f;
    const float MinDistance = 2f;
    const float MaxDistance = 40f;
    const float MinPitch = 5f;
    const float MaxPitch = 85f;

    ViewerHud _hud;
    ViewerEffectPlayer _player;
    Camera _camera;
    GameObject _ground;

    Vector3 _orbitTarget = new Vector3(0f, 0.4f, 0f);
    float _yaw;
    float _pitch;
    float _distance = 9f;
    bool _dragging;
    Vector2 _lastMouse;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart()
    {
        if (FindFirstObjectByType<ViewerBootstrap>() != null)
            return;
        var go = new GameObject("ViewerEffects");
        DontDestroyOnLoad(go);
        go.AddComponent<ViewerBootstrap>();
    }

    void Awake()
    {
        ViewerPaths.EnsureLoaded();
        UcEffectCatalog.Refresh();
        _player = gameObject.AddComponent<ViewerEffectPlayer>();
        _hud = gameObject.AddComponent<ViewerHud>();
        _hud.Player = _player;
        _camera = Camera.main;
        _orbitTarget = new Vector3(0f, 0.4f, 0f);
        ApplyDefaultCameraPose();
        DimSceneForBrighten(0.6f);
        EnsureGround();
        ViewerStatus.Instance.Set("ожидание",
            UcEffectCatalog.Names.Count + " эффектов. UModel: " +
            (UModelExtractor.IsAvailable ? Path.GetFileName(ViewerPaths.Config.umodelExe) : "нет") +
            ". Выбери в списке.");
    }

    void ApplyDefaultCameraPose()
    {
        // Same start pose as before orbit controls existed.
        Vector3 startPos = new Vector3(0f, 4.2f, -8f);
        Vector3 offset = startPos - _orbitTarget;
        _distance = offset.magnitude;
        _yaw = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        _pitch = Mathf.Asin(Mathf.Clamp(offset.y / Mathf.Max(_distance, 1e-4f), -1f, 1f)) * Mathf.Rad2Deg;
        _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
        ApplyOrbit();
    }

    void ApplyOrbit()
    {
        if (_camera == null)
            return;
        float yawRad = _yaw * Mathf.Deg2Rad;
        float pitchRad = _pitch * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(
            Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * Mathf.Cos(pitchRad));
        _camera.transform.position = _orbitTarget + dir * _distance;
        _camera.transform.LookAt(_orbitTarget);
    }

    static void DimSceneForBrighten(float scale)
    {
        // PTDS_Brighten is dest*(1-src)+src. A bright sky/cube washes the FX white.
        RenderSettings.skybox = null;
        RenderSettings.ambientIntensity *= scale;
        RenderSettings.ambientSkyColor *= scale;
        RenderSettings.ambientEquatorColor *= scale;
        RenderSettings.ambientGroundColor *= scale;
        RenderSettings.reflectionIntensity *= scale;
        RenderSettings.sun = null;

        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.132f, 0.090f, 0.046f, 1f);
        }

        Light[] lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].intensity *= scale;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.GetComponentInParent<ViewerParticleGroup>() != null)
                continue;
            Material[] mats = renderer.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                Material mat = mats[m];
                if (mat == null)
                    continue;
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * scale);
                if (mat.HasProperty("_Color"))
                    mat.color *= scale;
            }
        }
    }

    void EnsureGround()
    {
        _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _ground.name = "ViewerGround";
        _ground.transform.position = Vector3.zero;
        _ground.transform.localScale = new Vector3(8f, 1f, 8f);
        var renderer = _ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null)
            {
                var mat = new Material(lit);
                // PTDS_Brighten depends on the destination framebuffer color.
                // Keep the viewer ground 40% darker for comparison with L2 scenes.
                mat.SetColor("_BaseColor", new Color(0.108f, 0.12f, 0.132f, 1f));
                renderer.sharedMaterial = mat;
            }
        }
    }

    void Update()
    {
        if (_camera == null)
            _camera = Camera.main;
        if (_camera == null)
            return;

        Vector2 mouse = MousePosition();
        bool overHud = _hud != null && _hud.ContainsScreenPoint(mouse);

        HandleZoom(overHud);
        HandleOrbit(mouse, overHud);
        HandlePlayClick(mouse, overHud);
    }

    void HandleZoom(bool overHud)
    {
        if (overHud)
            return;
        float scroll = ScrollDelta();
        if (Mathf.Abs(scroll) < 1e-4f)
            return;
        // Wheel forward = closer, back = farther.
        _distance = Mathf.Clamp(_distance - scroll * ZoomSensitivity, MinDistance, MaxDistance);
        ApplyOrbit();
    }

    void HandleOrbit(Vector2 mouse, bool overHud)
    {
        if (WasRightPressed() && !overHud)
        {
            _dragging = true;
            _lastMouse = mouse;
        }

        if (_dragging && IsRightHeld())
        {
            Vector2 delta = mouse - _lastMouse;
            _lastMouse = mouse;
            if (delta.sqrMagnitude > 0f)
            {
                // RMB drag: left/right yaw, up/down pitch.
                _yaw += delta.x * OrbitSensitivity;
                _pitch = Mathf.Clamp(_pitch - delta.y * OrbitSensitivity, MinPitch, MaxPitch);
                ApplyOrbit();
            }
        }

        if (_dragging && WasRightReleased())
            _dragging = false;
    }

    void HandlePlayClick(Vector2 mouse, bool overHud)
    {
        if (overHud || _dragging)
            return;
        if (!WasLeftPressed())
            return;
        TryPlayAt(mouse);
    }

    void TryPlayAt(Vector2 mouse)
    {
        if (_player == null || !_player.IsReady)
            return;
        if (_hud != null && _hud.ContainsScreenPoint(mouse))
            return;
        if (!TryPickPoint(mouse, out Vector3 point, out string hitName))
            return;
        ViewerStatus.Instance.Set("запуск", "клик → " + hitName);
        _player.PlayAt(point);
    }

    static bool WasLeftPressed()
    {
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;
        return Input.GetMouseButtonDown(0);
    }

    static bool WasRightPressed()
    {
        if (Mouse.current != null)
            return Mouse.current.rightButton.wasPressedThisFrame;
        return Input.GetMouseButtonDown(1);
    }

    static bool WasRightReleased()
    {
        if (Mouse.current != null)
            return Mouse.current.rightButton.wasReleasedThisFrame;
        return Input.GetMouseButtonUp(1);
    }

    static bool IsRightHeld()
    {
        if (Mouse.current != null)
            return Mouse.current.rightButton.isPressed;
        return Input.GetMouseButton(1);
    }

    static float ScrollDelta()
    {
        // Prefer legacy delta — reliable in Game view. New Input System scroll
        // may already be normalized or reported as ±120 per notch.
        float legacy = Input.mouseScrollDelta.y;
        if (Mathf.Abs(legacy) > 1e-4f)
            return legacy;

        if (Mouse.current == null)
            return 0f;

        float y = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(y) < 1e-4f)
            return 0f;
        if (Mathf.Abs(y) >= 2f)
            return y / 120f;
        return y;
    }

    static Vector2 MousePosition()
    {
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
        return Input.mousePosition;
    }

    bool TryPickPoint(Vector2 mouse, out Vector3 point, out string hitName)
    {
        point = Vector3.zero;
        hitName = "";
        Ray ray = _camera.ScreenPointToRay(mouse);
        float best = 200f;
        bool hit = false;

        if (Physics.Raycast(ray, out RaycastHit physicsHit, 200f, ~0, QueryTriggerInteraction.Collide))
        {
            best = physicsHit.distance;
            point = physicsHit.point;
            hitName = physicsHit.collider.name;
            hit = true;
        }

        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.GetComponent<Collider>() != null)
                continue;
            if (renderer.GetComponentInParent<ViewerParticleGroup>() != null)
                continue;
            if (!renderer.bounds.IntersectRay(ray, out float dist) || dist < 0.05f || dist >= best)
                continue;
            best = dist;
            point = ray.GetPoint(dist);
            hitName = renderer.name + " (bounds)";
            hit = true;
        }

        if (hit)
            return true;

        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float enter) && enter > 0.05f && enter < 200f)
        {
            point = ray.GetPoint(enter);
            hitName = "ground";
            return true;
        }

        point = ray.GetPoint(6f);
        hitName = "air";
        return true;
    }
}
