using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ModelTable
{
    private static ModelTable _instance;
    public static ModelTable Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ModelTable();
            }

            return _instance;
        }
    }

    public static int RACE_COUNT = 14;
    private static int FACE_COUNT = 6;
    private static int HAIR_STYLE_COUNT = 6;
    private static int HAIR_COLOR_COUNT = 4;

    private GameObject[] _playerContainers;
    private GameObject[] _userContainers;
    private GameObject[] _pawnContainers;
    private Material[,] _faceMaterials;
    private GameObject[] _faceModels;
    private Material[,,] _hairMaterials;
    private GameObject[,] _hairModels;
    private Dictionary<string, GameObject> _weapons;
    private Dictionary<string, GameObject> _items;
    private Dictionary<string, GameObject> _npcs;
    private Dictionary<string, L2Armor> _armors;
    private readonly HashSet<GameObject> _fallbackWeaponPrefabs = new HashSet<GameObject>();
    private class L2Armor
    {
        public GameObject baseModel;
        public Dictionary<string, Material> materials;
    }

    public class L2ArmorPiece
    {
        public GameObject baseArmorModel;
        public Material material;

        public L2ArmorPiece(GameObject baseArmorModel, Material material)
        {
            this.baseArmorModel = baseArmorModel;
            this.material = material;
        }
    }

    public void Initialize()
    {
        CachePlayerContainers();
        CacheFaces();
        CacheHair();
        CacheWeapons();
        CacheArmors();
        CacheNpcs();
        CacheItemModels();
    }

    // private void OnDestroy() {
    //     _faces = null;
    //     _hair = null;
    //     _instance = null;
    //     _playerContainers = null;
    //     _userContainers = null;
    //     _weapons.Clear();
    //     _armors.Clear();
    //     _instance = null;
    // }

    // -------
    // CACHE
    // -------
    private void CachePlayerContainers()
    {
        _playerContainers = new GameObject[RACE_COUNT];
        _userContainers = new GameObject[RACE_COUNT];
        _pawnContainers = new GameObject[RACE_COUNT];

        // Player Containers
        for (int r = 0; r < RACE_COUNT; r++)
        {
            CharacterModelType raceId = (CharacterModelType)r;
            CharacterRace race = CharacterRaceParser.ParseRace(raceId);

            string path = $"Data/Animations/{race}/{raceId}/Player_{raceId}";
            _playerContainers[r] = Resources.Load<GameObject>(path);
            //   Debug.Log($"Loading player container {r} [{path}]");
        }

        // User Containers
        for (int r = 0; r < RACE_COUNT; r++)
        {
            CharacterModelType raceId = (CharacterModelType)r;
            CharacterRace race = CharacterRaceParser.ParseRace(raceId);

            string path = $"Data/Animations/{race}/{raceId}/User_{raceId}";
            _userContainers[r] = Resources.Load<GameObject>(path);
            //  Debug.Log($"Loading user container {r} [{path}]");
        }

        // Pawn Containers
        for (int r = 0; r < RACE_COUNT; r++)
        {
            CharacterModelType raceId = (CharacterModelType)r;
            CharacterRace race = CharacterRaceParser.ParseRace(raceId);

            string path = $"Data/Animations/{race}/{raceId}/Pawn_{raceId}";
            _pawnContainers[r] = Resources.Load<GameObject>(path);
            //  Debug.Log($"Loading user container {r} [{path}]");
        }
    }

    private void CacheFaces()
    {
        _faceModels = new GameObject[RACE_COUNT]; // there is 14 races, each race has 1 face model
        _faceMaterials = new Material[RACE_COUNT, FACE_COUNT]; // there is 14 races, each race has 6 face materials

        // Faces
        for (int r = 0; r < RACE_COUNT; r++)
        {
            CharacterModelType raceId = (CharacterModelType)r;
            CharacterRace race = CharacterRaceParser.ParseRace(raceId);

            if (ChargrpTable.Instance.CharGrps.TryGetValue(raceId, out Chargrp chargrp))
            {
                GameObject faceModel = LoadFaceModel(chargrp.FaceMesh);
                if (faceModel == null)
                {
                    continue;
                }

                _faceModels[r] = faceModel;

                for (int i = 0; i < chargrp.FaceTextures.Length; i++) //Hardcoded to 3 for now
                {
                    _faceMaterials[r, i] = LoadFaceMaterial(chargrp.FaceTextures[i]);
                }
            }
        }
    }

    private GameObject LoadFaceModel(string model)
    {
        string[] folderFile = model.Split(".");

        if (folderFile.Length < 2) return null;

        string modelPath = $"Data/Animations/{folderFile[0]}/{folderFile[1]}";

        GameObject face = (GameObject)Resources.Load(modelPath);
        if (face == null)
        {
            string raceFolder = GetRaceFolderFromModelOrTexture(folderFile[0], folderFile[1]);
            if (!string.IsNullOrEmpty(raceFolder))
            {
                face = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{folderFile[1]}");
                if (face == null)
                {
                    face = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{folderFile[1].ToLowerInvariant()}");
                }
            }
        }

        return face;
    }

    private Material LoadFaceMaterial(string texture)
    {
        string[] folderFile = texture.Split(".");

        if (folderFile.Length < 3) return null;

        string materialPath = $"Data/SysTextures/LineageFaceTex/Materials/{folderFile[2]}";

        Material material = (Material)Resources.Load(materialPath);
        if (material == null)
        {
            string raceFolder = GetRaceFolderFromModelOrTexture(folderFile[1], folderFile[2]);
            if (!string.IsNullOrEmpty(raceFolder))
            {
                material = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{folderFile[2]}");
                if (material == null)
                {
                    material = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{folderFile[2].ToLowerInvariant()}");
                }
            }
        }

        return material;
    }

    private void CacheHair()
    {
        _hairModels = new GameObject[RACE_COUNT, HAIR_STYLE_COUNT * 2]; // there is 14 races, each race has 6 hairstyle (2 models each) of 4 colors
        _hairMaterials = new Material[RACE_COUNT, HAIR_STYLE_COUNT * 2, HAIR_COLOR_COUNT]; // there is 14 races, each race has 6 hairstyle (2 models each) of 4 colors

        // Hair
        for (int r = 0; r < RACE_COUNT; r++)
        {
            CharacterModelType raceId = (CharacterModelType)r;
            CharacterRace race = CharacterRaceParser.ParseRace(raceId);


            if (ChargrpTable.Instance.CharGrps.TryGetValue(raceId, out Chargrp chargrp))
            {
                for (int hs = 0; hs < HAIR_STYLE_COUNT; hs++)
                {
                    GameObject ahModel = LoadHairModel(chargrp.HairStyles[hs].AhModel);
                    GameObject bhModel = LoadHairModel(chargrp.HairStyles[hs].BhModel);

                    if (ahModel == null && bhModel == null)
                    {
                        continue;
                    }

                    _hairModels[r, hs * 2] = ahModel;
                    _hairModels[r, hs * 2 + 1] = bhModel;

                    for (int hc = 0; hc < HAIR_COLOR_COUNT; hc++)
                    {
                        _hairMaterials[r, hs * 2, hc] = LoadHairMaterial(chargrp.HairStyles[hs].AhTextures[hc]);
                        _hairMaterials[r, hs * 2 + 1, hc] = LoadHairMaterial(chargrp.HairStyles[hs].BhTextures[hc]);
                    }
                }
            }

        }
    }

    private GameObject LoadHairModel(string model)
    {
        if (model == null)
        {
            return null;
        }

        string[] folderFile = model.Split(".");

        if (folderFile.Length < 2) return null;

        string modelPath = $"Data/Animations/{folderFile[0]}/{folderFile[1]}";

        GameObject hair = (GameObject)Resources.Load(modelPath);
        if (hair == null)
        {
            string raceFolder = GetRaceFolderFromModelOrTexture(folderFile[0], folderFile[1]);
            if (!string.IsNullOrEmpty(raceFolder))
            {
                hair = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{folderFile[1]}");
                if (hair == null)
                {
                    hair = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{folderFile[1].ToLowerInvariant()}");
                }
            }
        }

        return hair;
    }

    private Material LoadHairMaterial(string texture)
    {
        if (texture == null)
        {
            return null;
        }

        string[] folderFile = texture.Split(".");

        if (folderFile.Length < 2) return null;

        string materialPath = $"Data/SysTextures/{folderFile[0]}/Materials/{folderFile[1]}";

        Material material = (Material)Resources.Load(materialPath);
        if (material == null)
        {
            string raceFolder = GetRaceFolderFromModelOrTexture(folderFile[0], folderFile[1]);
            if (!string.IsNullOrEmpty(raceFolder))
            {
                material = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{folderFile[1]}");
                if (material == null)
                {
                    material = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{folderFile[1].ToLowerInvariant()}");
                }
            }
        }

        return material;
    }

    private void CacheWeapons()
    {
        _weapons = new Dictionary<string, GameObject>();
        int success = 0;
        foreach (KeyValuePair<int, Weapon> kvp in ItemTable.Instance.Weapons)
        {
            if (kvp.Value == null || kvp.Value.Weapongrp == null || kvp.Value.Weapongrp.Models == null)
            {
                continue;
            }

            foreach (string model in kvp.Value.Weapongrp.Models)
            {
                if (string.IsNullOrEmpty(model) || _weapons.ContainsKey(model))
                {
                    continue;
                }

                GameObject weapon = LoadWeaponModel(model);
                if (weapon != null)
                {
                    success++;
                    _weapons[model] = weapon;
                }
            }
        }

        Debug.Log($"Successfully loaded {success}/{ItemTable.Instance.Weapons.Count} weapon model(s).");
    }

    private static readonly Dictionary<string, string> _weaponModelAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "cursed_staff_m00_wp", "cursed_staff_m00_wp_" },
            { "maingauche_m00_wp", "maingauche_m00_wp_" },
            { "skull_graver_m00_wp", "skull_graver_m00_wp01" },
            { "silenos_blowgun_m00_wp", "silenos_blowgun_a_m00_wp" },
            { "shield_of_solar_eclipse_m00_sh", "shield_of_sola_eclipse_m00_sh" },
            { "spirits_staff_m00_wp", "Spirites_staff_m00_wp" },
            { "knights_sword_m00_wp", "knight_sword_m00_wp" },
            { "dark_elven_bow_m00_wp", "darkelven_bow_m00_wp" },
        };

    private GameObject LoadWeaponModel(string model)
    {
        if (string.IsNullOrEmpty(model)) return null;

        string[] folderFile = model.Split('.');
        string file = folderFile.Length > 1 ? folderFile[1] : model;
        string folder = folderFile.Length > 1 ? folderFile[0] : "";

        GameObject loaded = TryLoadWeaponCandidate(folder, file);
        if (loaded != null) return loaded;

        if (_weaponModelAliases.TryGetValue(file, out string alias))
        {
            loaded = TryLoadWeaponCandidate(folder, alias);
            if (loaded != null) return loaded;
        }

        return null;
    }

    /// <summary>Returns true if the weapon/item prefab was loaded from the assets/models/weapons fallback folder.</summary>
    public bool IsFallbackWeaponPrefab(GameObject prefab)
    {
        return prefab != null && _fallbackWeaponPrefabs.Contains(prefab);
    }

    private GameObject TryLoadWeaponCandidate(string folder, string file)
    {
        string lowerFile = file.ToLowerInvariant();
        string animFolder = string.IsNullOrEmpty(folder) ? "LineageWeapons" : folder;

        // Data/Animations/{folder} models are pre-scaled (globalScale=0.019) to match 100x hand bones.
        string[] legacyPaths =
        {
            $"Data/Animations/{animFolder}/{file}",
            $"Data/Animations/{animFolder}/{lowerFile}",
            $"Data/Animations/{animFolder}/Models/{file}",
            $"Data/Animations/{animFolder}/Models/{lowerFile}",
        };

        foreach (string path in legacyPaths)
        {
            GameObject legacy = Resources.Load<GameObject>(path);
            if (legacy != null) return legacy;
        }

        // Fallback only for models missing from Data/Animations/LineageWeapons (~133.68x larger local bounds).
        string[] weaponPaths =
        {
            $"assets/models/weapons/{file}/{file}_main",
            $"assets/models/weapons/{file}/{file}",
            $"assets/models/weapons/{file}",
            $"assets/models/weapons/{lowerFile}/{lowerFile}_main",
            $"assets/models/weapons/{lowerFile}/{lowerFile}",
        };

        foreach (string path in weaponPaths)
        {
            GameObject weapon = Resources.Load<GameObject>(path);
            if (weapon != null)
            {
                _fallbackWeaponPrefabs.Add(weapon);
                return weapon;
            }
        }

        return null;
    }

    private void CacheItemModels()
    {
        _items = new Dictionary<string, GameObject>();
        int success = 0;
        foreach (KeyValuePair<int, EtcItem> kvp in ItemTable.Instance.EtcItems)
        {
            if (kvp.Value.EtcItemgrp.Mesh != null)
            {
                if (_weapons.ContainsKey(kvp.Value.EtcItemgrp.Mesh))
                {
                    continue;
                }

                GameObject itemMesh = LoadWeaponModel(kvp.Value.EtcItemgrp.Mesh);
                if (itemMesh != null)
                {
                    success++;
                    _items[kvp.Value.EtcItemgrp.Mesh] = itemMesh;
                }
            }
        }

        Debug.Log($"Successfully loaded {success} item model(s).");
    }

    private void CacheArmors()
    {
        _armors = new Dictionary<string, L2Armor>();

        int armorMaterials = 0;
        foreach (KeyValuePair<int, Armor> kvp in ItemTable.Instance.Armors)
        {
            if (kvp.Value == null || kvp.Value.Armorgrp == null || kvp.Value.Armorgrp.Model == null)
            {
                continue;
            }

            for (int i = 0; i < RACE_COUNT; i++)
            {
                if (kvp.Value.Armorgrp.Model.Length <= i) continue;
                string model = kvp.Value.Armorgrp.Model[i];
                if (string.IsNullOrEmpty(model))
                {
                    continue;
                }

                L2Armor l2Model = null;
                if (!_armors.ContainsKey(model))
                {
                    l2Model = new L2Armor();
                    l2Model.baseModel = LoadArmorModel(model);
                    if (l2Model.baseModel != null)
                    {
                        l2Model.materials = new Dictionary<string, Material>();
                        _armors.Add(model, l2Model);
                    }
                }

                if (l2Model == null)
                {
                    _armors.TryGetValue(model, out l2Model);
                }

                if (l2Model == null || l2Model.baseModel == null)
                {
                    continue;
                }

                if (kvp.Value.Armorgrp.Texture == null || kvp.Value.Armorgrp.Texture.Length <= i)
                {
                    continue;
                }

                string texture = kvp.Value.Armorgrp.Texture[i];
                if (string.IsNullOrEmpty(texture) || l2Model.materials.ContainsKey(texture))
                {
                    continue;
                }

                Material armorMaterial = LoadArmorMaterial(texture);
                if (armorMaterial == null)
                {
                    continue;
                }

                l2Model.materials.Add(texture, armorMaterial);
                armorMaterials++;
            }
        }

        Debug.Log($"Successfully loaded {_armors.Count} armor model(s).");
        Debug.Log($"Successfully loaded {armorMaterials} armor material(s).");
    }

    private string GetRaceFolderFromModelOrTexture(string folder, string file)
    {
        if (string.IsNullOrEmpty(file)) return null;

        // Check file prefix against CharacterModelType enum names
        foreach (CharacterModelType r in Enum.GetValues(typeof(CharacterModelType)))
        {
            string rName = r.ToString();
            if (file.StartsWith(rName, StringComparison.OrdinalIgnoreCase))
            {
                return rName;
            }
        }

        // Map folder if provided
        if (!string.IsNullOrEmpty(folder))
        {
            switch (folder.ToLowerInvariant())
            {
                case "orc":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FOrc" : "MOrc";
                case "shaman":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FShaman" : "MShaman";
                case "fighter":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FFighter" : "MFighter";
                case "magic":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FMagic" : "MMagic";
                case "elf":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FElf" : "MElf";
                case "darkelf":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FDarkElf" : "MDarkElf";
                case "dwarf":
                    return file.StartsWith("f", StringComparison.OrdinalIgnoreCase) ? "FDwarf" : "MDwarf";
            }
        }

        return null;
    }

    private GameObject LoadArmorModel(string model)
    {
        if (string.IsNullOrEmpty(model)) return null;

        string[] folderFile = model.Split('.');
        string file = folderFile.Length > 1 ? folderFile[1] : model;
        string folder = folderFile.Length > 1 ? folderFile[0] : "";

        // 1. Legacy Data/Animations path
        if (!string.IsNullOrEmpty(folder))
        {
            GameObject legacy = Resources.Load<GameObject>($"Data/Animations/{folder}/{file}");
            if (legacy != null) return legacy;
        }

        // 2. Race-specific parts folder
        string raceFolder = GetRaceFolderFromModelOrTexture(folder, file);
        if (!string.IsNullOrEmpty(raceFolder))
        {
            GameObject piece = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{file}");
            if (piece != null) return piece;

            piece = Resources.Load<GameObject>($"assets/models/characters/{raceFolder}/parts/{file.ToLowerInvariant()}");
            if (piece != null) return piece;
        }

        // 3. Fallback across all character folders
        foreach (CharacterModelType r in Enum.GetValues(typeof(CharacterModelType)))
        {
            string rName = r.ToString();
            GameObject piece = Resources.Load<GameObject>($"assets/models/characters/{rName}/parts/{file.ToLowerInvariant()}");
            if (piece != null) return piece;
        }

        return null;
    }

    private Material LoadArmorMaterial(string texture)
    {
        if (string.IsNullOrEmpty(texture)) return null;

        string[] folderFile = texture.Split('.');
        string file = folderFile.Length > 1 ? folderFile[1] : texture;
        string folder = folderFile.Length > 1 ? folderFile[0] : "";

        // Check alternate _sp variation
        string fileAltSp = file.EndsWith("_sp", StringComparison.OrdinalIgnoreCase)
            ? file.Substring(0, file.Length - 3)
            : file + "_sp";

        // 1. Legacy Data/SysTextures path
        if (!string.IsNullOrEmpty(folder))
        {
            Material legacy = Resources.Load<Material>($"Data/SysTextures/{folder}/Materials/{file}");
            if (legacy != null) return legacy;

            legacy = Resources.Load<Material>($"Data/SysTextures/{folder}/Materials/{fileAltSp}");
            if (legacy != null) return legacy;
        }

        // 2. Race-specific materials folder
        string raceFolder = GetRaceFolderFromModelOrTexture(folder, file);
        if (!string.IsNullOrEmpty(raceFolder))
        {
            Material mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{file}");
            if (mat != null) return mat;

            mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{file.ToLowerInvariant()}");
            if (mat != null) return mat;

            // Check with/without _sp suffix
            mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{fileAltSp}")
               ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{fileAltSp.ToLowerInvariant()}");
            if (mat != null) return mat;

            string lowerRace = raceFolder.ToLowerInvariant();
            string slotSuffix = "_u";
            if (file.EndsWith("_l", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_l";
            else if (file.EndsWith("_g", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_g";
            else if (file.EndsWith("_b", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_b";

            // If an m001 material was requested (e.g. t01 variant), try t02 first!
            if (file.IndexOf("_m001_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string t02Variant = file.Replace("_t01_", "_t02_").Replace("_t00_", "_t02_");
                mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{t02Variant}")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{t02Variant.ToLowerInvariant()}")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m001_t02{slotSuffix}")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m001_t02{slotSuffix}");
                if (mat != null) return mat;

                string t01Variant = file.Replace("_t02_", "_t01_").Replace("_t00_", "_t01_");
                mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{t01Variant}")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{t01Variant.ToLowerInvariant()}")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m001_t01{slotSuffix}")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m001_t01{slotSuffix}");
                if (mat != null) return mat;
            }

            // If m000 (naked) requested
            if (file.IndexOf("_m000_", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m000_t1000{slotSuffix}")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m000_t1000{slotSuffix}");
                if (mat != null) return mat;
            }

            // Glove fallback guarantee for the race
            if (slotSuffix == "_g")
            {
                mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m000_t1000_g")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m001_t01_g")
                   ?? Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m001_t02_g")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m000_t1000_g")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m001_t01_g")
                   ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m001_t02_g");
                if (mat != null) return mat;
            }
        }

        // 3. Fallback across all character folders ONLY if race is unknown (never cross-contaminate Orc or known race!)
        if (string.IsNullOrEmpty(raceFolder))
        {
            foreach (CharacterModelType r in Enum.GetValues(typeof(CharacterModelType)))
            {
                string rName = r.ToString();
                Material mat = Resources.Load<Material>($"assets/models/characters/{rName}/materials/{file.ToLowerInvariant()}")
                            ?? Resources.Load<Material>($"assets/models/characters/{rName}/materials/{fileAltSp.ToLowerInvariant()}");
                if (mat != null) return mat;
            }
        }

        return null;
    }

    private void CacheNpcs()
    {
        _npcs = new Dictionary<string, GameObject>();
        int success = 0;
        foreach (KeyValuePair<int, Npcgrp> kvp in NpcgrpTable.Instance.Npcgrps)
        {
            if (_npcs.ContainsKey(kvp.Value.Mesh))
            {
                continue;
            }

            GameObject npc = LoadNpc(kvp.Value.Mesh);
            if (npc != null)
            {
                success++;
                _npcs[kvp.Value.Mesh] = npc;
            }
        }

        Debug.Log($"Loaded {success} npc model(s).");
    }

    private GameObject LoadNpc(string meshname)
    {
        string[] folderFile = meshname.Split(".");

        if (folderFile.Length < 2)
        {
            return null;
        }

        string path = $"Data/Animations/{folderFile[0]}/{folderFile[1]}/{folderFile[1]}";

        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null && folderFile[1].Equals("fox_m00", System.StringComparison.OrdinalIgnoreCase))
        {
            prefab = Resources.Load<GameObject>($"Data/Animations/{folderFile[0]}/keltir_m00/keltir_m00");
        }

        return prefab;
    }

    // -------
    // Getters
    // -------
    public L2ArmorPiece GetArmorPiece(Armor armor, CharacterModelType raceId)
    {
        if (armor == null || armor.Armorgrp == null)
        {
            Debug.LogWarning($"Given armor is null");
            return null;
        }

        if (armor.Armorgrp.Model == null || armor.Armorgrp.Model.Length <= (byte)raceId)
        {
            return null;
        }

        string model = armor.Armorgrp.Model[(byte)raceId];
        if (string.IsNullOrEmpty(model))
        {
            return null;
        }

        if (!_armors.TryGetValue(model, out L2Armor l2Armor) || l2Armor == null)
        {
            GameObject loadedModel = LoadArmorModel(model);
            if (loadedModel != null)
            {
                l2Armor = new L2Armor
                {
                    baseModel = loadedModel,
                    materials = new Dictionary<string, Material>()
                };
                _armors[model] = l2Armor;
            }
        }

        if (l2Armor == null || l2Armor.baseModel == null)
        {
            string texStr = (armor.Armorgrp.Texture != null && armor.Armorgrp.Texture.Length > (byte)raceId) ? armor.Armorgrp.Texture[(byte)raceId] : "unknown";
            Debug.LogWarning($"Can't find armor model {model} for {raceId} in ModelTable. Material is {texStr}.");
            return null;
        }

        string texture = (armor.Armorgrp.Texture != null && armor.Armorgrp.Texture.Length > (byte)raceId)
            ? armor.Armorgrp.Texture[(byte)raceId] : null;

        Material material = null;
        if (!string.IsNullOrEmpty(texture))
        {
            if (l2Armor.materials != null && l2Armor.materials.TryGetValue(texture, out Material cachedMat))
            {
                material = cachedMat;
            }
            else
            {
                material = LoadArmorMaterial(texture);
                if (material != null && l2Armor.materials != null)
                {
                    l2Armor.materials[texture] = material;
                }
            }
        }

        string raceFolder = raceId.ToString();
        string lowerRace = raceFolder.ToLowerInvariant();
        string slotSuffix = "_u";
        if (model.EndsWith("_l", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_l";
        else if (model.EndsWith("_g", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_g";
        else if (model.EndsWith("_b", StringComparison.OrdinalIgnoreCase)) slotSuffix = "_b";

        string meshSeries = null;
        Match mMatch = Regex.Match(model, @"_m(\d{3})", RegexOptions.IgnoreCase);
        if (mMatch.Success)
        {
            meshSeries = mMatch.Groups[1].Value;
        }
        else if (l2Armor.baseModel != null)
        {
            mMatch = Regex.Match(l2Armor.baseModel.name, @"_m(\d{3})", RegexOptions.IgnoreCase);
            if (mMatch.Success)
            {
                meshSeries = mMatch.Groups[1].Value;
            }
        }

        bool isNakedMesh = (meshSeries == "000");

        if (isNakedMesh)
        {
            if (material == null || !material.name.Contains("_m000_") || material.name.StartsWith("Default-") || (material.mainTexture == null && !material.HasProperty("_BaseMap")))
            {
                material = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m000_t1000{slotSuffix}");
            }
            return new L2ArmorPiece(l2Armor.baseModel, material);
        }

        // Non-naked mesh (m001, m002, m003, ...):
        // Verify material series matches mesh series and has valid texture
        bool matMatchesMesh = material != null
            && !material.name.Contains("_m000_t1000")
            && !material.name.StartsWith("Default-")
            && (material.mainTexture != null || (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null))
            && (string.IsNullOrEmpty(meshSeries) || material.name.IndexOf($"_m{meshSeries}_", StringComparison.OrdinalIgnoreCase) >= 0);

        // If not matching, try fallback within the same series
        if (!matMatchesMesh && meshSeries == "001")
        {
            Material m001Mat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m001_t02{slotSuffix}")
                            ?? Resources.Load<Material>($"Data/SysTextures/{raceFolder}/Materials/{raceFolder}_m001_t02{slotSuffix}");
            if (m001Mat != null)
            {
                material = m001Mat;
                matMatchesMesh = true;
            }
        }

        // If STILL not matching, NEVER put m000_t1000 or wrong series texture on m00X mesh!
        // Synchronously roll back BOTH mesh and material to naked body (m000 + m000_t1000)
        if (!matMatchesMesh)
        {
            GameObject nakedModel = LoadArmorModel($"{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Orc.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Shaman.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Fighter.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Magic.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Elf.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"DarkElf.{raceFolder}_m000{slotSuffix}")
                                 ?? LoadArmorModel($"Dwarf.{raceFolder}_m000{slotSuffix}");
            Material nakedMat = Resources.Load<Material>($"assets/models/characters/{raceFolder}/materials/{lowerRace}_m000_t1000{slotSuffix}");
            if (nakedModel != null && nakedMat != null)
            {
                Debug.LogWarning($"Series mismatch or missing material for {model} (mesh series m{meshSeries}) for {raceFolder}. Synchronously rolling back both mesh and material to naked body.");
                return new L2ArmorPiece(nakedModel, nakedMat);
            }
        }

        L2ArmorPiece armorModel = new L2ArmorPiece(l2Armor.baseModel, material);
        return armorModel;
    }

    public L2ArmorPiece GetArmorPieceByItemId(int itemId, CharacterModelType raceId)
    {
        Armor armor = ItemTable.Instance.GetArmor(itemId);

        return GetArmorPiece(armor, raceId);
    }

    public GameObject[] GetWeaponsById(int itemId)
    {
        Weapon weapon = ItemTable.Instance.GetWeapon(itemId);
        if (weapon == null || weapon.Weapongrp == null || weapon.Weapongrp.Models == null)
        {
            Debug.LogWarning($"Can't find weapon or models for {itemId} in ItemTable");
            return null;
        }

        GameObject[] models = new GameObject[weapon.Weapongrp.Models.Length];

        for (int i = 0; i < models.Length; i++)
        {
            string modelName = weapon.Weapongrp.Models[i];
            models[i] = GetWeapon(modelName);
            if (models[i] == null)
            {
                models[i] = LoadWeaponModel(modelName);
                if (models[i] != null)
                {
                    _weapons[modelName] = models[i];
                }
            }
        }

        return models;
    }

    public GameObject GetWeapon(string model)
    {
        if (string.IsNullOrEmpty(model)) return null;

        if (_weapons.TryGetValue(model, out GameObject go))
        {
            return go;
        }
        else
        {
            GameObject loaded = LoadWeaponModel(model);
            if (loaded != null)
            {
                _weapons[model] = loaded;
                return loaded;
            }
            Debug.LogWarning($"Can't find weapon model {model} in ModelTable");
            return null;
        }
    }

    public GameObject GetItemModelById(int itemId)
    {
        EtcItem item = ItemTable.Instance.GetEtcItem(itemId);
        if (item == null)
        {
            Debug.LogWarning($"Can't find etcitem {itemId} in ItemTable");
        }

        return GetItemModel(item.EtcItemgrp.Mesh);
    }

    public GameObject GetItemModel(string model)
    {
        if (_items.TryGetValue(model, out GameObject go))
        {
            return go;
        }
        else
        {
            Debug.LogWarning($"Can't find weapon model {model} in ModelTable");
            return null;
        }
    }

    public GameObject GetContainer(CharacterModelType raceId, EntityType entityType)
    {
        GameObject go = null;

        switch (entityType)
        {
            case EntityType.User:
                go = _userContainers[(byte)raceId];
                break;
            case EntityType.Player:
                go = _playerContainers[(byte)raceId];
                break;
            case EntityType.Pawn:
                go = _pawnContainers[(byte)raceId];
                break;
        }

        if (go == null)
        {
            Debug.LogError($"Can't find container for race {raceId} and entity type {entityType}");
        }

        return go;
    }

    public GameObject GetFace(CharacterModelType raceId, byte face)
    {
        face = (byte)Mathf.Min(face, FACE_COUNT - 1);

        // Debug.Log($"Loading Face[{face}] Race:{raceId}");
        GameObject prefab = _faceModels[(byte)raceId];
        if (prefab == null)
        {
            Debug.LogError($"Can't find face model for race {raceId} at index {raceId},{face}");
            return null;
        }

        Material material = _faceMaterials[(byte)raceId, face];
        if (material == null)
        {
            Debug.LogError($"Can't find face material {face} for race {raceId} at index {raceId},{face}");
        }

        GameObject go = GameObject.Instantiate(prefab);
        if (material != null)
        {
            var smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                smr.material = material;
            }
            else
            {
                var mr = go.GetComponentInChildren<MeshRenderer>();
                if (mr != null)
                {
                    mr.material = material;
                }
            }
        }

        return go;
    }

    public GameObject GetHair(CharacterModelType raceId, byte hairStyle, byte hairColor, bool bh)
    {
        hairStyle = (byte)Mathf.Min(hairStyle, HAIR_STYLE_COUNT - 1);
        hairColor = (byte)Mathf.Min(hairColor, HAIR_COLOR_COUNT - 1);

        hairStyle = (byte)(hairStyle * 2);

        if (bh)
        {
            hairStyle += 1;
        }

        // Debug.Log($"Loading hair[{hairStyle}:{hairColor}] Race:{raceId} Model:{hairStyle}_{hairColor}_{(bh ? "bh" : "ah")}");

        GameObject prefab = _hairModels[(byte)raceId, hairStyle];
        if (prefab == null)
        {
            // Debug.LogError($"Can't find hairstyle model {hairStyle} haircolor {hairColor} for race {raceId} at index {raceId},{hairStyle}");
            return null;
        }

        Material material = _hairMaterials[(byte)raceId, hairStyle, hairColor];

        if (material == null)
        {
            Debug.LogError($"Can't find hairstyle material {hairStyle} haircolor {hairColor} for race {raceId} at index {raceId},{hairColor}");
        }

        GameObject go = GameObject.Instantiate(prefab);
        SkinnedMeshRenderer skinnedMeshRenderer = go.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.material = material;
        }
        else
        {
            MeshRenderer meshRenderer = go.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }
            else
            {
                Debug.LogError($"Hairstyle material {hairStyle} haircolor {hairColor} for race {raceId} doesn't have any renderer.");
            }
        }


        return go;
    }

    public GameObject GetNpc(string meshname)
    {
        GameObject npc = null;
        _npcs.TryGetValue(meshname, out npc);
        if (npc == null)
        {
            Debug.LogError($"Can't find npc {meshname} model in ModelTable.");
        }

        return npc;
    }
}
