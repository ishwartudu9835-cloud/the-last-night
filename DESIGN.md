# THE LAST NIGHT — Game Design Document (v0.1, Phase 1)

> Working title: **THE LAST NIGHT** · Genre: first-person survival / psychological horror · Android · Unity 6 + URP · 1–4 players (multiplayer arrives in Phase 4)
> Visual direction: **stylized 3D**: simple shapes, a strong colour palette, heavy fog, cheap but scary lighting.

---

## 1. Story bible

> ⚠️ This section contains spoilers and is for the developer only. In the game, the story comes out piece by piece through notes, tapes and places.

### Setting: the village of Andhvan (fictional)
A small village near the forests of Jharkhand. In **2006**, during the night of Kartik Amavasya, **37 people** disappeared in a single night. No bodies were found and no signs of a struggle. The doors of the houses were locked from the inside. The police file was closed as "mass migration". Since then the village has been abandoned and the road to it is shut.

### The four friends (the players)
| Character | Role | Hidden thread |
|---|---|---|
| **Aryan** | Documentary host (YouTube channel "Lost Signal") | Organised the trip |
| **Zoya** | Camera / sound | Records radio static (story-clue mechanic) |
| **Kabir** | Driver, the "local" guide | **Twist:** Kabir is Dr. Sahay's grandson. He knew. |
| **Neha** | Medical student | Can read the hospital files (secret-ending condition) |

### Chapters
1. **Chapter 1: The Missing Village** *(Phase 1 slice = the first part of this chapter)*
   The car dies at midnight, the network is gone, and a scream comes from the forest. The village gate is locked and runs on an electric motor. Power comes from the old pump-house generator, and its fuses are scattered.
2. **Chapter 2: The Hospital.** Dr. Vikram Sahay's "free health camp" (2005–06). The patient files show every villager getting the same "sleep treatment".
3. **Chapter 3: The School.** Children's drawings: a tall man with no face. The teacher's diary: "the children say he only takes the ones who are afraid."
4. **Chapter 4: The Haveli (Mansion).** Dr. Sahay's house. His daughter **Meera** (age 9) had night terrors. The doctor found an ancient relic, the "Nidra Yantra", beneath the temple.
5. **Chapter 5: The Forest & Underground Lab.** Under the temple is a lab where the experiment took place: using the yantra, the doctor drew the fear out of the whole village to cure Meera.
6. **Chapter 6: The Truth.** Final revelation (below).

### Twists (revealed in order)
1. **The creature did not come from the experiment.** It is **made of** the fear of the 37 villagers. The whispers you hear are their voices.
2. **The creature cannot see the light; it sees fear.** (Gameplay hint: the more scared the player is, the stronger its "sight". Already in the Phase 1 code: the monster's detection grows with player fear.)
3. **Kabir knew.** The trip was his plan to bring the yantra back.
4. **Final revelation:** Meera is the creature's **core**. Her fear was what tied all 37 together, and she still believes this is all a bad dream.

### Endings (Phase 5; the conditions are decided now)
| Ending | Condition |
|---|---|
| **Good**: all escape | Every living player crosses the gate; no one stays "taken" |
| **Bad**: all captured | All players are captured (in Phase 1: death in solo play) |
| **Secret**: the full truth | All 12 clues + Neha reads the files + the yantra is turned off in the lab → the 37 souls are freed |
| **Sacrifice** | One player stays in the lab and holds the yantra (a hold interaction) while the others escape |

---

## 2. Phase 1: Vertical Slice (this delivery)

**Map:** "Andhvan: South Lane". The dead car, a village lane, 6 houses (3 can be entered), the temple chabutra, a well, the pump-house generator, the locked gate. Forest all around (a boundary you cannot cross).

**Objective loop:**
1. Find **3 fuses** (inside houses).
2. Fit them into the **generator** with a 4-second hold interaction. **It makes a loud noise and the monster comes.**
3. The generator starts → the street lights come on → the **gate motor** runs and opens in 6 s.
4. Reach the gate → **Escaped** (ending screen).
5. Death → **Game Over** (the slice's Bad ending).

**Survival rules (single-player balance):**
- HP 100, monster hit 40, regeneration after 8 s (slow). That is 3 hits.
- Flashlight battery ~100 s per cell. Spare batteries are in the inventory; refill = Inventory → Use.
- Stamina: sprint ~5 s, then you are tired.
- Crouching: slow, but quiet and harder to see.
- Hiding: cupboards and beds. **Not 100% safe.** If the monster saw you go in, it will check that spot.

### Monster: "Nishachar" FSM
```
Idle ──timer──▶ Patrol ──noise──▶ InvestigateNoise ──arrived──▶ Search
  ▲               │  ▲                    │ sees player                 │ sees player
  │               │  └──── ReturnToPatrol ◀── LoseTarget ◀── (lost LOS) │
  │               └──sees player──▶ Chase ──in range──▶ Attack ──▶ Chase / Idle(stagger)
  └──────────────────────────────── (random idle pauses during patrol) ──┘
```
| State | Speed | What happens |
|---|---|---|
| Idle | 0 | 2–5 s, head turns, low breathing |
| Patrol | 1.6 | Random patrol points (never the same point twice in a row), metal-drag sound |
| InvestigateNoise | 2.8 | Walks to the noise position (footsteps, door, generator, falling objects) |
| Search | 2.2 | 3–5 random points within an 8 m radius of the last known spot; may check a hiding spot |
| Chase | 4.6 | Scream, then runs; the player's sprint is 5.2, so a fair escape is possible |
| Attack | 0 | 0.45 s wind-up (warning), hit at 2 m, then 1.5 s stagger (gives you a chance to escape) |
| LoseTarget | 0 | 1.5 s sniffing (pause) |
| ReturnToPatrol | 2.0 | Goes to the nearest patrol point |

**Senses:** 110° vision cone, 14 m base range. Flashlight ON ×1.7. Crouch ×0.6. Fear adds up to +30%. Raycast line-of-sight. **Hearing:** `NoiseSystem.Emit(pos, radius)`; walk 3 m, sprint 11 m, door 9 m, generator 30 m.
**Fairness:** 20 s grace at the start. The chase always begins with a scream. Breathing and drag sounds are 3D, so you can tell the direction.

### Horror Director
Every 20–45 s it may trigger one event, based on tension (monster distance/state + fear). **A no-repeat history** (the last 3 events cannot come back), each event has its own cooldown, and the up-close jumpscare comes at most once every 3 minutes.
Events: light flicker · power failure · distant scream · whisper (near the ear) · distant crying · door slam · object fall · radio static · ghost glimpse · jumpscare.

---

## 3. Roadmap
| Phase | Content | External setup needed |
|---|---|---|
| **1 ✅ (this)** | Vertical slice (single player) | None |
| 2 | Horror polish: real models/animations (Mixamo), real sound samples, hiding variations, crouch-peek | Asset downloads (free/paid, see ASSETS.md) |
| 3 | Full map: hospital, school, haveli, forest, lab + puzzles + notes/tapes | 3D assets |
| 4 | **Multiplayer**: Photon Fusion 2 (Host mode), room code, private room, revive, spectator. **Photon Voice 2** proximity chat | **Photon account + App IDs (Fusion + Voice)**; free tier 20 CCU |
| 5 | 4 endings, cutscenes (Timeline), story audio logs | — |
| 6 | Device testing, AAB, Play Store listing | **Google Play Developer account ($25)**, signing keystore |

The Phase 1 code is already structured with multiplayer in mind: `NoiseSystem`, the `GameManager` events and the monster's "target selection" are written for multiple players. In Phase 4, the `NetworkBehaviour` layer will sit on top of these.
