# Lab2 — 2D Platformer (Unity 6)

A Mario-style 2D platformer built with Unity 6 and URP 2D, featuring a custom cute red ball character made in Blender.

---

## How to Play

| Key | Action |
|-----|--------|
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `Space` / `↑` | Jump |
| `Space` / `Enter` | Start game from intro screen |
| `R` / `Enter` | Restart after game over |

> Press **SPACE** on the intro screen first before moving.

---

## Gameplay

- Run as far as you can on procedurally generated platforms
- Avoid obstacles and enemies
- Stomp enemies for **+3 points**
- Score increases the further you run
- Fall into a gap or get hit = game over

---

## Character

Custom cute red ball made in **Blender** via Blender MCP:
- 15 hand-rendered animation frames: idle, blink, run (x6), jump anticipation, up, peak, fall, land
- Transparent PNG sprites with squash & stretch feel
- Animated in Unity via Animator Controller

---

## What's in the Project

### Scripts (`Assets/Scripts/`)

| Script | Description |
|--------|-------------|
| `PlayerController.cs` | Movement, jumping, coyote time, squash & stretch, animator drive |
| `CameraFollow.cs` | Smooth follow — scrolls right only during gameplay (Mario-style) |
| `CameraShake.cs` | Screen shake on death |
| `GameManager.cs` | Game states: Intro → Playing → GameOver, score tracking |
| `WorldGenerator.cs` | Procedural platform and enemy generation |
| `ParallaxBackground.cs` | Scrolling background layers |
| `CloudSpawner.cs` | Decorative cloud spawner |
| `Enemy.cs` | Enemy behaviour |
| `Obstacle.cs` | Moving obstacle logic |

### Sprites (`Assets/Sprites/`)

| File | Description |
|------|-------------|
| `ball_idle.png` | Idle frame |
| `ball_blink1-3.png` | Blink animation |
| `ball_run1-6.png` | Run cycle |
| `ball_jump_anticipation.png` | Crouch before jump |
| `ball_jump_up.png` | Rising |
| `ball_jump_peak.png` | Peak |
| `ball_jump_fall.png` | Falling |
| `ball_jump_land.png` | Landing squash |
| `Animations/BallAnimator.controller` | Animator wiring all states |

---

## Setup

1. Open in **Unity 6**
2. Wait for compile
3. Press **Play**
4. Press **SPACE** to start

---

## Tech

- Unity 6 (6000.3.9f1)
- Universal Render Pipeline (URP 2D)
- New Input System (`UnityEngine.InputSystem`)
- TextMeshPro
- Blender 5.1 (character art via Blender MCP)
