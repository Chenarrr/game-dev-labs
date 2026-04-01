# Lab2 — 2D Platformer (Unity 6)

A simple 2D platformer built with Unity 6 and the Universal Render Pipeline (URP 2D).

---

## How to Play

| Key | Action |
|-----|--------|
| `A` | Move left |
| `D` | Move right |
| `Enter` | Jump (only when grounded) |
| `R` | Restart after game over |

---

## Gameplay

- Survive as long as possible by avoiding dark grey obstacle squares coming from the right
- Your score increases by 1 every second you stay alive
- One hit from an obstacle ends the game
- The final score is shown on the Game Over screen

---

## What's in the Project

### Scripts (`Assets/Scripts/`)

| Script | Description |
|--------|-------------|
| `PlayerController.cs` | Handles player movement (A/D), jumping (Enter), and ground detection. Notifies GameManager on death |
| `Obstacle.cs` | Moves a square left at constant speed. Destroys itself when off-screen |
| `ObstacleSpawner.cs` | Spawns a new dark grey obstacle every 2 seconds from the right side of the screen |
| `GameManager.cs` | Singleton that tracks score, handles game over state, destroys active obstacles on death, and listens for R to restart |

### Editor Scripts (`Assets/Editor/`)

| Script | Description |
|--------|-------------|
| `SceneSetup.cs` | Unity editor menu item (`Lab2 → Fix Scene`) that builds the entire scene from scratch — ground, player, spawner, UI, camera |

### Scene (`Assets/Scenes/SampleScene.unity`)

| Object | Details |
|--------|---------|
| **Main Camera** | Sky blue background, orthographic size 5 |
| **Ground** | Green square sprite, width 22, `BoxCollider2D`, tagged `Ground` |
| **Player** | Red square sprite, `Rigidbody2D` (freeze Z rotation), `BoxCollider2D`, `PlayerController` |
| **ObstacleSpawner** | Empty GameObject that spawns obstacles at runtime |
| **GameManager** | Empty GameObject running game state logic |
| **GameUI** | Canvas with score text (top left) and Game Over overlay panel |

---

## Setup (first time)

1. Open the project in Unity 6
2. Wait for scripts to compile
3. Click **`Lab2 → Fix Scene`** in the top menu bar
4. Hit **Play**

---

## Tech

- Unity 6 (6000.3.9f1)
- Universal Render Pipeline (URP 2D)
- New Input System (`UnityEngine.InputSystem`)
- TextMeshPro for UI text
