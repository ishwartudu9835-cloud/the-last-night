# THE LAST NIGHT — Setup Guide (Phase 1 Vertical Slice)

> **Honest status:** These scripts are **compile-checked**. I ran them with the `mcs` C# compiler against Unity API stubs, in both input modes (Input System + legacy). Every Unity API used was also checked against the real Unity 6 / URP 17 / AI Navigation 2 / Input System source.
> **But they have not yet been run inside the Unity Editor or on a phone.** For that, go through the steps below and use `TESTING.md`. If you hit any error, send me the Console message and I'll fix it.

---

## Step 0: Install (one time)
1. Install **Unity Hub**, then **Unity 6 LTS (6000.0.x)**.
   While installing, tick the **Android Build Support** module, along with **OpenJDK** and **Android SDK & NDK Tools**.
2. Disk space: about 15 GB.

## Step 1: Make the project
1. Unity Hub → **New project** → template **"Universal 3D"** (URP) → name `TheLastNight`.
2. Once it opens: **Window → Package Manager → Unity Registry** → search **"AI Navigation"** → **Install**.
   *(The Input System and uGUI are already in the template.)*

## Step 2: Connect the GitHub repo to the project (recommended)
Close Unity, then open a terminal (Git Bash / PowerShell) **inside the project folder** and run:
```bash
git init
git remote add origin https://github.com/ishwartudu9835-cloud/the-last-night.git
git fetch origin
git checkout -t origin/main
```
This brings `Assets/TheLastNight` and all the docs into your project. Then send Unity's own files (ProjectSettings, Packages) up to the repo as well:
```bash
git add -A
git commit -m "Add Unity project settings"
git push
```
> Not using Git? Then copy the repo's (or zip's) `Assets/TheLastNight` folder into your project's `Assets/` folder.

Open Unity again and wait for it to compile. **The Console must have no red errors.**
- If you see a red error, send me a screenshot or copy of it.

## Step 3: Build the scene with one click
Menu bar → **The Last Night → 1. Build Vertical Slice Scene**

This builds everything automatically (it takes ~10–30 seconds):
- the village of Andhvan: 6 houses (3 can be entered), the temple, a well, the pump-house, the gate, the forest and fence
- doors, cupboards and beds (for hiding), fuses, batteries, a medkit and story notes
- the monster "Nishachar" (with AI + patrol points)
- the NavMesh bake, lighting, fog, rain and post-processing
- the player, flashlight and every system (game, audio, UI, horror director)

The scene is saved here: `Assets/TheLastNight/Scenes/Andhvan_Slice.unity`

## Step 4: Play in the Editor
Press **Play ▶**.

| PC key | Action |
|---|---|
| WASD | Move |
| Mouse (click once in the Game view) | Look |
| Shift | Sprint |
| C | Crouch |
| E (tap/hold) | Interact (doors, pick-ups, hiding, generator) |
| F | Flashlight |
| R | Put in a spare battery |
| Tab | Inventory |
| Esc | Pause |

To test the phone controls in the Editor: **Window → General → Device Simulator**.

## Step 5: Android build
1. Menu → **The Last Night → 2. Apply Android Player Settings**
   (IL2CPP, ARM64, Android 8.0+ (API 26), landscape, package `com.cowartu.thelastnight`)
2. **File → Build Profiles → Android → Switch Platform** (the first time takes a few minutes)
3. **Project Settings → Quality**: make **Mobile** the default for Android (the template already ships a `Mobile_RPAsset`).
4. On your phone: **Developer options → USB debugging ON**, then connect it by cable.
5. Build Profiles → **Build And Run**. The APK installs on the phone and opens.

> **Play Store (Phase 6):** for that you need an **.aab** with **Build App Bundle** ticked, plus a **keystore** (Player Settings → Publishing Settings). You'll also need a Google Play Developer account ($25, one-time).

## APK automatically from GitHub (optional, once Step 2 is done)
The repo already contains `.github/workflows/android-build.yml` (GameCI). To use it:
1. Step 2's push is done (`ProjectSettings/` is in the repo).
2. Repo → **Settings → Secrets and variables → Actions → New repository secret**:
   - `UNITY_EMAIL`: your Unity account email
   - `UNITY_PASSWORD`: your Unity password
   - `UNITY_LICENSE`: the contents of your Unity Personal license file (`.ulf`). How to get it: game.ci/docs/github/activation
3. **Actions → "Android APK (Unity)" → Run workflow**. In about 20–40 minutes the APK appears as the `the-last-night-apk` artifact.

> Your Unity password goes into GitHub only as a secret. Don't put it anywhere else (not in code, not in chat).

---

## Project structure
```
Assets/TheLastNight/
├── Scripts/
│   ├── Core/         GameManager, GameSettings (presets), InputBridge, NoiseSystem, ObjectPool, ShadowQualitySwitch, RainQuality
│   ├── Player/       PlayerController (walk/sprint/crouch/stamina), Flashlight, PlayerHealth (fear), Inventory
│   ├── Interaction/  Interactor (+IInteractable), Pickup, Door, HidingSpot, Generator, EscapeGate, EscapeZone
│   ├── AI/           MonsterAI (8-state FSM), MonsterAudio, MonsterAnimator (placeholder)
│   ├── Horror/       HorrorDirector, FearPostFX, FlickerLight, GhostApparition, HorrorProp
│   ├── Audio/        AudioManager (pooled 3D audio, dynamic music), ProceduralAudio (all sounds generated in code)
│   └── UI/           UIManager (every menu + HUD), VirtualJoystick, TouchLookPad, HoldButton
├── Editor/           SliceSceneBuilder (one-click scene + Android settings)
├── Generated/        (created by the builder: materials, textures, NavMesh, PostFX profile)
└── Scenes/           (created by the builder)
```

## Where to tune things
| What | Where |
|---|---|
| Monster speed / sight / hearing / damage | Select `Nishachar` → MonsterAI in the Inspector |
| Player speed / stamina | `Player` → PlayerController |
| Battery life | `Player/Head/Flashlight` → Flashlight.batterySeconds |
| Scare frequency | `Systems` → HorrorDirector.gapSeconds |
| Number of fuses | `Systems` → GameManager.fusesRequired (and place more fuses with the builder) |
| Noise radii | `NoiseSystem.DefaultRadius()` |
| Light brightness | Builder `Lamp()` / `Bulb()` intensity, or the Inspector |

## External things (need nothing for Phase 1)
| Phase | Requirement | Cost |
|---|---|---|
| 1 (this) | None. Every sound and shape is generated in code | Free |
| 2 | Real 3D models and animations (see ASSETS.md) | Free options available |
| 4 | **Photon Fusion 2 App ID** + **Photon Voice 2 App ID** (photonengine.com) | Free up to 20 concurrent players |
| 6 | Google Play Console account, keystore | $25 one-time |
