// UIManager: the complete horror-styled UI, built at runtime with uGUI (no prefabs to wire).
// Main menu, Settings, Loading, mobile HUD, Inventory, Note reader, Pause, Game Over, Ending.
// Visual language: bone-white text on deep black, blood-red accents, typewriter body text,
// thin rings + clean icons for touch controls, context-sensitive Interact button.
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
        public static readonly Color Blood = new Color(0.72f, 0.05f, 0.04f);
        public static readonly Color Bone = new Color(0.9f, 0.86f, 0.78f);
        public static readonly Color Dim = new Color(0.58f, 0.53f, 0.47f);
        public static readonly Color Ember = new Color(1f, 0.62f, 0.25f);
        static readonly Color Ink = new Color(0.02f, 0.015f, 0.015f, 0.92f);

        Font titleFont, bodyFont, hudFont;
        RectTransform root;
        GameObject mainMenu, settings, loading, hud, pause, inventoryPanel, notePanel, gameOver, escaped;
        GameObject settingsReturnTo;
        Text titleText, objectiveText, messageText, promptText, loadingTip, invText, noteTitle, noteBody, gameOverStats, escapedStats, bagBadge;
        CanvasGroup messageGroup, promptGroup, interactGroup, staminaGroup, hudGroup;
        Image loadingFill, healthFill, staminaFill, damageVignette, fearVignette, crosshair;
        Image[] batteryCells = new Image[4];
        HoldButton btnInteract, btnSprint, btnCrouch, btnTorch;
        RectTransform hudButtons;
        float messageUntil, objectiveFlash;
        bool damageHooked;
        GameManager gm;

        // ============================================================ setup
        void Awake()
        {
            Font legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleFont = Resources.Load<Font>("Fonts/Creepster-Regular") ?? legacy;
            bodyFont = Resources.Load<Font>("Fonts/SpecialElite-Regular") ?? legacy;
            hudFont = Resources.Load<Font>("Fonts/Oswald") ?? legacy;
            EnsureEventSystem();
            BuildCanvas();
            BuildHud();              // HUD first so overlays draw above it
            BuildMainMenu();
            BuildSettings();
            BuildLoading();
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
            gm.ObjectiveChanged += t => { objectiveText.text = t.ToUpperInvariant(); objectiveFlash = 1f; };
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
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            root = (RectTransform)go.transform;
        }

        // ============================================================ main menu
        void BuildMainMenu()
        {
            mainMenu = Layer("MainMenu");
            var shade = Img(mainMenu.transform, UISprites.SideShade, new Color(0, 0, 0, 0.92f));
            Anchor(shade.rectTransform, new Vector2(0, 0), new Vector2(0.62f, 1));
            var bottom = Img(mainMenu.transform, UISprites.VerticalShade, new Color(0, 0, 0, 0.7f));
            Anchor(bottom.rectTransform, new Vector2(0, 0), new Vector2(1, 0.35f));

            titleText = Label(mainMenu.transform, "THE LAST NIGHT", titleFont, 150, Blood, TextAnchor.LowerLeft);
            Pin(titleText.rectTransform, new Vector2(0, 1), new Vector2(120, -120), new Vector2(1300, 200));
            AddShadow(titleText, new Color(0, 0, 0, 0.9f), 6);
            var tag = Label(mainMenu.transform, "Andhvan, 2006.  37 log ek hi raat mein gayab ho gaye.", bodyFont, 34, Dim, TextAnchor.UpperLeft);
            Pin(tag.rectTransform, new Vector2(0, 1), new Vector2(128, -330), new Vector2(1300, 60));
            var line = Img(mainMenu.transform, UISprites.Square, new Color(Blood.r, Blood.g, Blood.b, 0.8f));
            Pin(line.rectTransform, new Vector2(0, 1), new Vector2(128, -395), new Vector2(120, 4));

            var col = Column(mainMenu.transform, new Vector2(0, 1), new Vector2(120, -440), 560, 6, TextAnchor.UpperLeft);
            TextButton(col, "KHELO", () => gm.StartGame(), 64);
            TextButton(col, "SETTINGS", () => OpenSettings(mainMenu), 64);
            TextButton(col, "BAHAR NIKLO", () => Application.Quit(), 64);

            var hint = Label(mainMenu.transform, "Headphones lagao.  Batti band karo.", bodyFont, 26, new Color(Dim.r, Dim.g, Dim.b, 0.8f), TextAnchor.LowerLeft);
            Pin(hint.rectTransform, new Vector2(0, 0), new Vector2(128, 70), new Vector2(1000, 40));
            var ver = Label(mainMenu.transform, "CHAPTER 1  ·  PEHLI RAAT", hudFont, 24, new Color(Dim.r, Dim.g, Dim.b, 0.6f), TextAnchor.LowerRight);
            Pin(ver.rectTransform, new Vector2(1, 0), new Vector2(-80, 70), new Vector2(600, 40), new Vector2(1, 0));
        }

        // ============================================================ settings
        void BuildSettings()
        {
            settings = Layer("Settings");
            Img(settings.transform, UISprites.Square, Ink).rectTransform.Stretch();
            var title = Label(settings.transform, "SETTINGS", hudFont, 72, Bone, TextAnchor.UpperLeft);
            Pin(title.rectTransform, new Vector2(0, 1), new Vector2(120, -70), new Vector2(800, 100));
            var line = Img(settings.transform, UISprites.Square, Blood);
            Pin(line.rectTransform, new Vector2(0, 1), new Vector2(124, -175), new Vector2(90, 4));

            var left = Column(settings.transform, new Vector2(0, 1), new Vector2(120, -220), 760, 10, TextAnchor.UpperLeft);
            Header(left, "GRAPHICS");
            Cycler(left, "Quality", new[] { "LOW", "MEDIUM", "HIGH" }, () => (int)GameSettings.Preset, i => GameSettings.Preset = (GraphicsPreset)i);
            Cycler(left, "Frame rate", new[] { "30 FPS", "45 FPS", "60 FPS" }, () => GameSettings.TargetFps == 60 ? 2 : GameSettings.TargetFps == 45 ? 1 : 0, i => GameSettings.TargetFps = i == 2 ? 60 : i == 1 ? 45 : 30);
            SliderRow(left, "Resolution", 0.5f, 1f, () => GameSettings.ResolutionScale, v => GameSettings.ResolutionScale = v);
            Header(left, "AUDIO");
            SliderRow(left, "Master", 0f, 1f, () => GameSettings.MasterVolume, v => { GameSettings.MasterVolume = v; AudioListener.volume = v; });
            SliderRow(left, "Music", 0f, 1f, () => GameSettings.MusicVolume, v => GameSettings.MusicVolume = v);

            var right = Column(settings.transform, new Vector2(0, 1), new Vector2(1000, -220), 760, 10, TextAnchor.UpperLeft);
            Header(right, "CONTROLS");
            SliderRow(right, "Look speed", 0.2f, 3f, () => GameSettings.LookSensitivity, v => GameSettings.LookSensitivity = v);
            ToggleRow(right, "Invert Y", () => GameSettings.InvertY, b => GameSettings.InvertY = b);
            ToggleRow(right, "Sprint toggle", () => GameSettings.SprintToggle, b => GameSettings.SprintToggle = b);
            SliderRow(right, "Button size", 0.7f, 1.4f, () => GameSettings.HudScale, v => GameSettings.HudScale = v);
            SliderRow(right, "HUD opacity", 0.2f, 1f, () => GameSettings.HudOpacity, v => GameSettings.HudOpacity = v);

            var back = TextButton(settings.transform, "SAVE  &  WAPAS", CloseSettings, 52);
            Pin((RectTransform)back.transform, new Vector2(0, 0), new Vector2(120, 70), new Vector2(600, 80));
        }

        // ============================================================ loading
        static readonly string[] Tips =
        {
            "Torch ki roshni use door se dikhti hai.",
            "Daudne ki awaaz 11 metre tak jaati hai. Jhuk ke chalo.",
            "Har chhupne ki jagah surakshit nahi. Agar usne tumhe ghuste dekha... wo jaanta hai.",
            "Loha ghasitne ki awaaz suno. Wo paas hai.",
            "Generator ki awaaz poore gaon tak jaayegi.",
            "Wo andhere se nahi, tumhare darr se aata hai.",
        };

        void BuildLoading()
        {
            loading = Layer("Loading");
            Img(loading.transform, UISprites.Square, Color.black).rectTransform.Stretch();
            var t = Label(loading.transform, "ANDHVAN", titleFont, 120, Blood, TextAnchor.MiddleCenter);
            Pin(t.rectTransform, new Vector2(0.5f, 0.58f), Vector2.zero, new Vector2(1200, 180), new Vector2(0.5f, 0.5f));
            loadingTip = Label(loading.transform, "", bodyFont, 34, Dim, TextAnchor.MiddleCenter);
            Pin(loadingTip.rectTransform, new Vector2(0.5f, 0.4f), Vector2.zero, new Vector2(1400, 120), new Vector2(0.5f, 0.5f));
            var bar = Img(loading.transform, UISprites.Square, new Color(1, 1, 1, 0.08f));
            Pin(bar.rectTransform, new Vector2(0.5f, 0.26f), Vector2.zero, new Vector2(520, 3), new Vector2(0.5f, 0.5f));
            loadingFill = Fill(bar.transform, Blood);
        }

        // ============================================================ HUD
        void BuildHud()
        {
            hud = Layer("HUD");
            hudGroup = hud.AddComponent<CanvasGroup>();

            // full-screen vignettes (no raycast)
            fearVignette = Img(hud.transform, UISprites.Vignette, new Color(0, 0, 0, 0.6f)); fearVignette.rectTransform.Stretch(); fearVignette.raycastTarget = false;
            damageVignette = Img(hud.transform, UISprites.Vignette, new Color(0.55f, 0f, 0f, 0f)); damageVignette.rectTransform.Stretch(); damageVignette.raycastTarget = false;

            // look pad: whole screen, under all controls
            var look = Img(hud.transform, UISprites.Square, new Color(0, 0, 0, 0)); look.rectTransform.Stretch();
            look.gameObject.AddComponent<TouchLookPad>();

            // joystick zone: left 42%, lower 80%
            var jz = Img(hud.transform, UISprites.Square, new Color(0, 0, 0, 0));
            Anchor(jz.rectTransform, Vector2.zero, new Vector2(0.42f, 0.8f));
            var jBaseGo = new GameObject("JoyBase", typeof(RectTransform), typeof(CanvasGroup));
            jBaseGo.transform.SetParent(jz.transform, false);
            var jBase = (RectTransform)jBaseGo.transform;
            jBase.anchorMin = jBase.anchorMax = Vector2.zero; jBase.sizeDelta = new Vector2(260, 260); jBase.anchoredPosition = new Vector2(260, 250);
            var jFill = Img(jBase, UISprites.SoftCircle, new Color(0, 0, 0, 0.35f)); jFill.rectTransform.Stretch(); jFill.raycastTarget = false;
            var jRing = Img(jBase, UISprites.Ring, new Color(Bone.r, Bone.g, Bone.b, 0.5f)); jRing.rectTransform.Stretch(); jRing.raycastTarget = false;
            for (int i = 0; i < 4; i++)
            {
                var tick = Img(jBase, UISprites.Square, new Color(Bone.r, Bone.g, Bone.b, 0.35f)); tick.raycastTarget = false;
                tick.rectTransform.sizeDelta = new Vector2(4, 14);
                tick.rectTransform.localRotation = Quaternion.Euler(0, 0, i * 90);
                tick.rectTransform.anchoredPosition = (Vector2)(Quaternion.Euler(0, 0, i * 90) * new Vector3(0, 104, 0));
            }
            var knob = Img(jBase, UISprites.Circle, new Color(Bone.r, Bone.g, Bone.b, 0.85f)); knob.rectTransform.sizeDelta = new Vector2(96, 96); knob.raycastTarget = false;
            var knobInner = Img(knob.transform, UISprites.Circle, new Color(0.1f, 0.07f, 0.06f, 0.9f)); knobInner.rectTransform.sizeDelta = new Vector2(70, 70); knobInner.raycastTarget = false;
            var joy = jz.gameObject.AddComponent<VirtualJoystick>();
            joy.baseRect = jBase; joy.knob = knob.rectTransform; joy.group = jBaseGo.GetComponent<CanvasGroup>(); joy.radius = 105f;

            // ---- top-left vitals: heart + bar, battery with 4 cells
            var vit = Rect(hud.transform, "Vitals", new Vector2(0, 1), new Vector2(56, -44), new Vector2(420, 120), new Vector2(0, 1));
            var heart = Img(vit, UISprites.IconHeart, new Color(0.85f, 0.15f, 0.12f, 0.95f)); Pin(heart.rectTransform, new Vector2(0, 1), Vector2.zero, new Vector2(38, 38), new Vector2(0, 1));
            var hpBg = Img(vit, UISprites.Pill, new Color(1, 1, 1, 0.12f)); Pin(hpBg.rectTransform, new Vector2(0, 1), new Vector2(56, -14), new Vector2(240, 10), new Vector2(0, 1));
            healthFill = Fill(hpBg.transform, new Color(0.8f, 0.12f, 0.1f));
            var bat = Img(vit, UISprites.IconBattery, new Color(Bone.r, Bone.g, Bone.b, 0.9f)); Pin(bat.rectTransform, new Vector2(0, 1), new Vector2(-4, -54), new Vector2(46, 46), new Vector2(0, 1));
            for (int i = 0; i < 4; i++)
            {
                var c = Img(vit, UISprites.Rounded, Ember);
                Pin(c.rectTransform, new Vector2(0, 1), new Vector2(56 + i * 30, -64), new Vector2(24, 26), new Vector2(0, 1));
                batteryCells[i] = c;
            }

            // ---- objective (top centre)
            var obj = Rect(hud.transform, "Objective", new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(1100, 70), new Vector2(0.5f, 1));
            var oline = Img(obj, UISprites.Square, Blood); Pin(oline.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -2), new Vector2(60, 3), new Vector2(0.5f, 1));
            objectiveText = Label(obj, "", hudFont, 34, Bone, TextAnchor.UpperCenter);
            Pin(objectiveText.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(1100, 56), new Vector2(0.5f, 1));
            AddShadow(objectiveText, new Color(0, 0, 0, 0.85f), 2);

            // ---- message toast
            var msg = Rect(hud.transform, "Message", new Vector2(0.5f, 0.27f), Vector2.zero, new Vector2(1400, 110), new Vector2(0.5f, 0.5f));
            messageGroup = msg.gameObject.AddComponent<CanvasGroup>(); messageGroup.alpha = 0f;
            messageText = Label(msg, "", bodyFont, 38, Bone, TextAnchor.MiddleCenter); messageText.rectTransform.Stretch();
            AddShadow(messageText, new Color(0, 0, 0, 0.95f), 3);

            // ---- centre: crosshair + prompt
            crosshair = Img(hud.transform, UISprites.Circle, new Color(1, 1, 1, 0.55f)); crosshair.raycastTarget = false;
            Pin(crosshair.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8, 8), new Vector2(0.5f, 0.5f));
            var prm = Rect(hud.transform, "Prompt", new Vector2(0.5f, 0.5f), new Vector2(0, -70), new Vector2(900, 60), new Vector2(0.5f, 0.5f));
            promptGroup = prm.gameObject.AddComponent<CanvasGroup>(); promptGroup.alpha = 0f;
            promptText = Label(prm, "", hudFont, 32, Bone, TextAnchor.MiddleCenter); promptText.rectTransform.Stretch();
            AddShadow(promptText, new Color(0, 0, 0, 0.9f), 2);

            // ---- stamina (thin bar under the crosshair, only while used)
            var st = Rect(hud.transform, "Stamina", new Vector2(0.5f, 0.5f), new Vector2(0, -120), new Vector2(220, 6), new Vector2(0.5f, 0.5f));
            staminaGroup = st.gameObject.AddComponent<CanvasGroup>(); staminaGroup.alpha = 0f;
            var stBg = Img(st, UISprites.Pill, new Color(1, 1, 1, 0.15f)); stBg.rectTransform.Stretch();
            staminaFill = Fill(stBg.transform, Bone);

            // ---- right-hand controls
            hudButtons = Rect(hud.transform, "Buttons", new Vector2(1, 0), new Vector2(-30, 30), new Vector2(620, 600), new Vector2(1, 0));
            btnInteract = IconButton(hudButtons, UISprites.IconHand, "", new Vector2(-150, 200), 200, true);
            btnInteract.onHeld = b => InputBridge.TouchInteractHeld = b;
            interactGroup = btnInteract.gameObject.AddComponent<CanvasGroup>();
            btnSprint = IconButton(hudButtons, UISprites.IconRun, "DAUDO", new Vector2(-375, 120), 140, false);
            btnSprint.onHeld = b => InputBridge.TouchSprintHeld = b;
            btnCrouch = IconButton(hudButtons, UISprites.IconCrouch, "JHUKO", new Vector2(-395, 305), 116, false);
            btnCrouch.onTap = () => InputBridge.TouchCrouchToggle = true;
            btnTorch = IconButton(hudButtons, UISprites.IconTorch, "TORCH", new Vector2(-150, 420), 116, false);
            btnTorch.onTap = () => InputBridge.TouchFlashlightToggle = true;
            var bag = IconButton(hudButtons, UISprites.IconBag, "BAG", new Vector2(-290, 470), 96, false);
            bag.onTap = ToggleInventory;
            var badge = Img(bag.transform, UISprites.Circle, Blood); Pin(badge.rectTransform, new Vector2(1, 1), new Vector2(4, 4), new Vector2(34, 34), new Vector2(1, 1)); badge.raycastTarget = false;
            bagBadge = Label(badge.transform, "", hudFont, 22, Color.white, TextAnchor.MiddleCenter); bagBadge.rectTransform.Stretch();

            var pauseBtn = IconButton((RectTransform)hud.transform, UISprites.IconPause, "", Vector2.zero, 84, false);
            var pr = (RectTransform)pauseBtn.transform; pr.anchorMin = pr.anchorMax = new Vector2(1, 1); pr.anchoredPosition = new Vector2(-70, -64);
            pauseBtn.onTap = () => gm.Pause();
        }

        HoldButton IconButton(RectTransform parent, Sprite icon, string label, Vector2 pos, float size, bool withProgress)
        {
            var go = new GameObject("btn_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = new Vector2(1, 0); r.sizeDelta = new Vector2(size, size); r.anchoredPosition = pos;
            var bg = Img(r, UISprites.Circle, new Color(0, 0, 0, 0.35f)); bg.rectTransform.Stretch();   // raycast target
            var ring = Img(r, UISprites.Ring, Bone); ring.rectTransform.Stretch(); ring.raycastTarget = false;
            var ic = Img(r, icon, Bone); ic.raycastTarget = false; ic.rectTransform.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            Image prog = null;
            if (withProgress)
            {
                prog = Img(r, UISprites.RingThick, Ember); prog.rectTransform.Stretch(); prog.raycastTarget = false;
                prog.type = Image.Type.Filled; prog.fillMethod = Image.FillMethod.Radial360; prog.fillOrigin = 2; prog.fillAmount = 0f;
            }
            if (label.Length > 0)
            {
                var t = Label(r, label, hudFont, 22, new Color(Bone.r, Bone.g, Bone.b, 0.75f), TextAnchor.UpperCenter);
                Pin(t.rectTransform, new Vector2(0.5f, 0), new Vector2(0, -6), new Vector2(200, 30), new Vector2(0.5f, 1));
            }
            var hb = go.AddComponent<HoldButton>();
            hb.background = bg; hb.ring = ring; hb.icon = ic; hb.progress = prog;
            return hb;
        }

        // ============================================================ inventory / note
        void BuildInventory()
        {
            inventoryPanel = Layer("Inventory");
            Img(inventoryPanel.transform, UISprites.Square, new Color(0, 0, 0, 0.75f)).rectTransform.Stretch();
            var card = Img(inventoryPanel.transform, UISprites.Rounded, new Color(0.06f, 0.045f, 0.04f, 0.97f));
            Pin(card.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 640), new Vector2(0.5f, 0.5f));
            var t = Label(card.transform, "BAG", hudFont, 60, Bone, TextAnchor.UpperLeft);
            Pin(t.rectTransform, new Vector2(0, 1), new Vector2(60, -40), new Vector2(600, 80), new Vector2(0, 1));
            invText = Label(card.transform, "", bodyFont, 36, Bone, TextAnchor.UpperLeft);
            Pin(invText.rectTransform, new Vector2(0, 1), new Vector2(60, -140), new Vector2(780, 260), new Vector2(0, 1));
            var row = Rect(card.transform, "Actions", new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(800, 90), new Vector2(0.5f, 0));
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 20; h.childControlWidth = true; h.childControlHeight = true; h.childForceExpandWidth = true; h.childForceExpandHeight = true;
            PillButton(row, "BATTERY LAGAO", () => { var p = gm.Player; if (p != null) p.Inventory.UseBattery(); RefreshInventory(); }, true);
            PillButton(row, "MEDKIT", () => { var p = gm.Player; if (p != null) p.Inventory.UseMedkit(); RefreshInventory(); }, false);
            PillButton(row, "BAND", ToggleInventory, false);
        }

        void BuildNote()
        {
            notePanel = Layer("Note");
            Img(notePanel.transform, UISprites.Square, new Color(0, 0, 0, 0.7f)).rectTransform.Stretch();
            var paper = Img(notePanel.transform, UISprites.Paper, Color.white);
            Pin(paper.rectTransform, new Vector2(0.5f, 0.53f), Vector2.zero, new Vector2(980, 780), new Vector2(0.5f, 0.5f));
            paper.rectTransform.localRotation = Quaternion.Euler(0, 0, -1.5f);
            noteTitle = Label(paper.transform, "", bodyFont, 42, new Color(0.35f, 0.04f, 0.02f), TextAnchor.UpperLeft);
            Pin(noteTitle.rectTransform, new Vector2(0, 1), new Vector2(70, -60), new Vector2(840, 70), new Vector2(0, 1));
            noteBody = Label(paper.transform, "", bodyFont, 32, new Color(0.14f, 0.1f, 0.08f), TextAnchor.UpperLeft);
            Pin(noteBody.rectTransform, new Vector2(0, 1), new Vector2(70, -150), new Vector2(840, 560), new Vector2(0, 1));
            noteBody.lineSpacing = 1.15f;
            var close = TextButton(notePanel.transform, "RAKH DO", () => notePanel.SetActive(false), 44);
            Pin((RectTransform)close.transform, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(400, 70), new Vector2(0.5f, 0));
            ((Text)close.targetGraphic).alignment = TextAnchor.MiddleCenter;
        }

        // ============================================================ pause / end
        void BuildPause()
        {
            pause = Layer("Pause");
            Img(pause.transform, UISprites.Square, new Color(0, 0, 0, 0.82f)).rectTransform.Stretch();
            var t = Label(pause.transform, "RUKO", titleFont, 140, Bone, TextAnchor.LowerLeft);
            Pin(t.rectTransform, new Vector2(0, 1), new Vector2(120, -120), new Vector2(1000, 200));
            var col = Column(pause.transform, new Vector2(0, 1), new Vector2(120, -380), 560, 6, TextAnchor.UpperLeft);
            TextButton(col, "WAPAS KHELO", () => gm.Resume(), 60);
            TextButton(col, "SETTINGS", () => OpenSettings(pause), 60);
            TextButton(col, "RESTART", () => gm.Restart(), 60);
            TextButton(col, "MAIN MENU", () => gm.ToMainMenu(), 60);
        }

        void BuildEndScreens()
        {
            gameOver = Layer("GameOver");
            var red = Img(gameOver.transform, UISprites.Vignette, new Color(0.45f, 0f, 0f, 1f)); red.rectTransform.Stretch();
            Img(gameOver.transform, UISprites.Square, new Color(0, 0, 0, 0.7f)).rectTransform.Stretch();
            var t = Label(gameOver.transform, "WO TUMHE LE GAYA", titleFont, 140, Blood, TextAnchor.MiddleCenter);
            Pin(t.rectTransform, new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(1700, 200), new Vector2(0.5f, 0.5f));
            AddShadow(t, Color.black, 6);
            gameOverStats = Label(gameOver.transform, "", hudFont, 32, Dim, TextAnchor.MiddleCenter);
            Pin(gameOverStats.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 60), new Vector2(0.5f, 0.5f));
            var r1 = Rect(gameOver.transform, "Buttons", new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(900, 90), new Vector2(0.5f, 0.5f));
            var h1 = r1.gameObject.AddComponent<HorizontalLayoutGroup>(); h1.spacing = 30; h1.childControlWidth = true; h1.childControlHeight = true; h1.childForceExpandWidth = true; h1.childForceExpandHeight = true;
            PillButton(r1, "PHIR KOSHISH", () => gm.Restart(), true);
            PillButton(r1, "MAIN MENU", () => gm.ToMainMenu(), false);

            escaped = Layer("Escaped");
            Img(escaped.transform, UISprites.Square, new Color(0, 0, 0, 0.92f)).rectTransform.Stretch();
            var e = Label(escaped.transform, "TUM BACH NIKLE", titleFont, 140, Bone, TextAnchor.MiddleCenter);
            Pin(e.rectTransform, new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(1700, 200), new Vector2(0.5f, 0.5f));
            var e2 = Label(escaped.transform, "...par 37 log abhi bhi wahin hain. Ye sirf pehli raat thi.", bodyFont, 36, Blood, TextAnchor.MiddleCenter);
            Pin(e2.rectTransform, new Vector2(0.5f, 0.55f), Vector2.zero, new Vector2(1600, 70), new Vector2(0.5f, 0.5f));
            escapedStats = Label(escaped.transform, "", hudFont, 30, Dim, TextAnchor.MiddleCenter);
            Pin(escapedStats.rectTransform, new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(1400, 100), new Vector2(0.5f, 0.5f));
            var r2 = Rect(escaped.transform, "Buttons", new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(900, 90), new Vector2(0.5f, 0.5f));
            var h2 = r2.gameObject.AddComponent<HorizontalLayoutGroup>(); h2.spacing = 30; h2.childControlWidth = true; h2.childControlHeight = true; h2.childForceExpandWidth = true; h2.childForceExpandHeight = true;
            PillButton(r2, "PHIR SE KHELO", () => gm.Restart(), true);
            PillButton(r2, "MAIN MENU", () => gm.ToMainMenu(), false);
        }

        // ============================================================ state & per-frame
        void OnState(GameState s)
        {
            switch (s)
            {
                case GameState.MainMenu: ShowOnly(mainMenu); break;
                case GameState.Loading:
                    ShowOnly(loading);
                    loadingTip.text = Tips[UnityEngine.Random.Range(0, Tips.Length)];
                    break;
                case GameState.Playing: ShowOnly(hud); break;
                case GameState.Paused: ShowOnly(pause); break;
                case GameState.GameOver: ShowOnly(gameOver); gameOverStats.text = Stats(); break;
                case GameState.Escaped: ShowOnly(escaped); escapedStats.text = Stats() + "\nENDING  ·  PEHLI RAAT"; break;
            }
        }

        string Stats() { return string.Format("SAMAY {0}:{1:00}     FUSE {2}/{3}     NOTES {4}", (int)(gm.PlayTime / 60), (int)(gm.PlayTime % 60), gm.FusesInserted, gm.fusesRequired, gm.NotesFound); }

        void ShowOnly(GameObject panel)
        {
            foreach (var p in new[] { mainMenu, settings, loading, hud, pause, inventoryPanel, notePanel, gameOver, escaped })
                if (p != null) p.SetActive(p == panel);
        }

        void Update()
        {
            if (gm == null) return;
            float dt = Time.unscaledDeltaTime;
            if (mainMenu.activeSelf)
            {
                // flickering title
                float f = Mathf.PerlinNoise(Time.unscaledTime * 3f, 0f) > 0.78f ? 0.35f : 1f;
                titleText.color = new Color(Blood.r, Blood.g, Blood.b, Mathf.MoveTowards(titleText.color.a, f, dt * 12f));
            }
            if (loading.activeSelf) loadingFill.fillAmount = ProceduralAudio.Progress;
            if (!hud.activeSelf) return;
            var p = gm.Player;
            if (p == null) return;
            if (!damageHooked) { damageHooked = true; p.Health.Damaged += a => damageVignette.color = new Color(0.6f, 0f, 0f, 0.85f); }
            InputBridge.Poll();
            if (InputBridge.InventoryPressed) ToggleInventory();

            // prompt + context-sensitive interact button
            var it = p.GetComponent<Interactor>();
            bool canInteract = it != null && it.CurrentPrompt != null;
            if (canInteract) promptText.text = it.CurrentPrompt.ToUpperInvariant();
            promptGroup.alpha = Mathf.MoveTowards(promptGroup.alpha, canInteract ? 1f : 0f, dt * 6f);
            interactGroup.alpha = Mathf.MoveTowards(interactGroup.alpha, canInteract ? 1f : 0.28f, dt * 6f);
            btnInteract.SetActiveVisual(canInteract);
            btnInteract.SetProgress(it != null ? it.HoldProgress : 0f);
            crosshair.rectTransform.sizeDelta = Vector2.one * Mathf.MoveTowards(crosshair.rectTransform.sizeDelta.x, canInteract ? 18f : 8f, dt * 80f);
            crosshair.color = canInteract ? Ember : new Color(1, 1, 1, 0.55f);

            // message toast + objective flash
            messageGroup.alpha = Mathf.MoveTowards(messageGroup.alpha, Time.unscaledTime < messageUntil ? 1f : 0f, dt * 3f);
            objectiveFlash = Mathf.MoveTowards(objectiveFlash, 0f, dt * 0.8f);
            objectiveText.color = Color.Lerp(Bone, Ember, objectiveFlash);

            // vitals
            healthFill.fillAmount = Mathf.MoveTowards(healthFill.fillAmount, p.Health.Health01, dt * 2f);
            float charge = p.Flashlight != null ? p.Flashlight.Charge01 : 0f;
            for (int i = 0; i < 4; i++)
            {
                float cell = Mathf.Clamp01(charge * 4f - i);
                bool low = charge < 0.18f;
                float blink = low ? (Mathf.Sin(Time.unscaledTime * 10f) > 0 ? 1f : 0.3f) : 1f;
                batteryCells[i].color = new Color(low ? 0.9f : Ember.r, low ? 0.15f : Ember.g, low ? 0.1f : Ember.b, (cell > 0.05f ? 0.3f + 0.7f * cell : 0.08f) * blink);
            }
            bool usingStamina = p.Stamina01 < 0.99f;
            staminaGroup.alpha = Mathf.MoveTowards(staminaGroup.alpha, usingStamina ? 1f : 0f, dt * 3f);
            staminaFill.fillAmount = p.Stamina01;
            staminaFill.color = p.Tired ? new Color(0.5f, 0.5f, 0.5f) : Bone;

            btnCrouch.SetActiveVisual(p.IsCrouching);
            btnTorch.SetActiveVisual(p.Flashlight != null && p.Flashlight.IsOn);
            btnSprint.SetActiveVisual(p.IsSprinting);
            int items = p.Inventory.Batteries + p.Inventory.Fuses + p.Inventory.Medkits;
            bagBadge.text = items > 0 ? items.ToString() : "";
            bagBadge.transform.parent.gameObject.SetActive(items > 0);

            // fear + damage vignettes
            float fear = p.Health.Fear, hurt = 1f - p.Health.Health01;
            fearVignette.color = new Color(0, 0, 0, 0.45f + fear * 0.4f);
            float pulse = hurt > 0.5f ? (Mathf.Sin(Time.unscaledTime * 6f) * 0.5f + 0.5f) * hurt * 0.4f : 0f;
            var dc = damageVignette.color; dc.a = Mathf.Max(Mathf.MoveTowards(dc.a, 0f, dt * 1.2f), pulse); damageVignette.color = dc;

            if (inventoryPanel.activeSelf) RefreshInventory();
        }

        void ToggleInventory()
        {
            if (gm == null || !gm.IsPlaying) return;
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            RefreshInventory();
        }

        void RefreshInventory()
        {
            var p = gm.Player; if (p == null) return;
            invText.text = string.Format("Fuse            {0}\nBattery         {1}     (torch {2:0}%)\nMedkit          {3}\nNotes mile      {4}",
                p.Inventory.Fuses, p.Inventory.Batteries, p.Flashlight != null ? p.Flashlight.Charge01 * 100f : 0f, p.Inventory.Medkits, gm.NotesFound);
        }

        void OpenSettings(GameObject from) { settingsReturnTo = from; ShowOnly(settings); }
        void CloseSettings() { GameSettings.Save(); ShowOnly(settingsReturnTo != null ? settingsReturnTo : mainMenu); }

        void ApplyHudSettings()
        {
            if (hudGroup != null) hudGroup.alpha = Mathf.Lerp(0.5f, 1f, GameSettings.HudOpacity);
            if (hudButtons != null) hudButtons.localScale = Vector3.one * GameSettings.HudScale;
        }

        // ============================================================ widget helpers
        GameObject Layer(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(root, false);
            ((RectTransform)go.transform).Stretch();
            return go;
        }

        RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            r.SetParent(parent, false);
            Pin(r, anchor, pos, size, pivot);
            return r;
        }

        Image Img(Transform parent, Sprite sprite, Color c)
        {
            var go = new GameObject("img", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var i = go.GetComponent<Image>();
            i.sprite = sprite; i.color = c;
            if (sprite == UISprites.Rounded || sprite == UISprites.Pill || sprite == UISprites.Square) i.type = Image.Type.Sliced;
            return i;
        }

        Image Fill(Transform parent, Color c)
        {
            var f = Img(parent, UISprites.Pill, c);
            f.rectTransform.Stretch();
            f.type = Image.Type.Filled; f.fillMethod = Image.FillMethod.Horizontal; f.fillAmount = 1f;
            f.raycastTarget = false;
            return f;
        }

        Text Label(Transform parent, string text, Font font, int size, Color c, TextAnchor anchor)
        {
            var go = new GameObject("txt", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font; t.text = text; t.fontSize = size; t.color = c; t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        static void AddShadow(Text t, Color c, float dist)
        {
            var s = t.gameObject.AddComponent<UnityEngine.UI.Shadow>();
            s.effectColor = c; s.effectDistance = new Vector2(dist, -dist);
        }

        static void Anchor(RectTransform r, Vector2 min, Vector2 max) { r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero; }
        static void Pin(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size) { Pin(r, anchor, pos, size, new Vector2(0, 1)); }
        static void Pin(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size, Vector2 pivot)
        {
            r.anchorMin = r.anchorMax = anchor; r.pivot = pivot; r.sizeDelta = size; r.anchoredPosition = pos;
        }

        RectTransform Column(Transform parent, Vector2 anchor, Vector2 pos, float width, float spacing, TextAnchor align)
        {
            var go = new GameObject("col", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            var r = (RectTransform)go.transform;
            Pin(r, anchor, pos, new Vector2(width, 0), new Vector2(0, 1));
            var v = go.GetComponent<VerticalLayoutGroup>();
            v.spacing = spacing; v.childControlWidth = true; v.childControlHeight = false; v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            v.childAlignment = align;
            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return r;
        }

        /// <summary>Menu item: plain bone text that turns blood-red with a dash when pressed.</summary>
        Button TextButton(Transform parent, string label, Action onClick, int size)
        {
            var t = Label(parent, label, hudFont, size, Bone, TextAnchor.MiddleLeft);
            t.raycastTarget = true;
            ((RectTransform)t.transform).sizeDelta = new Vector2(560, size * 1.35f);
            var b = t.gameObject.AddComponent<Button>();
            b.targetGraphic = t;
            var cb = b.colors;
            cb.normalColor = Color.white; cb.highlightedColor = new Color(1f, 0.55f, 0.45f); cb.pressedColor = new Color(0.9f, 0.1f, 0.06f); cb.selectedColor = Color.white;
            cb.fadeDuration = 0.08f; b.colors = cb;
            b.onClick.AddListener(() => { if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.5f); onClick(); });
            return b;
        }

        Button PillButton(Transform parent, string label, Action onClick, bool primary)
        {
            var img = Img(parent, UISprites.Pill, primary ? new Color(0.55f, 0.04f, 0.03f, 0.95f) : new Color(1, 1, 1, 0.07f));
            ((RectTransform)img.transform).sizeDelta = new Vector2(260, 84);
            var b = img.gameObject.AddComponent<Button>();
            var cb = b.colors; cb.highlightedColor = new Color(1f, 0.85f, 0.8f); cb.pressedColor = new Color(0.6f, 0.3f, 0.3f); b.colors = cb;
            b.onClick.AddListener(() => { if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.5f); onClick(); });
            var t = Label(img.transform, label, hudFont, 30, Bone, TextAnchor.MiddleCenter);
            t.rectTransform.Stretch();
            return b;
        }

        void Header(Transform parent, string text)
        {
            var t = Label(parent, text, hudFont, 28, Blood, TextAnchor.LowerLeft);
            ((RectTransform)t.transform).sizeDelta = new Vector2(700, 64);
        }

        RectTransform Row(Transform parent, string label)
        {
            var row = new GameObject("row_" + label, typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(parent, false); row.sizeDelta = new Vector2(740, 72);
            var l = Label(row, label, bodyFont, 32, Bone, TextAnchor.MiddleLeft);
            Anchor(l.rectTransform, new Vector2(0, 0), new Vector2(0.42f, 1));
            var sep = Img(row, UISprites.Square, new Color(1, 1, 1, 0.06f));
            Anchor(sep.rectTransform, new Vector2(0, 0), new Vector2(1, 0)); sep.rectTransform.sizeDelta = new Vector2(0, 2);
            return row;
        }

        void Cycler(Transform parent, string label, string[] options, Func<int> get, Action<int> set)
        {
            var row = Row(parent, label);
            var t = Label(row, "", hudFont, 32, Ember, TextAnchor.MiddleRight);
            Anchor(t.rectTransform, new Vector2(0.45f, 0), new Vector2(1, 1));
            t.raycastTarget = true;
            Action refresh = () => t.text = "‹   " + options[Mathf.Clamp(get(), 0, options.Length - 1)] + "   ›";
            refresh();
            var b = t.gameObject.AddComponent<Button>();
            b.targetGraphic = t;
            b.onClick.AddListener(() => { set((get() + 1) % options.Length); refresh(); GameSettings.Apply(); if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.4f); });
        }

        void SliderRow(Transform parent, string label, float min, float max, Func<float> get, Action<float> set)
        {
            var row = Row(parent, label);
            var go = new GameObject("slider", typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(row, false);
            var r = (RectTransform)go.transform; Anchor(r, new Vector2(0.48f, 0.3f), new Vector2(0.98f, 0.7f));
            var bg = Img(go.transform, UISprites.Pill, new Color(1, 1, 1, 0.12f)); Anchor(bg.rectTransform, new Vector2(0, 0.4f), new Vector2(1, 0.6f));
            var fillArea = new GameObject("fill", typeof(RectTransform)).GetComponent<RectTransform>(); fillArea.SetParent(go.transform, false); Anchor(fillArea, new Vector2(0, 0.4f), new Vector2(1, 0.6f));
            var fill = Img(fillArea, UISprites.Pill, Blood); fill.rectTransform.Stretch();
            var handleArea = new GameObject("handleArea", typeof(RectTransform)).GetComponent<RectTransform>(); handleArea.SetParent(go.transform, false); handleArea.Stretch();
            var handle = Img(handleArea, UISprites.Circle, Bone); handle.rectTransform.sizeDelta = new Vector2(40, 40);
            var s = go.GetComponent<Slider>();
            s.fillRect = fill.rectTransform; s.handleRect = handle.rectTransform; s.targetGraphic = handle;
            s.minValue = min; s.maxValue = max; s.value = get();
            s.onValueChanged.AddListener(v => set(v));
        }

        void ToggleRow(Transform parent, string label, Func<bool> get, Action<bool> set)
        {
            var row = Row(parent, label);
            var track = Img(row, UISprites.Pill, new Color(1, 1, 1, 0.12f));
            Pin(track.rectTransform, new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(110, 50), new Vector2(1, 0.5f));
            var knob = Img(track.transform, UISprites.Circle, Bone); knob.raycastTarget = false;
            Action refresh = () =>
            {
                bool on = get();
                track.color = on ? new Color(Blood.r, Blood.g, Blood.b, 0.9f) : new Color(1, 1, 1, 0.12f);
                Pin(knob.rectTransform, new Vector2(on ? 1 : 0, 0.5f), new Vector2(on ? -5 : 5, 0), new Vector2(40, 40), new Vector2(on ? 1 : 0, 0.5f));
            };
            refresh();
            var b = track.gameObject.AddComponent<Button>();
            b.onClick.AddListener(() => { set(!get()); refresh(); if (AudioManager.Instance != null) AudioManager.Instance.PlayUI(ProceduralAudio.Click, 0.4f); });
        }
    }

    static class RectExt
    {
        public static void Stretch(this RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
    }
}
