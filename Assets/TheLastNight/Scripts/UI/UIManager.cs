// UIManager: builds the whole dark-horror UI at runtime with uGUI (no prefabs to wire):
// Main Menu, Settings (graphics/audio/controls), Loading, mobile HUD, Inventory, Note reader,
// Pause, Game Over and Ending screens. Every button here does real work.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace TheLastNight
{
    public class UIManager : MonoBehaviour
    {
        static readonly Color Blood = new Color(0.62f, 0.04f, 0.03f);
        static readonly Color Bone = new Color(0.86f, 0.82f, 0.75f);
        static readonly Color Dim = new Color(0.55f, 0.5f, 0.45f);
        static readonly Color PanelBg = new Color(0f, 0f, 0f, 0.82f);

        Font font;
        Sprite circle, rounded, white;
        Canvas canvas;
        RectTransform root;
        GameObject mainMenu, settings, loading, hud, pause, inventoryPanel, notePanel, gameOver, escaped;
        GameObject settingsReturnTo;
        Text objectiveText, messageText, promptText, loadingText, invText, noteTitle, noteBody, gameOverStats, escapedStats, fuseText;
        Image holdRing, batteryFill, staminaFill, healthFill, damageFlash, loadingFill;
        HoldButton btnInteract, btnSprint, btnCrouch, btnFlash;
        CanvasGroup hudGroup;
        RectTransform hudButtons;
        float messageUntil;
        bool damageHooked;
        GameManager gm;

        // ============================================================ setup
        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            circle = MakeCircleSprite(128, 0.9f);
            rounded = MakeRoundedSprite(64, 14);
            white = MakeRoundedSprite(8, 0);
            EnsureEventSystem();
            BuildCanvas();
            BuildMainMenu();
            BuildSettings();
            BuildLoading();
            BuildHud();
            BuildInventory();
            BuildNote();
            BuildPause();
            BuildEndScreens();
            ShowOnly(null);
        }

        void Start()
        {
            gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[TLN] UIManager needs a GameManager in the scene."); return; }
            gm.StateChanged += OnState;
            gm.ObjectiveChanged += t => objectiveText.text = t;
            gm.Message += (t, s) => { messageText.text = t; messageUntil = Time.unscaledTime + s; };
            gm.NoteOpened += (t, b) => { noteTitle.text = t; noteBody.text = b; notePanel.SetActive(true); };
            GameSettings.Changed += ApplyHudSettings;
            ApplyHudSettings();
            OnState(gm.State);
        }

        void OnDestroy() { GameSettings.Changed -= ApplyHudSettings; }

        void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        void BuildCanvas()
        {
            var go = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            root = (RectTransform)go.transform;
        }

        // ============================================================ screens
        void BuildMainMenu()
        {
            mainMenu = Panel("MainMenu", new Color(0f, 0f, 0f, 0.55f));
            var t = Label(mainMenu.transform, "THE LAST NIGHT", 120, Blood, TextAnchor.MiddleCenter);
            Place(t.rectTransform, new Vector2(0.5f, 0.78f), new Vector2(1400, 160));
            t.fontStyle = FontStyle.Bold;
            var s = Label(mainMenu.transform, "ANDHVAN  •  KARTIK AMAVASYA  •  2006 KE 37 LOG", 30, Dim, TextAnchor.MiddleCenter);
            Place(s.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(1400, 50));
            var col = Column(mainMenu.transform, new Vector2(0.5f, 0.38f), 520, 22);
            MenuButton(col, "KHELO (SOLO)", () => gm.StartGame(), true);
            MenuButton(col, "SETTINGS", () => OpenSettings(mainMenu), false);
            MenuButton(col, "BAHAR NIKLO (QUIT)", () => Application.Quit(), false);
            var v = Label(mainMenu.transform, "Phase 1 vertical slice  -  Headphones lagao", 24, Dim, TextAnchor.LowerCenter);
            Place(v.rectTransform, new Vector2(0.5f, 0.06f), new Vector2(1200, 40));
        }

        void BuildSettings()
        {
            settings = Panel("Settings", PanelBg);
            var title = Label(settings.transform, "SETTINGS", 64, Bone, TextAnchor.MiddleCenter);
            Place(title.rectTransform, new Vector2(0.5f, 0.9f), new Vector2(800, 90));

            var left = Column(settings.transform, new Vector2(0.29f, 0.5f), 760, 14);
            Header(left, "GRAPHICS");
            Cycler(left, "Quality", new[] { "Low", "Medium", "High" }, () => (int)GameSettings.Preset, i => GameSettings.Preset = (GraphicsPreset)i);
            Cycler(left, "FPS", new[] { "30", "45", "60" }, () => GameSettings.TargetFps == 60 ? 2 : GameSettings.TargetFps == 45 ? 1 : 0, i => GameSettings.TargetFps = i == 2 ? 60 : i == 1 ? 45 : 30);
            SliderRow(left, "Resolution", 0.5f, 1f, () => GameSettings.ResolutionScale, v => GameSettings.ResolutionScale = v);
            Header(left, "AUDIO");
            SliderRow(left, "Master", 0f, 1f, () => GameSettings.MasterVolume, v => { GameSettings.MasterVolume = v; AudioListener.volume = v; });
            SliderRow(left, "Music", 0f, 1f, () => GameSettings.MusicVolume, v => GameSettings.MusicVolume = v);

            var right = Column(settings.transform, new Vector2(0.71f, 0.5f), 760, 14);
            Header(right, "CONTROLS");
            SliderRow(right, "Look speed", 0.2f, 3f, () => GameSettings.LookSensitivity, v => GameSettings.LookSensitivity = v);
            ToggleRow(right, "Invert Y", () => GameSettings.InvertY, b => GameSettings.InvertY = b);
            ToggleRow(right, "Sprint = toggle", () => GameSettings.SprintToggle, b => GameSettings.SprintToggle = b);
            SliderRow(right, "HUD size", 0.7f, 1.4f, () => GameSettings.HudScale, v => GameSettings.HudScale = v);
            SliderRow(right, "HUD opacity", 0.2f, 1f, () => GameSettings.HudOpacity, v => GameSettings.HudOpacity = v);

            var back = MenuButton(settings.transform, "SAVE & BACK", CloseSettings, true);
            Place((RectTransform)back.transform, new Vector2(0.5f, 0.08f), new Vector2(520, 90));
        }

        void BuildLoading()
        {
            loading = Panel("Loading", Color.black);
            var t = Label(loading.transform, "ANDHVAN", 80, Blood, TextAnchor.MiddleCenter);
            Place(t.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(1000, 120));
            loadingText = Label(loading.transform, "", 30, Dim, TextAnchor.MiddleCenter);
            Place(loadingText.rectTransform, new Vector2(0.5f, 0.42f), new Vector2(1500, 120));
            var bar = Img(loading.transform, rounded, new Color(1, 1, 1, 0.08f));
            Place(bar.rectTransform, new Vector2(0.5f, 0.3f), new Vector2(700, 14));
            loadingFill = Fill(bar.transform, Blood);
        }

        static readonly string[] Tips =
        {
            "Torch ON ho to wo tumhe door se dekh leta hai.",
            "Daudne ki awaaz 11 metre tak jaati hai. Jhuk ke chalo.",
            "Har chhupne ki jagah surakshit nahi. Agar usne tumhe ghuste dekha... wo jaanta hai.",
            "Metal ghasitne ki awaaz suno. Wo paas hai.",
            "Generator ki awaaz poore gaon tak jaayegi. Taiyaar raho.",
        };

        void BuildHud()
        {
            hud = new GameObject("HUD", typeof(RectTransform), typeof(CanvasGroup));
            hud.transform.SetParent(root, false);
            Stretch((RectTransform)hud.transform);
            hudGroup = hud.GetComponent<CanvasGroup>();

            // look pad: whole screen, behind everything else (buttons on top get priority)
            var look = Img(hud.transform, white, new Color(0, 0, 0, 0));
            Stretch(look.rectTransform);
            look.gameObject.AddComponent<TouchLookPad>();

            // joystick zone: left 40%
            var jz = Img(hud.transform, white, new Color(0, 0, 0, 0));
            var jzr = jz.rectTransform; jzr.anchorMin = new Vector2(0, 0); jzr.anchorMax = new Vector2(0.4f, 0.75f); jzr.offsetMin = jzr.offsetMax = Vector2.zero;
            var jBase = Img(jz.transform, circle, new Color(1, 1, 1, 0.12f)); jBase.rectTransform.sizeDelta = new Vector2(240, 240);
            jBase.rectTransform.anchorMin = jBase.rectTransform.anchorMax = new Vector2(0, 0);
            var knob = Img(jBase.transform, circle, new Color(1, 1, 1, 0.35f)); knob.rectTransform.sizeDelta = new Vector2(100, 100);
            jBase.raycastTarget = knob.raycastTarget = false;
            var joy = jz.gameObject.AddComponent<VirtualJoystick>();
            joy.baseRect = jBase.rectTransform; joy.knob = knob.rectTransform;
            var jHint = Label(jz.transform, "chalne ke liye yahan dabao", 22, new Color(1, 1, 1, 0.18f), TextAnchor.LowerLeft);
            jHint.raycastTarget = false;
            Place(jHint.rectTransform, new Vector2(0.35f, 0.12f), new Vector2(500, 40));

            // top-left stats
            var stats = new GameObject("Stats", typeof(RectTransform)).GetComponent<RectTransform>();
            stats.SetParent(hud.transform, false);
            stats.anchorMin = stats.anchorMax = stats.pivot = new Vector2(0, 1);
            stats.anchoredPosition = new Vector2(48, -40); stats.sizeDelta = new Vector2(360, 140);
            healthFill = StatBar(stats, "HP", 0, new Color(0.7f, 0.08f, 0.06f));
            batteryFill = StatBar(stats, "BAT", 1, new Color(0.95f, 0.8f, 0.3f));
            staminaFill = StatBar(stats, "STA", 2, new Color(0.5f, 0.7f, 0.95f));
            fuseText = Label(stats, "", 30, new Color(1f, 0.8f, 0.3f), TextAnchor.UpperLeft);
            Place(fuseText.rectTransform, new Vector2(0, 1), new Vector2(360, 40), new Vector2(0, -130), new Vector2(0, 1));

            objectiveText = Label(hud.transform, "", 34, Bone, TextAnchor.UpperCenter);
            objectiveText.raycastTarget = false;
            Place(objectiveText.rectTransform, new Vector2(0.5f, 1f), new Vector2(1100, 60), new Vector2(0, -36), new Vector2(0.5f, 1));
            messageText = Label(hud.transform, "", 36, Color.white, TextAnchor.MiddleCenter);
            messageText.raycastTarget = false;
            Place(messageText.rectTransform, new Vector2(0.5f, 0.3f), new Vector2(1300, 120));

            // centre: crosshair, prompt, hold ring
            var dot = Img(hud.transform, circle, new Color(1, 1, 1, 0.5f)); dot.raycastTarget = false;
            Place(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(8, 8));
            holdRing = Img(hud.transform, circle, new Color(1f, 0.85f, 0.5f, 0.9f)); holdRing.raycastTarget = false;
            holdRing.type = Image.Type.Filled; holdRing.fillMethod = Image.FillMethod.Radial360; holdRing.fillAmount = 0f;
            Place(holdRing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(70, 70));
            promptText = Label(hud.transform, "", 30, Bone, TextAnchor.UpperCenter); promptText.raycastTarget = false;
            Place(promptText.rectTransform, new Vector2(0.5f, 0.44f), new Vector2(900, 50));

            // right-side buttons
            hudButtons = new GameObject("Buttons", typeof(RectTransform)).GetComponent<RectTransform>();
            hudButtons.SetParent(hud.transform, false);
            hudButtons.anchorMin = hudButtons.anchorMax = hudButtons.pivot = new Vector2(1, 0);
            hudButtons.anchoredPosition = new Vector2(-40, 40); hudButtons.sizeDelta = new Vector2(560, 520);
            btnInteract = RoundButton(hudButtons, "HAATH", new Vector2(-110, 150), 190);
            btnInteract.onHeld = b => InputBridge.TouchInteractHeld = b;
            btnSprint = RoundButton(hudButtons, "DAUDO", new Vector2(-330, 90), 140);
            btnSprint.onHeld = b => InputBridge.TouchSprintHeld = b;
            btnCrouch = RoundButton(hudButtons, "JHUKO", new Vector2(-330, 260), 120);
            btnCrouch.onTap = () => InputBridge.TouchCrouchToggle = true;
            btnFlash = RoundButton(hudButtons, "TORCH", new Vector2(-110, 360), 130);
            btnFlash.onTap = () => InputBridge.TouchFlashlightToggle = true;
            var btnInv = RoundButton(hudButtons, "BAG", new Vector2(-470, 200), 110);
            btnInv.onTap = ToggleInventory;

            var pauseBtn = RoundButton((RectTransform)hud.transform, "II", Vector2.zero, 90);
            var pr = (RectTransform)pauseBtn.transform; pr.anchorMin = pr.anchorMax = new Vector2(1, 1); pr.anchoredPosition = new Vector2(-70, -60);
            pauseBtn.onTap = () => gm.Pause();

            damageFlash = Img(root, white, new Color(0.6f, 0f, 0f, 0f)); damageFlash.raycastTarget = false;
            Stretch(damageFlash.rectTransform);
        }

        Image StatBar(RectTransform parent, string icon, int row, Color c)
        {
            var ic = Label(parent, icon, 22, c, TextAnchor.MiddleLeft);
            Place(ic.rectTransform, new Vector2(0, 1), new Vector2(60, 36), new Vector2(0, -row * 40), new Vector2(0, 1));
            var bg = Img(parent, rounded, new Color(1, 1, 1, 0.1f));
            Place(bg.rectTransform, new Vector2(0, 1), new Vector2(260, 14), new Vector2(64, -row * 40 - 11), new Vector2(0, 1));
            return Fill(bg.transform, c);
        }

        void BuildInventory()
        {
            inventoryPanel = Panel("Inventory", PanelBg);
            var t = Label(inventoryPanel.transform, "BAG", 64, Bone, TextAnchor.MiddleCenter);
            Place(t.rectTransform, new Vector2(0.5f, 0.82f), new Vector2(800, 90));
            invText = Label(inventoryPanel.transform, "", 38, Bone, TextAnchor.UpperLeft);
            Place(invText.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(800, 300));
            var col = Column(inventoryPanel.transform, new Vector2(0.5f, 0.24f), 520, 16);
            MenuButton(col, "BATTERY LAGAO", () => { var p = gm.Player; if (p != null) p.Inventory.UseBattery(); RefreshInventory(); }, true);
            MenuButton(col, "MEDKIT USE KARO", () => { var p = gm.Player; if (p != null) p.Inventory.UseMedkit(); RefreshInventory(); }, false);
            MenuButton(col, "BAND KARO", ToggleInventory, false);
        }

        void BuildNote()
        {
            notePanel = Panel("Note", new Color(0f, 0f, 0f, 0.6f));
            var paper = Img(notePanel.transform, rounded, new Color(0.82f, 0.76f, 0.62f, 0.97f));
            Place(paper.rectTransform, new Vector2(0.5f, 0.52f), new Vector2(1000, 760));
            noteTitle = Label(paper.transform, "", 44, new Color(0.25f, 0.05f, 0.02f), TextAnchor.UpperCenter);
            Place(noteTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(900, 70), new Vector2(0, -40), new Vector2(0.5f, 1));
            noteBody = Label(paper.transform, "", 32, new Color(0.15f, 0.1f, 0.08f), TextAnchor.UpperLeft);
            Place(noteBody.rectTransform, new Vector2(0.5f, 0.45f), new Vector2(860, 560));
            var close = MenuButton(notePanel.transform, "RAKH DO", () => notePanel.SetActive(false), false);
            Place((RectTransform)close.transform, new Vector2(0.5f, 0.06f), new Vector2(420, 80));
        }

        void BuildPause()
        {
            pause = Panel("Pause", PanelBg);
            var t = Label(pause.transform, "RUKO", 90, Bone, TextAnchor.MiddleCenter);
            Place(t.rectTransform, new Vector2(0.5f, 0.75f), new Vector2(800, 120));
            var col = Column(pause.transform, new Vector2(0.5f, 0.42f), 520, 20);
            MenuButton(col, "WAPAS KHELO", () => gm.Resume(), true);
            MenuButton(col, "SETTINGS", () => OpenSettings(pause), false);
            MenuButton(col, "RESTART", () => gm.Restart(), false);
            MenuButton(col, "MAIN MENU", () => gm.ToMainMenu(), false);
        }

        void BuildEndScreens()
        {
            gameOver = Panel("GameOver", new Color(0.12f, 0f, 0f, 0.88f));
            var t = Label(gameOver.transform, "WO TUMHE LE GAYA", 100, Blood, TextAnchor.MiddleCenter);
            Place(t.rectTransform, new Vector2(0.5f, 0.7f), new Vector2(1600, 140));
            gameOverStats = Label(gameOver.transform, "", 32, Dim, TextAnchor.MiddleCenter);
            Place(gameOverStats.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(1400, 100));
            var c1 = Column(gameOver.transform, new Vector2(0.5f, 0.3f), 520, 20);
            MenuButton(c1, "PHIR KOSHISH KARO", () => gm.Restart(), true);
            MenuButton(c1, "MAIN MENU", () => gm.ToMainMenu(), false);

            escaped = Panel("Escaped", new Color(0f, 0f, 0f, 0.9f));
            var e = Label(escaped.transform, "TUM BACH NIKLE", 100, Bone, TextAnchor.MiddleCenter);
            Place(e.rectTransform, new Vector2(0.5f, 0.72f), new Vector2(1600, 140));
            var e2 = Label(escaped.transform, "...par 37 log abhi bhi wahin hain. Ye sirf pehli raat thi.", 34, Blood, TextAnchor.MiddleCenter);
            Place(e2.rectTransform, new Vector2(0.5f, 0.6f), new Vector2(1600, 60));
            escapedStats = Label(escaped.transform, "", 32, Dim, TextAnchor.MiddleCenter);
            Place(escapedStats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1400, 80));
            var c2 = Column(escaped.transform, new Vector2(0.5f, 0.26f), 520, 20);
            MenuButton(c2, "PHIR SE KHELO", () => gm.Restart(), true);
            MenuButton(c2, "MAIN MENU", () => gm.ToMainMenu(), false);
        }

        // ============================================================ state & per-frame
        void OnState(GameState s)
        {
            switch (s)
            {
                case GameState.MainMenu: ShowOnly(mainMenu); break;
                case GameState.Loading:
                    ShowOnly(loading);
                    loadingText.text = Tips[UnityEngine.Random.Range(0, Tips.Length)];
                    break;
                case GameState.Playing: ShowOnly(hud); break;
                case GameState.Paused: ShowOnly(pause); hud.SetActive(false); break;
                case GameState.GameOver:
                    ShowOnly(gameOver);
                    gameOverStats.text = Stats();
                    break;
                case GameState.Escaped:
                    ShowOnly(escaped);
                    escapedStats.text = Stats() + "\nENDING: \"Pehli Raat\" (slice)";
                    break;
            }
        }

        string Stats() { return string.Format("Samay: {0}:{1:00}   •   Fuses: {2}/{3}   •   Notes: {4}", (int)(gm.PlayTime / 60), (int)(gm.PlayTime % 60), gm.FusesInserted, gm.fusesRequired, gm.NotesFound); }

        void ShowOnly(GameObject panel)
        {
            foreach (var p in new[] { mainMenu, settings, loading, hud, pause, inventoryPanel, notePanel, gameOver, escaped })
                if (p != null) p.SetActive(p == panel);
        }

        void Update()
        {
            if (gm == null) return;
            if (loading.activeSelf) loadingFill.fillAmount = ProceduralAudio.Progress;
            if (!hud.activeSelf) return;
            var p = gm.Player;
            if (p == null) return;
            if (!damageHooked) { damageHooked = true; p.Health.Damaged += a => FlashDamage(); }
            InputBridge.Poll();
            if (InputBridge.InventoryPressed) ToggleInventory();

            var it = p.GetComponent<Interactor>();
            promptText.text = it != null && it.CurrentPrompt != null ? it.CurrentPrompt : "";
            holdRing.fillAmount = it != null ? it.HoldProgress : 0f;
            if (messageText.text.Length > 0 && Time.unscaledTime > messageUntil) messageText.text = "";
            healthFill.fillAmount = p.Health.Health01;
            batteryFill.fillAmount = p.Flashlight != null ? p.Flashlight.Charge01 : 0f;
            staminaFill.fillAmount = p.Stamina01;
            staminaFill.color = p.Tired ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.5f, 0.7f, 0.95f);
            fuseText.text = string.Format("FUSE {0}   BATTERY {1}", p.Inventory.Fuses, p.Inventory.Batteries);
            btnCrouch.SetActiveVisual(p.IsCrouching);
            btnFlash.SetActiveVisual(p.Flashlight != null && p.Flashlight.IsOn);
            float hurt = 1f - p.Health.Health01;
            damageFlash.color = new Color(0.5f, 0f, 0f, Mathf.MoveTowards(damageFlash.color.a, hurt > 0.5f ? hurt * 0.12f : 0f, Time.deltaTime));
            if (inventoryPanel.activeSelf) RefreshInventory();
        }

        public void FlashDamage() { damageFlash.color = new Color(0.6f, 0f, 0f, 0.45f); }

        void ToggleInventory()
        {
            if (gm == null || !gm.IsPlaying) return;
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            RefreshInventory();
        }

        void RefreshInventory()
        {
            var p = gm.Player; if (p == null) return;
            invText.text = string.Format("Fuse:  {0}\nBattery:  {1}  (torch {2:0}%)\nMedkit:  {3}\nNotes mile:  {4}",
                p.Inventory.Fuses, p.Inventory.Batteries, p.Flashlight != null ? p.Flashlight.Charge01 * 100f : 0f, p.Inventory.Medkits, gm.NotesFound);
        }

        void OpenSettings(GameObject from) { settingsReturnTo = from; ShowOnly(settings); }
        void CloseSettings()
        {
            GameSettings.Save();
            ShowOnly(settingsReturnTo != null ? settingsReturnTo : mainMenu);
        }

        void ApplyHudSettings()
        {
            if (hudGroup != null) hudGroup.alpha = GameSettings.HudOpacity;
            if (hudButtons != null) hudButtons.localScale = Vector3.one * GameSettings.HudScale;
        }

        // ============================================================ widget helpers
        GameObject Panel(string name, Color bg)
        {
            var img = Img(root, white, bg);
            img.name = name;
            Stretch(img.rectTransform);
            return img.gameObject;
        }

        Image Img(Transform parent, Sprite sprite, Color c)
        {
            var go = new GameObject("img", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var i = go.GetComponent<Image>();
            i.sprite = sprite; i.color = c;
            if (sprite == rounded) i.type = Image.Type.Sliced;
            return i;
        }

        Image Fill(Transform parent, Color c)
        {
            var f = Img(parent, white, c);
            Stretch(f.rectTransform);
            f.type = Image.Type.Filled; f.fillMethod = Image.FillMethod.Horizontal; f.fillAmount = 1f;
            f.raycastTarget = false;
            return f;
        }

        Text Label(Transform parent, string text, int size, Color c, TextAnchor anchor)
        {
            var go = new GameObject("txt", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }

        static void Place(RectTransform r, Vector2 anchor, Vector2 size) { Place(r, anchor, size, Vector2.zero, new Vector2(0.5f, 0.5f)); }
        static void Place(RectTransform r, Vector2 anchor, Vector2 size, Vector2 offset, Vector2 pivot)
        {
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = offset;
        }

        RectTransform Column(Transform parent, Vector2 anchor, float width, float spacing)
        {
            var go = new GameObject("col", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            Place(r, anchor, new Vector2(width, 0));
            var v = go.GetComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.childControlWidth = true; v.childControlHeight = false; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            v.childAlignment = TextAnchor.MiddleCenter;
            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return r;
        }

        Button MenuButton(Transform parent, string label, Action onClick, bool primary)
        {
            var img = Img(parent, rounded, primary ? new Color(0.45f, 0.03f, 0.02f, 0.95f) : new Color(0.12f, 0.08f, 0.07f, 0.95f));
            img.name = "btn_" + label;
            ((RectTransform)img.transform).sizeDelta = new Vector2(520, 86);
            var b = img.gameObject.AddComponent<Button>();
            var cb = b.colors; cb.highlightedColor = new Color(1f, 0.85f, 0.8f); cb.pressedColor = new Color(0.6f, 0.3f, 0.3f); b.colors = cb;
            b.onClick.AddListener(() => { if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.5f); onClick(); });
            var t = Label(img.transform, label, 34, Bone, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform);
            return b;
        }

        HoldButton RoundButton(RectTransform parent, string label, Vector2 pos, float size)
        {
            var img = Img(parent, circle, new Color(1, 1, 1, 0.18f));
            img.name = "hud_" + label;
            var r = img.rectTransform; r.anchorMin = r.anchorMax = new Vector2(1, 0); r.sizeDelta = new Vector2(size, size); r.anchoredPosition = pos;
            var t = Label(img.transform, label, Mathf.RoundToInt(size * 0.2f), Bone, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform);
            var hb = img.gameObject.AddComponent<HoldButton>();
            hb.graphic = img;
            return hb;
        }

        void Header(Transform parent, string text)
        {
            var t = Label(parent, text, 30, Blood, TextAnchor.LowerLeft);
            ((RectTransform)t.transform).sizeDelta = new Vector2(700, 56);
        }

        RectTransform Row(Transform parent, string label)
        {
            var row = new GameObject("row_" + label, typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(parent, false); row.sizeDelta = new Vector2(700, 70);
            var l = Label(row, label, 30, Bone, TextAnchor.MiddleLeft);
            l.rectTransform.anchorMin = new Vector2(0, 0); l.rectTransform.anchorMax = new Vector2(0.42f, 1); l.rectTransform.offsetMin = l.rectTransform.offsetMax = Vector2.zero;
            return row;
        }

        void Cycler(Transform parent, string label, string[] options, Func<int> get, Action<int> set)
        {
            var row = Row(parent, label);
            var img = Img(row, rounded, new Color(0.15f, 0.1f, 0.09f, 1f));
            var r = img.rectTransform; r.anchorMin = new Vector2(0.45f, 0.1f); r.anchorMax = new Vector2(1f, 0.9f); r.offsetMin = r.offsetMax = Vector2.zero;
            var t = Label(img.transform, "< " + options[Mathf.Clamp(get(), 0, options.Length - 1)] + " >", 30, Color.white, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform);
            var b = img.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() =>
            {
                int i = (get() + 1) % options.Length; set(i);
                t.text = "< " + options[i] + " >";
                GameSettings.Apply();
            });
        }

        void SliderRow(Transform parent, string label, float min, float max, Func<float> get, Action<float> set)
        {
            var row = Row(parent, label);
            var go = new GameObject("slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(row, false);
            var r = (RectTransform)go.transform; r.anchorMin = new Vector2(0.45f, 0.3f); r.anchorMax = new Vector2(1f, 0.7f); r.offsetMin = r.offsetMax = Vector2.zero;
            var bg = Img(go.transform, rounded, new Color(1, 1, 1, 0.12f)); Stretch(bg.rectTransform);
            var fillArea = new GameObject("fill", typeof(RectTransform)).GetComponent<RectTransform>(); fillArea.SetParent(go.transform, false); Stretch(fillArea);
            var fill = Img(fillArea, rounded, Blood); Stretch(fill.rectTransform);
            var handleArea = new GameObject("handleArea", typeof(RectTransform)).GetComponent<RectTransform>(); handleArea.SetParent(go.transform, false); Stretch(handleArea);
            var handle = Img(handleArea, circle, Bone); handle.rectTransform.sizeDelta = new Vector2(44, 44);
            var s = go.GetComponent<Slider>();
            s.fillRect = fill.rectTransform; s.handleRect = handle.rectTransform; s.targetGraphic = handle;
            s.minValue = min; s.maxValue = max; s.value = get();
            s.onValueChanged.AddListener(v => set(v));
        }

        void ToggleRow(Transform parent, string label, Func<bool> get, Action<bool> set)
        {
            var row = Row(parent, label);
            var img = Img(row, rounded, new Color(0.15f, 0.1f, 0.09f, 1f));
            var r = img.rectTransform; r.anchorMin = new Vector2(0.45f, 0.1f); r.anchorMax = new Vector2(0.7f, 0.9f); r.offsetMin = r.offsetMax = Vector2.zero;
            var t = Label(img.transform, get() ? "ON" : "OFF", 30, Color.white, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform);
            var b = img.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() => { bool v = !get(); set(v); t.text = v ? "ON" : "OFF"; });
        }

        // ============================================================ generated sprites
        static Sprite MakeCircleSprite(int size, float hardness)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float r = size * 0.5f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                    float a = Mathf.Clamp01((1f - d) / (1f - hardness));
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px); tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeRoundedSprite(int size, int radius)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float a = 1f;
                    if (radius > 0)
                    {
                        float cx = Mathf.Clamp(x + 0.5f, radius, size - radius), cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                        float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                        a = Mathf.Clamp01(radius - d + 0.5f);
                    }
                    px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px); tex.Apply();
            float b = Mathf.Max(radius, 1);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }
    }
}
