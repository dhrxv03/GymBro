# GymBro — Assessments 3 and 4

A 2D gym-themed maze game made in Unity. PacStudent is a gym-goer collecting protein while avoiding Burger, Doughnut, Pizza and Fries.

## Open and play

Open the project in Unity **6000.4.11f1** (the editor version recorded in this project), open `Assets/Scenes/StartScene.unity`, and press Play. Click the Game view so it receives keyboard input. The build scene list includes StartScene, Level01 and InnovationScene, in that order.

| Control | Action |
|---|---|
| W / A / S / D | Queue the next movement direction. Movement continues after releasing the key. |
| Exit button | Return to the main menu during active play. |
| Space — Level 2 only | Spend 4 energy for a 0.6-second dash while moving. |
| E — Level 2 only | Spend 8 energy to complete an unfinished nearby workout station. |

## Level 1: classic maze

Collect every pellet to finish the round. Standard protein pickups award 10 points; protein shakes award 50 points and scare the enemies for 10 seconds. During the final 3 seconds, the enemies enter their recovering state. A scared/recovering enemy awards 300 points when eaten, returns directly to its own spawn position, and revives in the current global state. The moving golden dumbbell awards 100 points.

Touching a normal enemy costs one of three lives. The player and ghosts reset after the death animation; collected pellets and score remain. Both collecting every pellet and losing all lives end the round, show a blocking message for three seconds, and return to the menu. Side tunnels teleport the player, but ghosts turn back at their ends.

The four numbered enemies behave differently:

1. Burger prefers valid moves that increase or preserve distance from the player.
2. Doughnut prefers valid moves that decrease or preserve distance from the player.
3. Pizza randomly chooses a valid non-reversing direction.
4. Fries routes to the outer boundary and then follows the wall clockwise.

All enemies flee when scared/recovering. They avoid reversing unless trapped. Where no move satisfies a chase/flee distance preference, they use another valid non-reversing move so they do not stop. Ghosts 1 and 3 leave through the top gate; 2 and 4 leave through the bottom gate. Normal speed is 90% of player speed; scared, recovering and dead speed is 50% of normal ghost speed.

## Level 2: Workout Circuit — design innovation

**Goal: complete all four workout stations before losing all three lives.** Clearing the pellets alone does not win this mode. The four labelled dumbbells are fixed workout stations; the unlabelled moving dumbbell remains the bonus item.

You start with 8 energy and can hold up to 12. Each protein pickup adds 1 energy, and active play restores 1 energy every three seconds. The slow recharge means depleted pickups cannot make the goal impossible.

Space spends 4 energy on a dash: movement doubles for 0.6 seconds and the character turns cyan. During that time, touching normal enemies is safe. The dash still respects walls and does not change enemy speeds. It can only start while moving and cannot be stacked while active.

Press E within 0.8 world units of an unfinished station with at least 8 energy. It consumes 8 energy, awards 500 points, changes the label to DONE and turns the station green. Each station can be completed once. Complete all four to see CIRCUIT COMPLETE and return to the menu. Completed workouts survive a lost life. Death clears the dash and restores energy to at least 4.

This changes the core objective and adds a resource decision: spend energy to escape immediate danger, or save it for the next station. Players choose routes between stations and protein pickups rather than clearing every corridor. Slow recharge provides a fallback, while the recorded completion time rewards efficient routes. Countdown, death and game over suspend the abilities and recharge.

## Scores and implementation

Each level saves its own best score and the associated completion time using PlayerPrefs. A higher score replaces the record; equal scores replace it only with a faster time. The keys are `GymBro.Level01.BestScore` / `BestTime` and `GymBro.InnovationScene.BestScore` / `BestTime`.

- `LevelGenerator`: builds the mirrored assessment map and exposes grid queries.
- `PacStudentController`: queued keyboard input, grid interpolation, animation and movement effects.
- `GameController`: countdown, collection, swept contact checks, scores, lives and round flow.
- `GhostController` / `GhostStateController`: grid decisions, directional animations and return-to-spawn behaviour.
- `CherryController`: moving bonus spawning and travel.
- `WorkoutController`: Level 2 energy, dash and station objective.
- `HUDController`, `StartScreenController`, `HighScoreStore`, `GameAudioController`: UI, records and audio.

Level 1 has no WorkoutController reference; its original objective and movement speed remain unchanged. The movement code does not use Rigidbody forces or Unity navigation tools. Level 2 reuses the existing sprites and audio, including the bonus sound for successful dashes and workouts.

## Validation and final playthrough

Editor Play Mode checks covered the countdown, score values, lives/respawn, tunnel travel, power-up transitions, music priority, both Level 1 end conditions, high-score rules and menu return. Ghost checks simulated 300 seconds per enemy, checked wall/pen/tunnel restrictions, outer patrol coverage, chase/flee choices, revival states and matching movement at 30/144 FPS. Level 2 checks covered ability gating, energy cap/costs, dash protection, station proximity and single completion, scoring, the circuit win, menu return and Level 2 navigation. Temporary setup/test helpers were removed from the game project.

Before submission, manually play both levels from StartScene, including a lost life, a power-up, a dash, and a completed circuit. Automated checks do not replace a full human playthrough or guarantee assessment marks.

## AI assistance disclosure

ChatGPT/Codex was used substantially during development, including generating and revising C# scripts, Unity scene setup, animator wiring, sprite creation/repair, troubleshooting and test helpers. The resulting game should not be described as entirely unaided work.

Examples of prompts used in the development conversation include:

- “create all the sprites for the game in 32x32 format” — gym character, food enemies and themed items; later requests corrected animation frames and wall seams.
- “gimme the script for `GameAudioController` aswell” — audio controller development.
- “fix the sprites and make the wall” — tile corrections and maze assembly.
- “I can use AI for scripts aswell now. Use basic level scripts. Start wokring on it now” — permission and requested script style for subsequent implementation.
- “create the required animators and animations” — animation clips and controllers.
- “continue onto next” — ghost movement implementation following the completed gameplay stage.
- “continue with it.” — Workout Circuit implementation after reviewing remaining assessment work.

Follow-up prompts such as “next”, “continue”, and requests to fix or check screenshots were interpreted using the assessment PDFs and preceding conversation. Outputs used include the gameplay scripts listed above, scene and animator changes, and the generated/revised sprite assets. The conversation contains the full iterative prompts and responses; this README is a summary of assistance, not a complete transcript. Review the final declaration against the actual work and the submission template.

## Audio credits


| Local file | Track / effect | Source |
|---|---|---|
| `Music_Normal.mp3` | Galactic Rap — Kevin MacLeod | [Incompetech](https://incompetech.com/music/royalty-free/mp3-royaltyfree/Galactic%20Rap.mp3) |
| `Music_Intro.mp3` | Magical Reveal Start | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-magical-reveal-start-388923/) |
| `Music_Menu.mp3` | Equatorial Complex — Kevin MacLeod | [Incompetech](https://incompetech.com/music/royalty-free/mp3-royaltyfree/Equatorial%20Complex.mp3) |
| `Music_Scared.mp3` | Desert chase | [Pixabay](https://pixabay.com/music/beats-desert-chase-304512/) |
| `Music_Dead.mp3` | Dry Bones | [Myinstants](https://www.myinstants.com/en/instant/dry-bones-death-74443/) |
| `SFX_Move.mp3` | Bundeswehr Bag, Heavier, Light Movement, PROCESSED, 24bit96kHz_014 | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-bundeswehr-bag-heavier-light-movement-processed-24bit96khz-014-104102/) |
| `SFX_EatPellet.wav` | Chewing something crunchy | [Mixkit](https://mixkit.co/free-sound-effects/eat/) |
| `SFX_EatEnemy.wav` | Juicy insect fast squish | [Mixkit](https://mixkit.co/free-sound-effects/discover/squish/) |
| `SFX_Bonus.mp3` | Video Game – Bonus | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-video-game-bonus-323603/) |
| `SFX_WallHit.mp3` | Stick Hitting a Dreadlock (Small thud) | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-stick-hitting-a-dreadlock-small-thud-83297/) |
| `SFX_Death.mp3` | Male Scream | [Pixabay](https://pixabay.com/sound-effects/film-special-effects-male-scream-121085/) |


“Galactic Rap” and “Equatorial Complex” by Kevin MacLeod (incompetech.com), licensed under Creative Commons Attribution 4.0: https://creativecommons.org/licenses/by/4.0/

The table records the selected sources. Keep any downloaded licence records with your submission records; a source link alone does not establish reuse permission.
