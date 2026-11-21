# Asset Requirements (Phase 2+)

Phase 1 uses **no external assets**: shapes are primitives, textures are generated in code, and every sound is synthesised. The table below lists the real art needed for Phase 2. **Always check the licence before Play Store release.**

| Need | Free option | Paid option (stylized) | Replaces |
|---|---|---|---|
| Monster model + animations | **Mixamo** (free, Adobe login): a creepy character + Walk/Run/Attack/Idle anims | Asset Store horror creature packs | Builder's primitive monster → swap `MonsterAnimator` for an Animator |
| Player hands + flashlight | Mixamo arms / Quaternius | Asset Store FPS arms | The torch cylinder |
| Village houses, props | **Quaternius**, **Kenney** (CC0) | **Synty POLYGON** (Horror / Town packs) | Builder houses |
| Indian-village details (charpai, matka, temple, well) | Blender + simple modelling, or commission | Fab.com / Sketchfab (check licence) | Greybox props |
| Textures | **Poly Haven**, **ambientCG** (CC0) | — | Generated noise textures |
| Sound effects | **Freesound.org** (prefer CC0; for CC-BY add credits), **Sonniss GDC bundle** (free, royalty-free) | Asset Store horror SFX packs | `ProceduralAudio` clips (same names → just assign) |
| Music | Self-made / commissioned, or royalty-free horror ambience | — | Drone/Chase loops |
| Font (Devanagari + Latin) | **Google Fonts**: Noto Sans Devanagari, Special Elite | — | LegacyRuntime font |
| Ghost | A simple pale character (Mixamo) + transparent shader | — | Capsule ghost |

**Budget-friendly route:** Synty POLYGON Horror (~$50–100 when on sale) + Mixamo + Sonniss/Freesound = a stylized, consistent look at low cost.
