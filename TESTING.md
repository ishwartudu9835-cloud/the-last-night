# Phase 1 Test Checklist

The spec says: "Test every major feature before moving to the next phase." Tick each line in the Editor and then on the phone. If anything fails, send me the line number + Console error.

| # | Test | How | Expected | Editor | Phone |
|---|---|---|---|---|---|
| 1 | Compile | Copy the folder in | Console has 0 red errors | ☐ | — |
| 2 | Scene builder | Menu → 1. Build | Dialog says "Scene ban gaya", NavMesh is blue (Window → AI → Navigation) | ☐ | — |
| 3 | Main menu | Play | THE LAST NIGHT title, the village behind it | ☐ | ☐ |
| 4 | Loading | KHELO (Play) | Progress bar fills (sounds generating), tip shown | ☐ | ☐ |
| 5 | Movement | WASD / joystick | Walk, footstep sound, head-bob | ☐ | ☐ |
| 6 | Look | Mouse / right-side drag | Smooth; Settings → Look speed changes it | ☐ | ☐ |
| 7 | Sprint + stamina | Shift / DAUDO | STA bar drains, grey + breathing when tired | ☐ | ☐ |
| 8 | Crouch | C / JHUKO | Camera drops, slower, button highlights | ☐ | ☐ |
| 9 | Flashlight | F / TORCH | On/off click, BAT bar drains, flicker below 18% | ☐ | ☐ |
| 10 | Battery | Pick up a battery, R / Bag → Battery lagao | Torch at 100% | ☐ | ☐ |
| 11 | Door | E / HAATH near a door | Opens with a creak; locked doors show a message | ☐ | ☐ |
| 12 | Note | Paper on the car | Note panel opens | ☐ | ☐ |
| 13 | Fuse ×3 | In the 3 houses you can enter (L1, R2, L3) | Objective counter goes 1/3 → 3/3 | ☐ | ☐ |
| 14 | Generator | Pump-house, hold | 3 fuses inserted, then pull-start (may fail once), LOUD | ☐ | ☐ |
| 15 | Monster patrol | Wait 20 s+ | Metal-drag sound, it walks from point to point | ☐ | ☐ |
| 16 | Hearing | Sprint near it / bang a door | It comes to investigate (growl) | ☐ | ☐ |
| 17 | Sight + chase | Walk in front of it with the torch | Scream → chase music → it runs | ☐ | ☐ |
| 18 | Escape chase | Sprint, break line-of-sight | Sniff → it searches → goes back to patrol | ☐ | ☐ |
| 19 | Attack | Let it catch you | Wind-up growl, hit, red flash + shake, HP −40 | ☐ | ☐ |
| 20 | Hiding (safe) | Hide in the almari where it can't see you | It usually passes by (sometimes it checks!) | ☐ | ☐ |
| 21 | Hiding (seen) | Get in while it is watching | It opens the cupboard and pulls you out | ☐ | ☐ |
| 22 | Doors vs monster | Chase it through a closed door | Door slams open | ☐ | ☐ |
| 23 | Horror events | Play for 5+ minutes | Flicker / whisper / scream / ghost etc. The same event does not repeat back-to-back | ☐ | ☐ |
| 24 | Fear FX | Stay close to the monster | Heartbeat, vignette, distortion | ☐ | ☐ |
| 25 | Gate + escape | After the generator | Gate opens (~7 s), walk through → "TUM BACH NIKLE" | ☐ | ☐ |
| 26 | Death | 3 hits | "WO TUMHE LE GAYA" → Retry works | ☐ | ☐ |
| 27 | Pause | Esc / II | Game stops; Resume / Restart / Menu work | ☐ | ☐ |
| 28 | Settings | Quality Low/Med/High, FPS 30/45/60 | Low: no post-FX/shadows, less rain. Settings persist after a restart | ☐ | ☐ |
| 29 | HUD customise | HUD size / opacity | Buttons get bigger/smaller, transparency changes | ☐ | ☐ |
| 30 | Performance | Stats window (Editor), phone feels smooth | Medium preset holds a steady 30 FPS on a mid-range phone | ☐ | ☐ |
| 31 | Battery/heat | Play 15 min on the phone | Phone not too hot (if it is: Low + 30 FPS) | — | ☐ |
