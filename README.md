# Lab2 — 2D Platformer (Unity 6)

A Mario-style 2D infinite runner / platformer built with Unity 6 and URP 2D, featuring a custom cute red ball character made in Blender.

---

## How to Play

| Key | Action |
|-----|--------|
| `A` / `←` | Move left |
| `D` / `→` | Move right |
| `Space` / `↑` | Jump (hold for higher, tap for short hop) |
| `Space` / `Enter` | Start game from intro screen |
| `R` / `Enter` | Restart after game over |

---

## Gameplay

- Run across procedurally generated platforms
- Jump over gaps and between platforms
- Stomp enemies from above to bounce off them
- Fall into a gap or touch an enemy from the side = game over
- Difficulty ramps up — gaps get wider, enemies get faster
- Your best distance is saved between sessions

---

## Character

Custom cute red ball made in **Blender** via Blender MCP:
- 15 hand-rendered animation frames: idle, blink (x3), run (x6), jump anticipation, up, peak, fall, land
- Transparent PNG sprites
- Squash & stretch physics in Unity
- Dust particles when running and landing

---

## Features

- **Smooth camera** — follows player on both axes with look-ahead and damping
- **Procedural world** — platforms, gaps, and enemies generated infinitely
- **Parallax background** — mountains and hills scroll at different speeds
- **Coyote time** — forgiving jump window after leaving a ledge
- **Jump buffering** — jump input registers slightly before landing
- **Variable jump height** — hold for high jump, tap for short hop
- **Squash & stretch** — ball deforms on jump and landing
- **Dust particles** — visual feedback when running and landing
- **Screen shake** — on death for impact
- **Difficulty scaling** — gaps widen, enemies speed up as you progress

---

## Scripts (`Assets/Scripts/`)

| Script | Description |
|--------|-------------|
| `PlayerController.cs` | Movement, jump (coyote time + buffer), squash & stretch, dust particles |
| `CameraFollow.cs` | Smooth SmoothDamp follow with velocity-based look-ahead |
| `CameraShake.cs` | Screen shake as offset applied through CameraFollow |
| `GameManager.cs` | Game states (Intro → Playing → GameOver), best distance tracking |
| `WorldGenerator.cs` | Procedural platform, gap, and enemy generation with difficulty ramp |
| `ParallaxBackground.cs` | Two-layer scrolling background (mountains + hills) |
| `CloudSpawner.cs` | Decorative cloud system |
| `Enemy.cs` | Patrol + stomp/damage behaviour |

---

## Sprites (`Assets/Sprites/`)

| File | Description |
|------|-------------|
| `ball_idle.png` | Idle frame |
| `ball_blink1-3.png` | Blink animation |
| `ball_run1-6.png` | Run cycle |
| `ball_jump_anticipation.png` | Crouch before jump |
| `ball_jump_up.png` | Rising |
| `ball_jump_peak.png` | Peak of jump |
| `ball_jump_fall.png` | Falling |
| `ball_jump_land.png` | Landing squash |

---

## Setup

1. Open in **Unity 6** (6000.3.9f1 or later)
2. Wait for compile
3. Press **Play**
4. Press **SPACE** to start

---

## Tech Stack

- Unity 6 with Universal Render Pipeline (URP 2D)
- New Input System (`UnityEngine.InputSystem`)
- TextMeshPro for UI text
- Blender 5.1 (character art via Blender MCP)
