# Game Documentation 

**Game Title:** Broken Warrior: Time Traveller Run  
**Student Name:** [Your Name Here]  
**IT Number:** [Your IT Number Here]  

---

## 1. Summary of the Game Concept
*Broken Warrior: Time Traveller Run* is an endless-runner 2D action game featuring intense sword combat. The player controls a legendary warrior charging forward through time, fending off endless waves of corrupted samurai enemies. The game demands sharp reflexes as players chain together 3-hit weapon combos, collect procedurally animated health packs to survive, and strive to reach a score of 100 to achieve ultimate victory. 

---

## 2. Description of How Core Features are Implemented

**1. Modular Character Controller & Physics:**
The player is controlled via a custom `PlayerMovement.cs` script leveraging Unity's `Rigidbody2D` for physics-based movement and jumping. Ground detection is handled via `Physics2D.OverlapCircle` against a dedicated "Ground" LayerMask to prevent infinite jumping.

**2. Endless Enemy Spawning System:**
The game features an endless challenge mode powered by the `EnemySpawner.cs` and `HealthPackSpawner.cs` scripts. These managers track the player's X-coordinate and dynamically instantiate enemies and power-ups ahead of the player at safe ground-level coordinates (`Y = -2` and `Y = -4`), ensuring a continuous, unbroken gameplay loop.

**3. State-Machine Combat & Hit Detection:**
The combat engine uses an Enum-based State Machine in `PlayerCombat.cs` allowing players to trigger 6 distinct attack varieties (Slash, Chop, Stab, Upper, Low, Power). Damage is calculated via `Physics2D.OverlapCircle` casting from a dedicated `attackPoint`, filtering collisions strictly through the "Enemy" LayerMask.

**4. Automated Game Flow & Scene Management:**
A persistent Singleton `GameManager.cs` gracefully tracks the player's score and health across scenes. When health depletes to 0, or score reaches 100, a robust SceneManager fallback ensures safe transitions to a procedurally generated "Game Over / You Win" scene architecture.

---

## 3. Screenshots of Gameplay
*(Please insert your screenshots here by dragging and dropping them into your Word document before saving as PDF)*

*   **[Insert Screenshot 1: Main Menu]**
*   **[Insert Screenshot 2: Player executing a sword combo against an enemy]**
*   **[Insert Screenshot 3: Collecting a Health Pack]**
*   **[Insert Screenshot 4: Game Over / Victory Screen]**

---

## 4. Control Guide

*   **Move Right / Run:** [D] or [Right Arrow]
*   **Move Left:** [A] or [Left Arrow]
*   **Jump:** [Spacebar]
*   **Light Attack:** [Left Mouse Click]
*   **Heavy / Power Attack:** [Right Mouse Click]
*   **Block / Defend:** [Shift]
*   **UI Navigation:** Mouse Cursor

---

## 5. Description of the Creative Feature

**Feature: "Procedural Shadow-Fight Style Weapon Animations"**
Instead of relying on heavily pre-baked, rigid 2D sprite sheets for every single attack frame, this game utilizes an advanced **Procedural Animation Engine** via C# Coroutines (`CodeAttackAnimation`). 

When an attack is triggered:
1.  The code calculates custom pivot points for the character's limbs and sword.
2.  It uses mathematical interpolation (`Quaternion.Slerp` and `Mathf.SmoothStep`) to fluidly trace the weapon through three distinct phases: **Windup ➔ Strike ➔ Recover**. 
3.  This system scales endlessly, allowing for a deep 3-hit combo combat system (Slash, Chop, Upper) that feels incredibly snappy and responsive, drastically reducing the memory footprint of animated sprites while delivering a premium "Shadow Fight" aesthetic. Furthermore, visual feedback is generated procedurally via temporary multi-colored `SpriteRenderer` slash trails instantiated exactly at the strike arc.

---

## 6. Credits for External Assets/Tools Used

*   **Game Engine:** Unity Editor 2022.3.62f3
*   **Physics Engine:** Built-in Unity Box2D
*   **Sprites & Art:** Unity Standard Assets / [List any itch.io asset packs you used here]
*   **Fonts:** Arial (Built-in)
*   **Audio:** [List any audio tool or asset used, or put "Procedural Audio / Custom"]

---

## 7. Video Link

**Gameplay Demonstration Video:** 
[Paste your YouTube / Google Drive Video URL Here]
