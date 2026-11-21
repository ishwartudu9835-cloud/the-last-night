# THE LAST NIGHT — Phase 1 Vertical Slice (Unity 6 + URP, Android)

First-person survival horror. The village of Andhvan, 2006: 37 people vanished in a single night.

**APK without Unity on your PC:** `BUILD_IN_CLOUD.md` (GitHub builds it in the cloud and you download it from Releases)
**Opening it in Unity on a PC:** `SETUP.md` → `TESTING.md` (31-point test list)
**Design / story / roadmap:** `DESIGN.md`  ·  **Assets needed later:** `ASSETS.md`

What is in this slice (all real, working systems; no placeholder buttons):
- 1 small map (South Lane: 6 houses, temple, well, pump-house, gate), built by a one-click editor script
- FPS player: walk/sprint/crouch, stamina, health, fear/stress
- Mobile HUD: joystick, look pad, Interact/Sprint/Crouch/Torch/Bag/Pause + PC keys
- Flashlight with limited battery + spare batteries
- Monster AI: 8 states (Idle, Patrol, InvestigateNoise, Search, Chase, Attack, LoseTarget, ReturnToPatrol), sight + hearing, opens doors, checks hiding spots
- Hiding (almari + under the bed) that is not 100% safe
- Horror Director: 10 scare events with cooldowns and no-repeat rules
- Procedural 3D audio: footsteps, monster breathing/drag/scream, heartbeat, whispers, rain/wind, dynamic chase music
- Objective: 3 fuses → generator → gate → escape. Death → game over
- Menus: Main, Settings (graphics/audio/controls), Loading, Pause, Inventory, Note, Game Over, Ending
- Android: Low/Med/High presets, 30/45/60 FPS, resolution scale, object pooling, static batching

Not in this slice (later phases): multiplayer + voice chat (Phase 4, Photon), the other maps (Phase 3), 4 endings + cutscenes (Phase 5), real 3D art (Phase 2).
