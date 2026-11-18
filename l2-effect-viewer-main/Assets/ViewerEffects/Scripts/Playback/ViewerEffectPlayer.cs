using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ViewerPreparedEmitter
{
    public ViewerEmitterData Data;
    public Mesh Mesh;
    public Material Material;
    public string Warning;
}

public sealed class ViewerEffectPlayer : MonoBehaviour
{
    public ViewerEffectData Current { get; private set; }
    public readonly List<ViewerPreparedEmitter> Prepared = new List<ViewerPreparedEmitter>();
    public bool IsReady => Prepared.Count > 0 && !ViewerStatus.Instance.Busy;
    GameObject _activeRoot;

    public IEnumerator Prepare(string effectName)
    {
        ViewerStatus.Instance.Busy = true;
        ViewerStatus.Instance.SelectedEffect = effectName;
        ViewerStatus.Instance.Set("создание", effectName);
        Prepared.Clear();
        Current = null;

        if (!UcEffectCatalog.TryGetPath(effectName, out string path))
        {
            ViewerStatus.Instance.Set("ошибка", "UC не найден: " + effectName);
            ViewerStatus.Instance.Busy = false;
            yield break;
        }

        Debug.Log("[ViewerEffects] UC source: " + path);

        ViewerEffectData effect;
        try
        {
            effect = UcEffectParser.ParseFile(path);
        }
        catch (Exception ex)
        {
            ViewerStatus.Instance.Set("ошибка", ex.Message);
            ViewerStatus.Instance.Busy = false;
            yield break;
        }

        Current = effect;
        ViewerDiagnostics.Begin(effect);
        if (effect.Emitters.Count == 0)
        {
            ViewerStatus.Instance.Set("ошибка", "В UC нет Emitter блоков");
            ViewerStatus.Instance.Busy = false;
            yield break;
        }

        foreach (ViewerEmitterData emitter in effect.Emitters)
        {
            ViewerDiagnostics.Emitter(emitter);
            ViewerStatus.Instance.Set("создание", emitter.Kind + " " + emitter.DisplayName);
            yield return null;
            var prepared = new ViewerPreparedEmitter { Data = emitter };
            try
            {
                if (emitter.Kind == ViewerEmitterKind.Trail ||
                    emitter.Kind == ViewerEmitterKind.Spark)
                {
                    prepared.Warning = emitter.Kind + " пока играется как sprite, если есть Texture";
                    if (string.IsNullOrEmpty(emitter.Texture))
                    {
                        prepared.Warning = emitter.Kind + " пропущен — нет Texture";
                        Prepared.Add(prepared);
                        continue;
                    }
                    emitter.Kind = ViewerEmitterKind.Sprite;
                }

                if (emitter.Kind == ViewerEmitterKind.Beam && string.IsNullOrEmpty(emitter.Texture))
                {
                    prepared.Warning = "Beam пропущен — нет Texture";
                    Prepared.Add(prepared);
                    continue;
                }

                ViewerStatus.Instance.Set("конвертация", emitter.DisplayName);
                ViewerResolvedAssets assets = ViewerAssetResolver.Resolve(effect, emitter);
                if (emitter.Kind == ViewerEmitterKind.Beam)
                {
                    prepared.Mesh = ViewerBeamBuilder.BuildStripMesh(emitter);
                    prepared.Warning = "beam HF=" + emitter.HighFrequencyPoints +
                                       " " + emitter.DetermineEndPointBy;
                }
                else
                {
                    prepared.Mesh = assets.Mesh;
                    if (!string.IsNullOrEmpty(assets.MeshSource))
                        prepared.Warning = "mesh " + assets.MeshSource;
                }
                ViewerDiagnostics.Assets(emitter, assets);
                prepared.Material = ViewerMaterialFactory.Create(effect, emitter, assets.Texture);
            }
            catch (Exception ex)
            {
                prepared.Warning = ex.Message;
                ViewerDiagnostics.Error(emitter.DisplayName, ex);
                Debug.LogWarning("[ViewerEffects] " + emitter.DisplayName + ": " + ex);
            }
            Prepared.Add(prepared);
            yield return null;
        }

        int ok = 0;
        foreach (var p in Prepared)
        {
            if (p.Material != null)
                ok++;
        }
        ViewerStatus.Instance.Busy = false;
        ViewerStatus.Instance.Set("запуск",
            ok + "/" + Prepared.Count + " слоёв готовы. Клик по земле — проиграть.");
    }

    const float FloorLiftMeters = 0.2f;

    public void PlayAt(Vector3 worldPos)
    {
        if (Prepared.Count == 0)
            return;

        Vector3 spawnPos = new Vector3(worldPos.x, worldPos.y + FloorLiftMeters, worldPos.z);
        ViewerDiagnostics.Play(spawnPos);
        if (_activeRoot != null)
            Destroy(_activeRoot);
        var root = new GameObject(Current != null ? Current.ClassName : "Effect");
        _activeRoot = root;
        root.transform.position = spawnPos;
        float maxLife = 0.2f;
        foreach (ViewerPreparedEmitter prepared in Prepared)
        {
            if (prepared.Material == null)
                continue;
            ViewerParticleGroup.Spawn(
                root.transform,
                prepared.Data.DisplayName,
                prepared.Data,
                prepared.Mesh,
                prepared.Material,
                spawnPos);
            maxLife = Mathf.Max(maxLife, prepared.Data.MaxLifetime);
        }
        Destroy(root, maxLife + 2f);
        ViewerStatus.Instance.Set("запуск",
            (Current != null ? Current.ClassName : "effect") + " @ " + spawnPos.ToString("F2"));
    }
}
