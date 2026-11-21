// One-click builder for the Phase-1 vertical slice: "Andhvan — South Lane".
// Menu: The Last Night > 1. Build Vertical Slice Scene
//       The Last Night > 2. Apply Android Player Settings
// Everything is greybox/stylized geometry made from primitives + generated textures, so the slice
// is playable with zero downloaded assets. Swap in real art later (see ASSETS.md) — scripts stay.
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLastNight.EditorTools
{
    public static class SliceSceneBuilder
    {
        const string Root = "Assets/TheLastNight";
        const string GenDir = Root + "/Generated";
        const string ScenePath = Root + "/Scenes/Andhvan_Slice.unity";

        static Material mDirt, mLane, mPlaster, mPlasterDark, mWood, mDoor, mRoof, mStone, mMetal, mFoliage, mTrunk, mCloth, mMonster, mEye, mGhost, mBulb, mFuse, mBattery, mMedkit, mPaper, mCar, mGlass, mRain, mWater;
        static Transform level;
        static readonly List<Collider> bakeDisabled = new List<Collider>();
        static readonly List<Transform> patrol = new List<Transform>();
        static readonly List<Light> villageLamps = new List<Light>();
        static readonly List<Light> generatorLamps = new List<Light>();
        static Transform dynamicRoot;

        [MenuItem("The Last Night/1. Build Vertical Slice Scene", priority = 1)]
        public static void Build()
        {
            if (GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset == null)
            {
                EditorUtility.DisplayDialog("The Last Night", "URP active nahi hai.\nProject 'Universal 3D' template se banao (SETUP.md step 1).", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            bakeDisabled.Clear(); patrol.Clear(); villageLamps.Clear(); generatorLamps.Clear();
            EnsureFolder(Root, "Generated"); EnsureFolder(Root, "Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EditorUtility.DisplayProgressBar("The Last Night", "Materials...", 0.1f);
            MakeMaterials();
            EditorUtility.DisplayProgressBar("The Last Night", "Lighting & fog...", 0.2f);
            SetupLighting();
            level = new GameObject("Level").transform;
            dynamicRoot = new GameObject("Dynamic").transform;
            EditorUtility.DisplayProgressBar("The Last Night", "Village...", 0.35f);
            BuildVillage();
            EditorUtility.DisplayProgressBar("The Last Night", "NavMesh bake...", 0.6f);
            BakeNavMesh();
            EditorUtility.DisplayProgressBar("The Last Night", "Player, monster, systems...", 0.8f);
            var player = BuildPlayer(new Vector3(1.6f, 0f, -1.5f));
            var ghost = BuildGhost();
            BuildMonster(new Vector3(0f, 0f, 41f));
            BuildSystems(player, ghost);

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorUtility.ClearProgressBar();
            Debug.Log("[TLN] Vertical slice built: " + ScenePath + "  — press Play.");
            EditorUtility.DisplayDialog("The Last Night", "Scene ban gaya!\n" + ScenePath + "\n\nPlay dabao. PC controls: WASD, mouse (click to lock), Shift, C, E, F, Tab, R, Esc.", "Chalo");
        }

        [MenuItem("The Last Night/2. Apply Android Player Settings", priority = 2)]
        public static void AndroidSettings()
        {
            PlayerSettings.companyName = "Cowartu";
            PlayerSettings.productName = "The Last Night";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.cowartu.thelastnight");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            Debug.Log("[TLN] Android player settings applied (IL2CPP, ARM64, API 26+, landscape).");
            EditorUtility.DisplayDialog("The Last Night", "Android settings set.\nAb File > Build Profiles > Android > Switch Platform, phir Build.", "OK");
        }

        // ================================================================== materials
        static void MakeMaterials()
        {
            var noise = SaveTexture("noise_grain", Noise(256, 0.55f, 0.45f, 3));
            var plaster = SaveTexture("plaster", Plaster(256));
            var planks = SaveTexture("planks", Planks(256));
            mDirt = Lit("Dirt", new Color(0.24f, 0.2f, 0.15f), noise, 0.05f, new Vector2(30, 30));
            mLane = Lit("Lane", new Color(0.17f, 0.15f, 0.13f), noise, 0.15f, new Vector2(4, 30));
            mPlaster = Lit("Plaster", new Color(0.62f, 0.55f, 0.45f), plaster, 0.1f, new Vector2(2, 1));
            mPlasterDark = Lit("PlasterDark", new Color(0.38f, 0.33f, 0.3f), plaster, 0.1f, new Vector2(2, 1));
            mWood = Lit("Wood", new Color(0.36f, 0.24f, 0.15f), planks, 0.2f, Vector2.one);
            mDoor = Lit("Door", new Color(0.28f, 0.16f, 0.1f), planks, 0.25f, Vector2.one);
            mRoof = Lit("Roof", new Color(0.32f, 0.14f, 0.1f), planks, 0.15f, new Vector2(3, 3));
            mStone = Lit("Stone", new Color(0.4f, 0.39f, 0.37f), noise, 0.1f, new Vector2(2, 2));
            mMetal = Lit("Metal", new Color(0.3f, 0.32f, 0.3f), noise, 0.5f, Vector2.one, 0.7f);
            mFoliage = Lit("Foliage", new Color(0.08f, 0.14f, 0.09f), noise, 0f, Vector2.one);
            mTrunk = Lit("Trunk", new Color(0.18f, 0.13f, 0.1f), noise, 0f, Vector2.one);
            mCloth = Lit("Cloth", new Color(0.4f, 0.12f, 0.1f), noise, 0f, Vector2.one);
            mCar = Lit("Car", new Color(0.12f, 0.16f, 0.2f), null, 0.6f, Vector2.one, 0.4f);
            mGlass = Lit("Glass", new Color(0.05f, 0.07f, 0.08f), null, 0.9f, Vector2.one);
            mWater = Lit("Water", new Color(0.02f, 0.03f, 0.03f), null, 0.95f, Vector2.one);
            mMonster = Lit("Monster", new Color(0.03f, 0.025f, 0.025f), noise, 0.35f, Vector2.one);
            mEye = Emissive("MonsterEye", Color.black, new Color(1f, 0.05f, 0f) * 3f);
            mGhost = Emissive("Ghost", new Color(0.6f, 0.62f, 0.65f), new Color(0.35f, 0.38f, 0.42f));
            mBulb = Emissive("Bulb", new Color(1f, 0.85f, 0.6f), new Color(1f, 0.7f, 0.4f) * 2f);
            mFuse = Emissive("Fuse", new Color(1f, 0.7f, 0.2f), new Color(1f, 0.55f, 0.1f) * 1.5f);
            mBattery = Emissive("Battery", new Color(0.3f, 0.9f, 0.4f), new Color(0.1f, 0.7f, 0.2f) * 1.2f);
            mMedkit = Emissive("Medkit", new Color(0.9f, 0.9f, 0.9f), new Color(0.6f, 0.05f, 0.05f));
            mPaper = Emissive("Paper", new Color(0.85f, 0.8f, 0.65f), new Color(0.25f, 0.23f, 0.18f));
            mRain = RainMaterial();
        }

        static Material Lit(string name, Color c, Texture2D tex, float smooth, Vector2 tiling, float metallic = 0f)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
            if (tex != null) { m.SetTexture("_BaseMap", tex); m.SetTextureScale("_BaseMap", tiling); }
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", metallic);
            m.enableInstancing = true;
            return SaveMat(name, m);
        }

        static Material Emissive(string name, Color c, Color emission)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return SaveMat(name, m);
        }

        static Material RainMaterial()
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            m.SetColor("_BaseColor", new Color(0.7f, 0.75f, 0.85f, 0.25f));
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            return SaveMat("Rain", m);
        }

        static Material SaveMat(string name, Material m)
        {
            string path = GenDir + "/M_" + name + ".mat";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Texture2D SaveTexture(string name, Texture2D t)
        {
            string path = GenDir + "/T_" + name + ".png";
            File.WriteAllBytes(path, t.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            imp.maxTextureSize = 256; imp.mipmapEnabled = true; imp.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static Texture2D Noise(int s, float baseV, float amp, int octaves)
        {
            var t = new Texture2D(s, s);
            for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
            {
                float v = 0f, a = amp, f = 4f;
                for (int o = 0; o < octaves; o++) { v += (Mathf.PerlinNoise(x * f / s + o * 7.3f, y * f / s + o * 3.1f) - 0.5f) * a; a *= 0.5f; f *= 2.3f; }
                v += Random.Range(-0.04f, 0.04f);
                float g = Mathf.Clamp01(baseV + v);
                t.SetPixel(x, y, new Color(g, g, g));
            }
            t.Apply();
            return t;
        }

        static Texture2D Plaster(int s)
        {
            var t = Noise(s, 0.7f, 0.35f, 4);
            // damp stains near the bottom + a few cracks
            for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
            {
                float damp = Mathf.Clamp01(1f - y / (s * 0.35f)) * (0.5f + 0.5f * Mathf.PerlinNoise(x * 0.05f, 9f));
                Color c = t.GetPixel(x, y) * (1f - damp * 0.45f);
                t.SetPixel(x, y, c);
            }
            for (int k = 0; k < 6; k++)
            {
                int x = Random.Range(0, s), y = Random.Range(s / 3, s);
                for (int i = 0; i < 40; i++) { x = Mathf.Clamp(x + Random.Range(-1, 2), 0, s - 1); y = Mathf.Clamp(y - 1, 0, s - 1); t.SetPixel(x, y, Color.black * 0.3f); }
            }
            t.Apply();
            return t;
        }

        static Texture2D Planks(int s)
        {
            var t = new Texture2D(s, s);
            int plank = s / 6;
            for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
            {
                int row = x / plank;
                float grain = Mathf.PerlinNoise(x * 0.4f + row * 13f, y * 0.03f) * 0.35f;
                float edge = (x % plank) < 2 ? 0.35f : 1f;
                float g = (0.55f + grain + (row % 2) * 0.06f) * edge;
                t.SetPixel(x, y, new Color(g, g, g));
            }
            t.Apply();
            return t;
        }

        // ================================================================== lighting
        static void SetupLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.045f, 0.05f, 0.075f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.02f, 0.024f, 0.032f);
            RenderSettings.fogDensity = 0.06f;

            var moon = new GameObject("Moon").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.45f, 0.55f, 0.85f);
            moon.intensity = 0.18f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            moon.gameObject.AddComponent<ShadowQualitySwitch>().minimumPreset = GraphicsPreset.High;
        }

        // ================================================================== village
        static void BuildVillage()
        {
            // ground & lane
            Box("Ground", level, new Vector3(0, -0.1f, 25), new Vector3(90, 0.2f, 100), mDirt);
            Box("Lane", level, new Vector3(0, 0.005f, 25), new Vector3(5.5f, 0.02f, 72), mLane, collider: false);

            // boundary fence (bamboo) + invisible walls
            Fence(new Vector3(-22, 0, -10), new Vector3(-22, 0, 56));
            Fence(new Vector3(22, 0, -10), new Vector3(22, 0, 56));
            Fence(new Vector3(-22, 0, -10), new Vector3(22, 0, -10));
            Fence(new Vector3(-22, 0, 56), new Vector3(-3, 0, 56));
            Fence(new Vector3(3, 0, 56), new Vector3(22, 0, 56));
            Wall("Bound_W", new Vector3(-22.3f, 2, 23), new Vector3(0.4f, 4, 68));
            Wall("Bound_E", new Vector3(22.3f, 2, 23), new Vector3(0.4f, 4, 68));
            Wall("Bound_S", new Vector3(0, 2, -10.3f), new Vector3(45, 4, 0.4f));
            Wall("Bound_N_L", new Vector3(-12.5f, 2, 56.3f), new Vector3(19.4f, 4, 0.4f));
            Wall("Bound_N_R", new Vector3(12.5f, 2, 56.3f), new Vector3(19.4f, 4, 0.4f));
            Wall("Bound_Exit", new Vector3(0, 2, 63), new Vector3(10, 4, 0.4f));
            Wall("Bound_ExitW", new Vector3(-3.2f, 2, 59.5f), new Vector3(0.4f, 4, 7));
            Wall("Bound_ExitE", new Vector3(3.2f, 2, 59.5f), new Vector3(0.4f, 4, 7));

            // forest outside + a few trees inside
            var r = new System.Random(7);
            for (int i = 0; i < 140; i++)
            {
                float a = (float)r.NextDouble() * Mathf.PI * 2f, d = 26f + (float)r.NextDouble() * 18f;
                Vector3 p = new Vector3(Mathf.Cos(a) * d * 0.8f, 0, 23 + Mathf.Sin(a) * d);
                Tree(p, 0.8f + (float)r.NextDouble() * 0.7f, false);
            }
            foreach (var p in new[] { new Vector3(-17, 0, 2), new Vector3(16, 0, 30), new Vector3(-17, 0, 30), new Vector3(15, 0, 4), new Vector3(-6, 0, 52), new Vector3(17, 0, 48) })
                Tree(p, 1.1f, true);

            // the dead car at the start
            Car(new Vector3(-1.2f, 0, -5f));

            // houses: (centre, door faces +x for left side / -x for right side)
            House("House_L1", new Vector3(-9.5f, 0, 8), 1, true, HouseKind.Diary);
            House("House_R1", new Vector3(9.5f, 0, 8), -1, false, HouseKind.None);
            House("House_R2", new Vector3(9.5f, 0, 22), -1, true, HouseKind.Radio);
            House("House_L2", new Vector3(-9.5f, 0, 22), 1, false, HouseKind.None);
            House("House_L3", new Vector3(-9.5f, 0, 36), 1, true, HouseKind.Shelf);
            House("House_R3", new Vector3(9.5f, 0, 36), -1, false, HouseKind.None);

            Well(new Vector3(4.5f, 0, 15f));
            Temple(new Vector3(0, 0, 46));
            PumpHouse(new Vector3(-12f, 0, 48.5f));
            Gate(new Vector3(0, 0, 56));

            // street lamps (village grid: weak + flickery; die in power failures)
            float[] zs = { 2, 14, 26, 38 };
            for (int i = 0; i < zs.Length; i++) villageLamps.Add(Lamp(new Vector3(i % 2 == 0 ? -3.4f : 3.4f, 0, zs[i]), true, 2.2f));

            // patrol points
            foreach (var p in new[] { new Vector3(0, 0, 4), new Vector3(0, 0, 18), new Vector3(0, 0, 30), new Vector3(0, 0, 42), new Vector3(-15, 0, 15), new Vector3(15, 0, 15),
                         new Vector3(-15, 0, 42), new Vector3(15, 0, 42), new Vector3(-9.5f, 0, 8), new Vector3(9.5f, 0, 22), new Vector3(-9.5f, 0, 36), new Vector3(-11f, 0, 50), new Vector3(6, 0, 52) })
            {
                var t = new GameObject("Patrol").transform; t.SetParent(dynamicRoot); t.position = p; patrol.Add(t);
            }

            // items: batteries & medkit & notes around (fuses are placed inside houses)
            Item(ItemType.Battery, new Vector3(0.5f, 0.2f, -2.4f));    // next to the car
            Item(ItemType.Battery, new Vector3(6.0f, 0.2f, 15.2f));    // by the well
            Item(ItemType.Battery, new Vector3(2.2f, 0.75f, 45f));     // on the chabutra
            Item(ItemType.Battery, new Vector3(13.5f, 0.3f, 9.5f));    // behind R1
            Note(new Vector3(-1.0f, 1.07f, -3.3f), "Kabir ka message (11:48 PM)",
                "Aryan, gaadi gaon ke gate ke paas chhod dena.\nAndar mat jaana jab tak main na aaun.\nAur agar koi tumhara naam pukaare...\njawab mat dena.\n\n— K");
            Note(new Vector3(1.2f, 0.62f, 44.2f), "Swasthya Shivir — 2005",
                "MUFT SWASTHYA SHIVIR\nDr. Vikram Sahay\nGaon ke Hospital mein\n\nNeend na aana? Bure sapne? Darr?\nEk injection — aur sab theek.\n\n(Neeche kisi ne laal syahi se likha hai:)\n\"usne darr nikaal liya. ab darr bahar ghoomta hai.\"");
        }

        enum HouseKind { None, Diary, Radio, Shelf }

        static void House(string name, Vector3 c, int face, bool enterable, HouseKind kind)
        {
            var h = new GameObject(name).transform; h.SetParent(level); h.position = c;
            h.rotation = Quaternion.Euler(0, face > 0 ? 90 : -90, 0); // local +z = toward the lane
            float w = 7f, d = 6.5f, ht = 3f, t = 0.25f, doorW = 1.4f, doorH = 2.2f;
            var wall = enterable ? mPlaster : mPlasterDark;
            // floor
            Box("Floor", h, L(h, 0, 0.05f, 0), new Vector3(w, 0.1f, d), mWood, rot: h.rotation);
            // back & sides
            Box("Back", h, L(h, 0, ht / 2, -d / 2), new Vector3(w, ht, t), wall, rot: h.rotation);
            Box("SideL", h, L(h, -w / 2, ht / 2, 0), new Vector3(t, ht, d), wall, rot: h.rotation);
            Box("SideR", h, L(h, w / 2, ht / 2, 0), new Vector3(t, ht, d), wall, rot: h.rotation);
            // front with door gap
            float side = (w - doorW) / 2f;
            Box("FrontL", h, L(h, -w / 2 + side / 2, ht / 2, d / 2), new Vector3(side, ht, t), wall, rot: h.rotation);
            Box("FrontR", h, L(h, w / 2 - side / 2, ht / 2, d / 2), new Vector3(side, ht, t), wall, rot: h.rotation);
            Box("FrontTop", h, L(h, 0, doorH + (ht - doorH) / 2, d / 2), new Vector3(doorW, ht - doorH, t), wall, rot: h.rotation);
            // dark window holes (decor)
            Box("Window", h, L(h, -w / 4 - 0.4f, 1.7f, d / 2 + 0.13f), new Vector3(0.9f, 0.8f, 0.02f), mGlass, collider: false, rot: h.rotation);
            // roof
            var roofA = Box("RoofA", h, L(h, 0, ht + 0.7f, -d / 4), new Vector3(w + 0.8f, 0.15f, d / 2 + 0.9f), mRoof, rot: h.rotation * Quaternion.Euler(-25, 0, 0));
            Box("RoofB", h, L(h, 0, ht + 0.7f, d / 4), new Vector3(w + 0.8f, 0.15f, d / 2 + 0.9f), mRoof, rot: h.rotation * Quaternion.Euler(25, 0, 0));
            Box("Ceiling", h, L(h, 0, ht, 0), new Vector3(w, 0.1f, d), mWood, rot: h.rotation);
            roofA.isStatic = true;

            // door
            var door = MakeDoor(h, L(h, -doorW / 2, 0, d / 2), h.rotation, doorW, doorH);
            door.locked = !enterable;
            if (!enterable) door.lockedMessage = "Band hai. Andar se kuch khurach raha hai...";
            if (!enterable) return;

            // interior: almari (hide), bed (hide under), table, props
            Cupboard(h, L(h, w / 2 - 0.55f, 0, -d / 2 + 1.2f), h.rotation * Quaternion.Euler(0, -90, 0));
            Bed(h, L(h, -w / 2 + 1.1f, 0, -d / 2 + 1.4f), h.rotation);
            Box("Table", h, L(h, 0.6f, 0.75f, -0.6f), new Vector3(1.4f, 0.08f, 0.8f), mWood, rot: h.rotation);
            foreach (var lx in new[] { -0.6f, 0.6f }) foreach (var lz in new[] { -0.35f, 0.35f })
                    Box("Leg", h, L(h, 0.6f + lx * 0.9f, 0.37f, -0.6f + lz), new Vector3(0.07f, 0.74f, 0.07f), mWood, rot: h.rotation);
            Prop(L(h, 0.2f, 0.95f, -0.5f), new Vector3(0.12f, 0.3f, 0.12f), mGlass, false);    // bottle on table
            Prop(L(h, -1.5f, 0.2f, 1.6f), new Vector3(0.4f, 0.4f, 0.4f), mWood, false);       // crate
            Prop(L(h, 1.8f, 0.15f, 1.2f), new Vector3(0.35f, 0.3f, 0.35f), mMetal, false);    // bucket

            // single bulb (village grid)
            villageLamps.Add(Bulb(L(h, 0, ht - 0.35f, 0)));

            switch (kind)
            {
                case HouseKind.Diary:
                    Item(ItemType.Fuse, L(h, 0.9f, 0.95f, -0.6f));
                    Note(L(h, 0.3f, 0.82f, -0.75f), "Ramesh ki diary — 14 Nov 2006",
                        "Aaj phir 3 log gayab. Sab ne doctor sahab ka injection liya tha.\nMeera bitiya raat bhar hansti rehti hai.\nKehti hai \"wo\" uska dost hai.\n\nHum darwaze andar se band kar ke so rahe hain.\nPar wo darwaze se nahi aata.\nWo DARR se aata hai.");
                    break;
                case HouseKind.Radio:
                    Item(ItemType.Fuse, L(h, -1.4f, 0.12f, -0.2f));
                    Item(ItemType.Medkit, L(h, 0.3f, 0.9f, -0.5f));
                    var radio = Prop(L(h, 1.0f, 0.9f, -0.75f), new Vector3(0.4f, 0.22f, 0.18f), mCar, true);
                    radio.name = "Radio";
                    break;
                case HouseKind.Shelf:
                    Box("Shelf", h, L(h, -w / 2 + 0.3f, 1.4f, 0.8f), new Vector3(0.35f, 0.06f, 1.6f), mWood, rot: h.rotation);
                    Item(ItemType.Fuse, L(h, -w / 2 + 0.3f, 1.55f, 0.6f));
                    Item(ItemType.Battery, L(h, -w / 2 + 0.3f, 1.55f, 1.2f));
                    break;
            }
        }

        static Vector3 L(Transform h, float x, float y, float z) { return h.TransformPoint(new Vector3(x, y, z)); }

        static Door MakeDoor(Transform parent, Vector3 hingePos, Quaternion rot, float w, float hgt)
        {
            var root = new GameObject("Door").transform; root.SetParent(parent); root.position = hingePos; root.rotation = rot;
            var hinge = new GameObject("Hinge").transform; hinge.SetParent(root, false);
            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Panel"; panel.transform.SetParent(hinge, false);
            panel.transform.localPosition = new Vector3(w / 2, hgt / 2, 0); panel.transform.localScale = new Vector3(w - 0.04f, hgt - 0.02f, 0.08f);
            panel.GetComponent<Renderer>().sharedMaterial = mDoor;
            bakeDisabled.Add(panel.GetComponent<Collider>());
            var door = root.gameObject.AddComponent<Door>();
            door.hinge = hinge;
            return door;
        }

        static void Cupboard(Transform parent, Vector3 pos, Quaternion rot)
        {
            var root = new GameObject("Almari").transform; root.SetParent(parent); root.position = pos; root.rotation = rot;
            Box("Body", root, root.TransformPoint(new Vector3(0, 1.0f, -0.05f)), new Vector3(1.1f, 2.0f, 0.1f), mWood, rot: rot);
            Box("SideL", root, root.TransformPoint(new Vector3(-0.53f, 1.0f, 0.3f)), new Vector3(0.06f, 2.0f, 0.7f), mWood, rot: rot);
            Box("SideR", root, root.TransformPoint(new Vector3(0.53f, 1.0f, 0.3f)), new Vector3(0.06f, 2.0f, 0.7f), mWood, rot: rot);
            Box("Top", root, root.TransformPoint(new Vector3(0, 2.0f, 0.3f)), new Vector3(1.1f, 0.06f, 0.7f), mWood, rot: rot);
            var doorPivot = new GameObject("DoorPivot").transform; doorPivot.SetParent(root, false); doorPivot.localPosition = new Vector3(-0.5f, 0, 0.66f);
            var d = Box("Door", doorPivot, root.TransformPoint(new Vector3(0, 1.0f, 0.66f)), new Vector3(1.0f, 1.95f, 0.05f), mDoor, rot: rot, isStatic: false);
            d.transform.SetParent(doorPivot, true);
            var col = root.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 1f, 0.3f); col.size = new Vector3(1.1f, 2f, 0.7f); col.isTrigger = true;
            var spot = root.gameObject.AddComponent<HidingSpot>();
            spot.hidePoint = Child(root, "HidePoint", new Vector3(0, 0, 0.3f), Quaternion.identity);
            spot.exitPoint = Child(root, "ExitPoint", new Vector3(0, 0, 1.3f), Quaternion.identity);
            spot.monsterCheckPoint = Child(root, "MonsterCheck", new Vector3(0, 0, 1.6f), Quaternion.identity);
            spot.eyeHeight = 1.55f; spot.doorVisual = doorPivot; spot.label = "Almari";
        }

        static void Bed(Transform parent, Vector3 pos, Quaternion rot)
        {
            var root = new GameObject("Bed").transform; root.SetParent(parent); root.position = pos; root.rotation = rot;
            Box("Mattress", root, root.TransformPoint(new Vector3(0, 0.55f, 0)), new Vector3(1.2f, 0.18f, 2.0f), mCloth, rot: rot);
            Box("Frame", root, root.TransformPoint(new Vector3(0, 0.43f, 0)), new Vector3(1.3f, 0.06f, 2.1f), mWood, rot: rot);
            foreach (var x in new[] { -0.6f, 0.6f }) foreach (var z in new[] { -1f, 1f })
                    Box("Leg", root, root.TransformPoint(new Vector3(x, 0.2f, z)), new Vector3(0.08f, 0.4f, 0.08f), mWood, rot: rot);
            var col = root.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0, 0.3f, 0); col.size = new Vector3(1.4f, 0.6f, 2.2f); col.isTrigger = true;
            var spot = root.gameObject.AddComponent<HidingSpot>();
            spot.hidePoint = Child(root, "HidePoint", new Vector3(0, 0, 0.2f), Quaternion.Euler(0, 180, 0));
            spot.exitPoint = Child(root, "ExitPoint", new Vector3(1.2f, 0, 0), Quaternion.Euler(0, 90, 0));
            spot.monsterCheckPoint = Child(root, "MonsterCheck", new Vector3(1.4f, 0, 0), Quaternion.identity);
            spot.eyeHeight = 0.22f; spot.label = "Palang ke neeche";
        }

        static void Car(Vector3 p)
        {
            var c = new GameObject("DeadCar").transform; c.SetParent(level); c.position = p; c.rotation = Quaternion.Euler(0, 8, 0);
            Box("Body", c, c.TransformPoint(new Vector3(0, 0.7f, 0)), new Vector3(1.8f, 0.7f, 4.2f), mCar, rot: c.rotation);
            Box("Cabin", c, c.TransformPoint(new Vector3(0, 1.3f, -0.3f)), new Vector3(1.6f, 0.6f, 2.2f), mGlass, rot: c.rotation);
            foreach (var x in new[] { -0.9f, 0.9f }) foreach (var z in new[] { -1.4f, 1.4f })
                {
                    var w = Cyl("Wheel", c, c.TransformPoint(new Vector3(x, 0.35f, z)), new Vector3(0.7f, 0.12f, 0.7f), mTrunk, c.rotation * Quaternion.Euler(0, 0, 90));
                    Object.DestroyImmediate(w.GetComponent<Collider>());
                }
            // hazard light blinking weakly — the last bit of the outside world
            var l = new GameObject("Hazard").AddComponent<Light>(); l.transform.SetParent(c); l.transform.position = c.TransformPoint(new Vector3(0, 0.8f, 2.2f));
            l.type = LightType.Point; l.color = new Color(1f, 0.55f, 0.1f); l.range = 5f; l.intensity = 1.2f;
            var fl = l.gameObject.AddComponent<FlickerLight>(); fl.baseIntensity = 1.2f; fl.idleFlicker = 0.5f;
        }

        static void Well(Vector3 p)
        {
            var w = new GameObject("Well").transform; w.SetParent(level); w.position = p;
            var ring = Cyl("Ring", w, p + Vector3.up * 0.45f, new Vector3(1.8f, 0.45f, 1.8f), mStone, Quaternion.identity);
            Cyl("Water", w, p + Vector3.up * 0.86f, new Vector3(1.5f, 0.02f, 1.5f), mWater, Quaternion.identity);
            Box("PostL", w, p + new Vector3(-0.95f, 1.2f, 0), new Vector3(0.12f, 1.6f, 0.12f), mWood);
            Box("PostR", w, p + new Vector3(0.95f, 1.2f, 0), new Vector3(0.12f, 1.6f, 0.12f), mWood);
            Box("Beam", w, p + new Vector3(0, 2f, 0), new Vector3(2.1f, 0.12f, 0.12f), mWood);
            Prop(p + new Vector3(1.3f, 0.15f, 0.6f), new Vector3(0.35f, 0.3f, 0.35f), mMetal, false);
            ring.name = "WellRing";
        }

        static void Temple(Vector3 p)
        {
            var t = new GameObject("Temple").transform; t.SetParent(level); t.position = p;
            Box("Chabutra", t, p + new Vector3(0, 0.3f, 0), new Vector3(6, 0.6f, 6), mStone);
            Box("Step", t, p + new Vector3(0, 0.15f, -3.4f), new Vector3(3, 0.3f, 0.8f), mStone);
            Box("Shrine", t, p + new Vector3(0, 1.6f, 1.5f), new Vector3(2.2f, 2f, 1.8f), mStone);
            Box("ShrineTop", t, p + new Vector3(0, 3.0f, 1.5f), new Vector3(1.4f, 0.8f, 1.2f), mStone);
            Box("Niche", t, p + new Vector3(0, 1.5f, 0.58f), new Vector3(0.8f, 1.0f, 0.05f), mGlass, collider: false);
            Box("Flag", t, p + new Vector3(2.5f, 2.5f, 2.5f), new Vector3(0.05f, 5f, 0.05f), mWood);
            Box("FlagCloth", t, p + new Vector3(2.9f, 4.6f, 2.5f), new Vector3(0.8f, 0.5f, 0.02f), mCloth, collider: false);
            // diya flame
            var l = new GameObject("Diya").AddComponent<Light>(); l.transform.SetParent(t); l.transform.position = p + new Vector3(0, 1.3f, 0.4f);
            l.type = LightType.Point; l.color = new Color(1f, 0.45f, 0.15f); l.range = 4f; l.intensity = 1.5f;
            var fl = l.gameObject.AddComponent<FlickerLight>(); fl.baseIntensity = 1.5f; fl.idleFlicker = 0.35f;
        }

        static void PumpHouse(Vector3 p)
        {
            var h = new GameObject("PumpHouse").transform; h.SetParent(level); h.position = p; h.rotation = Quaternion.Euler(0, 90, 0);
            Box("Floor", h, L(h, 0, 0.05f, 0), new Vector3(5, 0.1f, 4.5f), mStone, rot: h.rotation);
            Box("Back", h, L(h, 0, 1.5f, -2.25f), new Vector3(5, 3, 0.25f), mPlasterDark, rot: h.rotation);
            Box("SideL", h, L(h, -2.5f, 1.5f, 0), new Vector3(0.25f, 3, 4.5f), mPlasterDark, rot: h.rotation);
            Box("SideR", h, L(h, 2.5f, 1.5f, 0), new Vector3(0.25f, 3, 4.5f), mPlasterDark, rot: h.rotation);
            Box("Roof", h, L(h, 0, 3.1f, 0), new Vector3(5.4f, 0.15f, 5f), mMetal, rot: h.rotation);

            // generator
            var g = new GameObject("Generator").transform; g.SetParent(h); g.position = L(h, 0, 0, -1.1f); g.rotation = h.rotation;
            var shake = new GameObject("Shake").transform; shake.SetParent(g, false);
            var body = Box("Body", shake, g.TransformPoint(new Vector3(0, 0.55f, 0)), new Vector3(1.6f, 0.9f, 0.9f), mMetal, rot: g.rotation, isStatic: false);
            Box("Tank", shake, g.TransformPoint(new Vector3(0, 1.1f, 0)), new Vector3(1.0f, 0.25f, 0.6f), mCloth, rot: g.rotation, isStatic: false, collider: false);
            Box("FusePanel", shake, g.TransformPoint(new Vector3(0.55f, 0.75f, 0.46f)), new Vector3(0.4f, 0.4f, 0.04f), mGlass, rot: g.rotation, isStatic: false, collider: false);
            var status = new GameObject("Status").AddComponent<Light>(); status.transform.SetParent(g, false); status.transform.localPosition = new Vector3(0.55f, 1.1f, 0.6f);
            status.type = LightType.Point; status.range = 2.5f; status.intensity = 1.5f; status.color = Color.red;
            var gen = g.gameObject.AddComponent<Generator>();
            gen.statusLight = status; gen.shakeRoot = shake;
            var gcol = g.gameObject.AddComponent<BoxCollider>(); gcol.center = new Vector3(0, 0.7f, 0.3f); gcol.size = new Vector3(1.8f, 1.4f, 1.4f); gcol.isTrigger = true;
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Box("GenBlock", h, g.TransformPoint(new Vector3(0, 0.55f, 0)), new Vector3(1.6f, 1.1f, 0.9f), null, rot: g.rotation, renderer: false);

            generatorLamps.Add(Bulb(L(h, 0, 2.6f, 0), false));
            generatorLamps.Add(Lamp(new Vector3(-7f, 0, 47f), false, 2.8f));
            Note(L(h, -1.6f, 0.9f, -1.75f), "Pump-house register",
                "Generator ke 3 fuse nikaal kar alag-alag gharon mein rakh diye hain.\nGate isi se chalta hai.\n\nKoi bhi gate na khole.\nAmavasya tak nahi.\n\n— Panchayat, 2006");
            Box("NoteShelf", h, L(h, -1.6f, 0.85f, -1.75f), new Vector3(0.8f, 0.05f, 0.4f), mWood, rot: h.rotation);
        }

        static void Gate(Vector3 p)
        {
            var g = new GameObject("EscapeGate").transform; g.SetParent(dynamicRoot); g.position = p;
            Box("PillarL", level, p + new Vector3(-3, 1.6f, 0), new Vector3(0.6f, 3.2f, 0.6f), mStone);
            Box("PillarR", level, p + new Vector3(3, 1.6f, 0), new Vector3(0.6f, 3.2f, 0.6f), mStone);
            Box("Arch", level, p + new Vector3(0, 3.3f, 0), new Vector3(6.6f, 0.4f, 0.6f), mStone);
            var left = new GameObject("Left").transform; left.SetParent(g, false); left.localPosition = new Vector3(-1.35f, 0, 0);
            var right = new GameObject("Right").transform; right.SetParent(g, false); right.localPosition = new Vector3(1.35f, 0, 0);
            foreach (var panel in new[] { left, right })
            {
                for (int i = -2; i <= 2; i++) Box("Bar", panel, panel.TransformPoint(new Vector3(i * 0.26f, 1.3f, 0)), new Vector3(0.07f, 2.6f, 0.07f), mMetal, isStatic: false, collider: false);
                Box("Rail", panel, panel.TransformPoint(new Vector3(0, 2.4f, 0)), new Vector3(1.3f, 0.1f, 0.08f), mMetal, isStatic: false, collider: false);
                Box("Rail", panel, panel.TransformPoint(new Vector3(0, 0.3f, 0)), new Vector3(1.3f, 0.1f, 0.08f), mMetal, isStatic: false, collider: false);
            }
            var blocker = g.gameObject.AddComponent<BoxCollider>(); blocker.center = new Vector3(0, 1.5f, 0); blocker.size = new Vector3(5.4f, 3f, 0.3f);
            var signal = new GameObject("Signal").AddComponent<Light>(); signal.transform.SetParent(g, false); signal.transform.localPosition = new Vector3(0, 3.8f, 0.4f);
            signal.type = LightType.Point; signal.color = Color.red; signal.range = 6f; signal.intensity = 2f;
            var gate = g.gameObject.AddComponent<EscapeGate>();
            gate.leftPanel = left; gate.rightPanel = right; gate.blocker = blocker; gate.signalLight = signal;
            var zone = new GameObject("ExitZone"); zone.transform.SetParent(dynamicRoot); zone.transform.position = p + new Vector3(0, 1, 4.5f);
            var zc = zone.AddComponent<BoxCollider>(); zc.isTrigger = true; zc.size = new Vector3(6, 3, 2);
            zone.AddComponent<EscapeZone>();
            generatorLamps.Add(Lamp(p + new Vector3(4.2f, 0, -1.2f), false, 3f));
            // the road beyond the gate
            Box("Road", level, p + new Vector3(0, 0.005f, 6), new Vector3(5, 0.02f, 12), mLane, collider: false);
        }

        // ================================================================== small builders
        static Light Lamp(Vector3 p, bool powered, float intensity)
        {
            var t = new GameObject("StreetLamp").transform; t.SetParent(level); t.position = p;
            Box("Pole", t, p + Vector3.up * 2.2f, new Vector3(0.12f, 4.4f, 0.12f), mMetal);
            Box("Arm", t, p + new Vector3(p.x < 0 ? 0.5f : -0.5f, 4.35f, 0), new Vector3(1.0f, 0.08f, 0.08f), mMetal, collider: false);
            var bulbGo = Box("Bulb", t, p + new Vector3(p.x < 0 ? 0.95f : -0.95f, 4.2f, 0), new Vector3(0.25f, 0.15f, 0.25f), mBulb, collider: false, isStatic: false);
            var l = new GameObject("Light").AddComponent<Light>(); l.transform.SetParent(t); l.transform.position = bulbGo.transform.position - Vector3.up * 0.2f;
            l.type = LightType.Point; l.color = new Color(1f, 0.72f, 0.45f); l.range = 11f; l.intensity = intensity; l.shadows = LightShadows.None;
            var fl = l.gameObject.AddComponent<FlickerLight>(); fl.baseIntensity = intensity; fl.idleFlicker = powered ? 0.12f : 0.02f; fl.Powered = powered;
            fl.bulb = bulbGo.GetComponent<Renderer>();
            if (!powered) l.enabled = false;
            return l;
        }

        static Light Bulb(Vector3 p, bool powered = true)
        {
            var b = Box("Bulb", level, p, new Vector3(0.12f, 0.15f, 0.12f), mBulb, collider: false, isStatic: false);
            var l = new GameObject("BulbLight").AddComponent<Light>(); l.transform.SetParent(b.transform); l.transform.position = p - Vector3.up * 0.15f;
            l.type = LightType.Point; l.color = new Color(1f, 0.75f, 0.5f); l.range = 6f; l.intensity = 1.4f;
            var fl = l.gameObject.AddComponent<FlickerLight>(); fl.baseIntensity = 1.4f; fl.idleFlicker = 0.2f; fl.Powered = powered; fl.bulb = b.GetComponent<Renderer>();
            if (!powered) l.enabled = false;
            return l;
        }

        static void Tree(Vector3 p, float s, bool inside)
        {
            var t = new GameObject("Tree").transform; t.SetParent(level); t.position = p;
            var trunk = Cyl("Trunk", t, p + Vector3.up * 2.5f * s, new Vector3(0.35f * s, 2.5f * s, 0.35f * s), mTrunk, Quaternion.identity);
            if (!inside) Object.DestroyImmediate(trunk.GetComponent<Collider>()); // outside the fence: visual only
            for (int i = 0; i < 3; i++)
            {
                var c = GameObject.CreatePrimitive(PrimitiveType.Sphere); c.name = "Leaves"; c.transform.SetParent(t);
                c.transform.position = p + Vector3.up * (4.2f + i * 1.1f) * s; c.transform.localScale = new Vector3(3.2f - i * 0.8f, 1.6f, 3.2f - i * 0.8f) * s;
                c.GetComponent<Renderer>().sharedMaterial = mFoliage; Object.DestroyImmediate(c.GetComponent<Collider>());
                GameObjectUtility.SetStaticEditorFlags(c, StaticEditorFlags.BatchingStatic);
            }
        }

        static void Fence(Vector3 a, Vector3 b)
        {
            float len = Vector3.Distance(a, b);
            int n = Mathf.CeilToInt(len / 0.45f);
            var parent = new GameObject("Fence").transform; parent.SetParent(level);
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                float h = 2.2f + Mathf.PerlinNoise(p.x * 0.7f, p.z * 0.7f) * 0.8f;
                var post = Box("Bamboo", parent, p + Vector3.up * h / 2, new Vector3(0.1f, h, 0.1f), mTrunk, collider: false);
                post.transform.rotation = Quaternion.Euler(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            }
        }

        static void Wall(string name, Vector3 p, Vector3 size)
        {
            Box(name, level, p, size, null, renderer: false);
        }

        static GameObject Prop(Vector3 p, Vector3 size, Material m, bool radio)
        {
            var go = GameObject.CreatePrimitive(radio ? PrimitiveType.Cube : PrimitiveType.Cylinder);
            go.name = radio ? "Radio" : "Prop"; go.transform.SetParent(dynamicRoot); go.transform.position = p;
            go.transform.localScale = radio ? size : new Vector3(size.x, size.y * 0.5f, size.z);
            go.GetComponent<Renderer>().sharedMaterial = m;
            var rb = go.AddComponent<Rigidbody>(); rb.mass = radio ? 3f : 1f;
            if (radio) rb.isKinematic = true;
            var hp = go.AddComponent<HorrorProp>(); hp.IsRadio = radio;
            return go;
        }

        static void Item(ItemType type, Vector3 p)
        {
            GameObject go;
            switch (type)
            {
                case ItemType.Fuse:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.transform.localScale = new Vector3(0.12f, 0.14f, 0.12f);
                    go.transform.rotation = Quaternion.Euler(0, 0, 90); go.GetComponent<Renderer>().sharedMaterial = mFuse; break;
                case ItemType.Battery:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.transform.localScale = new Vector3(0.09f, 0.11f, 0.09f);
                    go.GetComponent<Renderer>().sharedMaterial = mBattery; break;
                default:
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.transform.localScale = new Vector3(0.35f, 0.2f, 0.25f);
                    go.GetComponent<Renderer>().sharedMaterial = mMedkit; break;
            }
            go.name = "Pickup_" + type;
            go.transform.SetParent(dynamicRoot); go.transform.position = p;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var c = go.AddComponent<SphereCollider>(); c.isTrigger = true; c.radius = 2.2f;   // big, touch-friendly
            go.AddComponent<Pickup>().type = type;
        }

        static void Note(Vector3 p, string title, string body)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Note"; go.transform.SetParent(dynamicRoot); go.transform.position = p;
            go.transform.localScale = new Vector3(0.22f, 0.01f, 0.3f); go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            go.GetComponent<Renderer>().sharedMaterial = mPaper;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var c = go.AddComponent<BoxCollider>(); c.isTrigger = true; c.size = new Vector3(2.5f, 30f, 2f);
            var pk = go.AddComponent<Pickup>(); pk.type = ItemType.Note; pk.noteTitle = title; pk.noteBody = body; pk.bobAmount = 0f;
        }

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material m, bool collider = true, Quaternion? rot = null, bool isStatic = true, bool renderer = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent);
            go.transform.position = pos; go.transform.rotation = rot ?? Quaternion.identity; go.transform.localScale = Vector3.one;
            SetWorldScale(go.transform, size);
            if (!renderer) { Object.DestroyImmediate(go.GetComponent<MeshRenderer>()); Object.DestroyImmediate(go.GetComponent<MeshFilter>()); }
            else go.GetComponent<Renderer>().sharedMaterial = m;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
            if (isStatic) GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static GameObject Cyl(string name, Transform parent, Vector3 pos, Vector3 scale, Material m, Quaternion rot)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name; go.transform.SetParent(parent); go.transform.position = pos; go.transform.rotation = rot;
            SetWorldScale(go.transform, scale);
            go.GetComponent<Renderer>().sharedMaterial = m;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        static void SetWorldScale(Transform t, Vector3 world)
        {
            Vector3 ps = t.parent != null ? t.parent.lossyScale : Vector3.one;
            t.localScale = new Vector3(world.x / ps.x, world.y / ps.y, world.z / ps.z);
        }

        static Transform Child(Transform parent, string name, Vector3 local, Quaternion localRot)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); t.localPosition = local; t.localRotation = localRot; return t;
        }

        // ================================================================== navmesh
        static void BakeNavMesh()
        {
            var surface = level.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            foreach (var c in bakeDisabled) if (c != null) c.enabled = false;
            // hiding-spot triggers must not become obstacles
            var triggers = new List<Collider>();
            foreach (var c in level.GetComponentsInChildren<Collider>()) if (c.isTrigger && c.enabled) { c.enabled = false; triggers.Add(c); }
            surface.BuildNavMesh();
            // a NavMesh baked from script must be saved as an asset, otherwise the scene loses it on reload
            string navPath = GenDir + "/NavMesh_Andhvan.asset";
            AssetDatabase.DeleteAsset(navPath);
            AssetDatabase.CreateAsset(surface.navMeshData, navPath);
            foreach (var c in bakeDisabled) if (c != null) c.enabled = true;
            foreach (var c in triggers) c.enabled = true;
        }

        // ================================================================== player
        static PlayerController BuildPlayer(Vector3 pos)
        {
            var go = new GameObject("Player");
            go.transform.position = pos; go.transform.rotation = Quaternion.Euler(0, 10, 0);
            go.tag = "Player";
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.75f; cc.radius = 0.32f; cc.center = new Vector3(0, 0.875f, 0); cc.stepOffset = 0.35f; cc.slopeLimit = 50f;
            var head = new GameObject("Head").transform; head.SetParent(go.transform, false); head.localPosition = new Vector3(0, 1.63f, 0);
            var camGo = new GameObject("PlayerCamera"); camGo.tag = "MainCamera"; camGo.transform.SetParent(head, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 70f; cam.fieldOfView = 70f;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = RenderSettings.fogColor;
            camGo.AddComponent<AudioListener>();
            var camData = cam.GetUniversalAdditionalCameraData(); camData.renderPostProcessing = true;

            // flashlight (held in right hand)
            var fl = new GameObject("Flashlight"); fl.transform.SetParent(head, false); fl.transform.localPosition = new Vector3(0.22f, -0.22f, 0.35f);
            var model = Cyl("TorchBody", fl.transform, fl.transform.position, new Vector3(0.05f, 0.1f, 0.05f), mMetal, head.rotation * Quaternion.Euler(90, 0, 0));
            GameObjectUtility.SetStaticEditorFlags(model, 0);
            Object.DestroyImmediate(model.GetComponent<Collider>());
            var lens = Cyl("Lens", fl.transform, fl.transform.position + head.forward * 0.1f, new Vector3(0.055f, 0.005f, 0.055f), mBulb, head.rotation * Quaternion.Euler(90, 0, 0));
            GameObjectUtility.SetStaticEditorFlags(lens, 0);
            Object.DestroyImmediate(lens.GetComponent<Collider>());
            var spot = new GameObject("Beam").AddComponent<Light>(); spot.transform.SetParent(fl.transform, false); spot.transform.localPosition = new Vector3(0, 0, 0.12f);
            spot.type = LightType.Spot; spot.range = 24f; spot.spotAngle = 58f; spot.innerSpotAngle = 25f; spot.intensity = 6f; spot.color = new Color(1f, 0.95f, 0.85f);
            spot.shadows = LightShadows.Soft;
            var flash = fl.AddComponent<Flashlight>(); flash.spot = spot; flash.lensRenderer = lens.GetComponent<Renderer>();

            var pc = go.AddComponent<PlayerController>(); pc.head = head; pc.cam = cam;
            go.AddComponent<PlayerHealth>();
            go.AddComponent<Inventory>();
            go.AddComponent<Interactor>();

            // rain follows the player
            var rain = new GameObject("Rain"); rain.transform.SetParent(go.transform, false); rain.transform.localPosition = new Vector3(0, 8, 2);
            var ps = rain.AddComponent<ParticleSystem>();
            var main = ps.main; main.startLifetime = 0.9f; main.startSpeed = 14f; main.startSize = 0.03f; main.maxParticles = 1200; main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(0.75f, 0.8f, 0.9f, 0.35f);
            var em = ps.emission; em.rateOverTime = 700f;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Box; sh.scale = new Vector3(22, 0.1f, 22); sh.rotation = new Vector3(90, 0, 0);
            var pr = rain.GetComponent<ParticleSystemRenderer>(); pr.renderMode = ParticleSystemRenderMode.Stretch; pr.lengthScale = 6f; pr.velocityScale = 0.04f; pr.sharedMaterial = mRain;
            rain.AddComponent<RainQuality>();
            return pc;
        }

        // ================================================================== monster & ghost
        static void BuildMonster(Vector3 pos)
        {
            var go = new GameObject("Nishachar");
            go.transform.position = pos; go.transform.rotation = Quaternion.Euler(0, 180, 0);
            var col = go.AddComponent<CapsuleCollider>(); col.height = 2.7f; col.radius = 0.35f; col.center = new Vector3(0, 1.35f, 0);
            var agent = go.AddComponent<NavMeshAgent>();
            agent.radius = 0.4f; agent.height = 2.6f; agent.angularSpeed = 400f; agent.acceleration = 14f; agent.stoppingDistance = 0.5f; agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

            var model = new GameObject("Model").transform; model.SetParent(go.transform, false);
            var hips = Child(model, "Hips", new Vector3(0, 1.15f, 0), Quaternion.identity);
            var body = Child(hips, "Body", Vector3.zero, Quaternion.identity);
            Limb("Torso", body, new Vector3(0, 0.55f, 0), new Vector3(0.42f, 0.55f, 0.28f), PrimitiveType.Capsule);
            Limb("Chest", body, new Vector3(0, 1.0f, 0.03f), new Vector3(0.55f, 0.22f, 0.32f), PrimitiveType.Capsule);
            var head = Child(body, "Head", new Vector3(0, 1.32f, 0.1f), Quaternion.identity);
            Limb("Skull", head, new Vector3(0, 0.12f, 0), new Vector3(0.3f, 0.42f, 0.32f), PrimitiveType.Sphere);
            var eyeL = Limb("EyeL", head, new Vector3(-0.07f, 0.15f, 0.15f), new Vector3(0.05f, 0.035f, 0.03f), PrimitiveType.Sphere);
            var eyeR = Limb("EyeR", head, new Vector3(0.07f, 0.15f, 0.15f), new Vector3(0.05f, 0.035f, 0.03f), PrimitiveType.Sphere);
            eyeL.GetComponent<Renderer>().sharedMaterial = mEye; eyeR.GetComponent<Renderer>().sharedMaterial = mEye;
            Limb("Jaw", head, new Vector3(0, -0.05f, 0.1f), new Vector3(0.18f, 0.1f, 0.15f), PrimitiveType.Cube);
            var armL = Child(body, "ArmL", new Vector3(-0.34f, 1.05f, 0), Quaternion.identity);
            var armR = Child(body, "ArmR", new Vector3(0.34f, 1.05f, 0), Quaternion.identity);
            foreach (var a in new[] { armL, armR })
            {
                Limb("Upper", a, new Vector3(0, -0.45f, 0), new Vector3(0.09f, 0.45f, 0.09f), PrimitiveType.Capsule);
                Limb("Lower", a, new Vector3(0, -1.15f, 0.05f), new Vector3(0.07f, 0.4f, 0.07f), PrimitiveType.Capsule);
                for (int i = -1; i <= 1; i++) Limb("Claw", a, new Vector3(i * 0.035f, -1.62f, 0.07f), new Vector3(0.015f, 0.14f, 0.015f), PrimitiveType.Cube);
            }
            var legL = Child(hips, "LegL", new Vector3(-0.14f, 0, 0), Quaternion.identity);
            var legR = Child(hips, "LegR", new Vector3(0.14f, 0, 0), Quaternion.identity);
            foreach (var l in new[] { legL, legR }) Limb("Leg", l, new Vector3(0, -0.57f, 0), new Vector3(0.1f, 0.58f, 0.1f), PrimitiveType.Capsule);

            var ai = go.AddComponent<MonsterAI>();
            ai.eyes = head; ai.patrolPoints = patrol.ToArray();
            go.AddComponent<MonsterAudio>();
            var anim = go.AddComponent<MonsterAnimator>();
            anim.body = body; anim.head = head; anim.armL = armL; anim.armR = armR; anim.legL = legL; anim.legR = legR;
            anim.eyeRenderers = new[] { eyeL.GetComponent<Renderer>(), eyeR.GetComponent<Renderer>() };

            var glow = new GameObject("EyeGlow").AddComponent<Light>(); glow.transform.SetParent(head, false); glow.transform.localPosition = new Vector3(0, 0.15f, 0.4f);
            glow.type = LightType.Point; glow.color = new Color(1f, 0.1f, 0f); glow.range = 3f; glow.intensity = 1.2f;
        }

        static GameObject Limb(string name, Transform parent, Vector3 local, Vector3 scale, PrimitiveType type)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = local; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mMonster;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GhostApparition BuildGhost()
        {
            var go = new GameObject("Ghost");
            go.transform.SetParent(dynamicRoot);
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule); body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0, 0.8f, 0); body.transform.localScale = new Vector3(0.45f, 0.8f, 0.3f);
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere); head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0, 1.75f, 0); head.transform.localScale = new Vector3(0.28f, 0.34f, 0.28f);
            var hair = GameObject.CreatePrimitive(PrimitiveType.Cube); hair.transform.SetParent(go.transform, false);
            hair.transform.localPosition = new Vector3(0, 1.55f, 0.05f); hair.transform.localScale = new Vector3(0.34f, 0.6f, 0.3f);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mGhost;
            hair.GetComponent<Renderer>().sharedMaterial = mMonster;
            foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            return go.AddComponent<GhostApparition>();
        }

        // ================================================================== systems
        static void BuildSystems(PlayerController player, GhostApparition ghost)
        {
            var sys = new GameObject("Systems");
            sys.AddComponent<GameManager>();
            sys.AddComponent<AudioManager>();
            sys.AddComponent<UIManager>();
            var director = sys.AddComponent<HorrorDirector>(); director.ghost = ghost;

            var gen = Object.FindFirstObjectByType<Generator>();
            gen.poweredLights = generatorLamps.ToArray();
            gen.gate = Object.FindFirstObjectByType<EscapeGate>();

            // post-processing volume + profile (saved as an asset)
            string profPath = GenDir + "/PP_Andhvan.asset";
            AssetDatabase.DeleteAsset(profPath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, profPath);
            var vig = profile.Add<Vignette>(true); vig.intensity.Override(0.35f); vig.smoothness.Override(0.45f);
            var ca = profile.Add<ChromaticAberration>(true); ca.intensity.Override(0f);
            var fg = profile.Add<FilmGrain>(true); fg.intensity.Override(0.3f);
            var ld = profile.Add<LensDistortion>(true); ld.intensity.Override(0f);
            var adj = profile.Add<ColorAdjustments>(true); adj.saturation.Override(-25f); adj.contrast.Override(18f); adj.colorFilter.Override(new Color(0.85f, 0.95f, 1f)); adj.postExposure.Override(0.3f);
            var tm = profile.Add<Tonemapping>(true); tm.mode.Override(TonemappingMode.ACES);
            var bl = profile.Add<Bloom>(true); bl.intensity.Override(0.6f); bl.threshold.Override(1f);
            foreach (var comp in profile.components) AssetDatabase.AddObjectToAsset(comp, profile);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var volGo = new GameObject("PostFX");
            var vol = volGo.AddComponent<Volume>(); vol.isGlobal = true; vol.sharedProfile = profile; vol.priority = 1f;
            var fx = volGo.AddComponent<FearPostFX>(); fx.volume = vol; fx.cam = player.cam; fx.health = player.GetComponent<PlayerHealth>();
        }

        static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name)) AssetDatabase.CreateFolder(parent, name);
        }
    }
}
