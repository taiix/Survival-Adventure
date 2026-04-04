# Game Design Document — Survival Adventure

**Engine:** Unity (URP)  
**Genre:** 3D Action-Adventure / Survival RPG  
**Perspective:** Third-person  
**Platform:** PC (keyboard + mouse, Xbox gamepad supported)  
**Status:** In Development

---

## Table of Contents

1. [Overview](#1-overview)
2. [Core Pillars](#2-core-pillars)
3. [Game Loop](#3-game-loop)
4. [Player](#4-player)
5. [Combat](#5-combat)
6. [Enemies](#6-enemies)
7. [World & Level Design](#7-world--level-design)
8. [Progression & Economy](#8-progression--economy)
9. [NPC & Interaction System](#9-npc--interaction-system)
10. [User Interface](#10-user-interface)
11. [Controls](#11-controls)
12. [Audio](#12-audio)
13. [Technical Architecture](#13-technical-architecture)
14. [Known Gaps & Planned Features](#14-known-gaps--planned-features)

---

## 1. Overview

**Survival Adventure** is a third-person 3D action RPG in which the player explores a procedurally generated world, fights increasingly dangerous enemies, interacts with NPCs to upgrade their gear, and pushes deeper into the world to survive.

The game blends real-time melee combat with light RPG progression. The player invests gold earned in combat into weapon and armor upgrades, unlocking higher damage and survivability needed to tackle tougher enemy encounters and the Knight Boss.

---

## 2. Core Pillars

| Pillar | Description |
|--------|-------------|
| **Responsive Combat** | Sword attacks, dash-dodging, and stamina management must feel tight and rewarding. |
| **Meaningful Progression** | Every upgrade visibly changes how the player handles encounters. |
| **World Exploration** | A procedurally generated tile world rewards exploration with enemies, loot, and NPC hubs. |
| **Enemy Variety** | Melee, ranged, and boss enemies each demand different approaches from the player. |

---

## 3. Game Loop

```
Spawn in World
     │
     ▼
Explore Procedural Terrain
     │
     ▼
Encounter Enemies → Defeat → Earn Gold
     │
     ▼
Return to Hub City / Find NPC
     │
     ▼
Upgrade Weapon / Armor at Blacksmith
     │
     ▼
Fast Travel (Teleporter NPC) ─► Explore Deeper Zones
     │
     ▼
Defeat Knight Boss
```

**Session rhythm:**
- Short loops (1–3 min): fight a group of enemies, collect gold.
- Medium loops (5–10 min): return to hub, upgrade, prepare.
- Long loops (20+ min): push deeper, tackle the Knight Boss.

---

## 4. Player

### 4.1 Stats

| Stat | Default | Notes |
|------|---------|-------|
| Max Health | Configurable | Increased by ArmorItem upgrades |
| Health Regen | None (items/consumables only) | — |
| Min/Max Damage | Configurable | Scaled by WeaponItem upgrades |
| Attack Speed | Configurable | Scaled by WeaponItem upgrades |
| Defense | Configurable | Scales incoming damage reduction |
| Gold | 0 | Earned by defeating enemies |
| Walk Speed | 5 m/s | — |
| Sprint Speed | 8 m/s | Consumes stamina |
| Jump Height | 1.5 m | — |

### 4.2 Stamina

Stamina gates the sprint ability.

| Parameter | Value |
|-----------|-------|
| Max Stamina | 100 units |
| Drain Rate (sprinting) | 20 units/s |
| Regen Rate | 10 units/s |
| Regen Delay (after stop) | 1.5 s |

When stamina is empty, the player automatically returns to walking speed until stamina recovers.

### 4.3 Dash

The dash is the player's defensive tool. It provides a brief window of **invulnerability** and covers distance quickly.

| Parameter | Value |
|-----------|-------|
| Duration | 0.2 s |
| Speed | 12 m/s |
| Cooldown | 0.5 s |
| Invulnerable During Dash | Yes |
| Disabled In | Water zones |

Visual feedback: particle effects fire on dash activation. The player cannot dash in water.

### 4.4 Player States

| State | Allowed Actions |
|-------|----------------|
| Normal | Move, Sprint, Attack, Jump, Dash, Interact |
| Attacking | No movement |
| Dashing | Movement locked; invulnerable |
| Interacting | No movement, no combat |
| Stunned | No actions |
| Dead | None |

### 4.5 Equipment

The player holds one **Weapon** slot and one **Armor** slot. Equipping or upgrading either slot fires events that update all related stats in real time.

---

## 5. Combat

### 5.1 Melee Attack

- **Input:** Left Mouse Button / Gamepad X
- The player performs a **quick-attack combo** with up to 3 variants (animation-driven).
- Damage is applied via a **sphere overlap** centred on the sword collider at the moment of the swing's impact frame (driven by Animation Events).
- Damage value is randomised between `minDamage` and `maxDamage` from `PlayerStats`, then modified by the enemy's defense value.
- All objects in range that implement `IDamageable` receive damage.

### 5.2 Hit Feedback

- Enemies and the player both flash **red** on hit (0.15 s flash, 0.3 s fade).
- Optional particle effect on impact point.

### 5.3 Dash Invulnerability Window

The dash state sets `IsInvulnerable = true`. Any `TakeDamage()` call is ignored while the player is in the dash state, rewarding well-timed dodges.

---

## 6. Enemies

### 6.1 Base AI Behaviour

All enemies share a common state machine:

```
Idle ──(no nearby player)──► Patrol
  │                              │
  └────(player detected)────► Chase ──(in attack range)──► Attack
                                              │
                                   (player leaves range)
                                              │
                                           Chase
```

| Shared Stat | Default Value |
|-------------|--------------|
| Max Health | 50 |
| Walk Speed | 2 m/s |
| Chase Speed | 5 m/s |
| Detection Range | 15 m |
| Attack Range | 2 m |
| Attack Damage | 10 |
| Attack Cooldown | 1.5 s |
| Patrol Wait Time | 2–5 s (random) |

Pathfinding uses Unity's **NavMesh**. Patrol waypoints are chosen randomly within the NavMesh bounds.

---

### 6.2 Chasing Enemy (Melee)

- Standard melee enemy.
- Damage is dealt via a **head collider** on impact.
- No special abilities.
- Role: Swarm enemy — dangerous in groups.

### 6.3 Ranged Enemy

- Stays at distance; fires arcing **projectiles** at the player.
- Projectile follows a parabolic arc (configurable arc height and travel time).
- A **landing indicator** appears under the projectile to telegraph impact.
- Ideal counter: close the gap quickly to nullify ranged advantage.

**Projectile parameters (configurable):**
- Travel time
- Arc height
- Destroy on player contact

### 6.4 Knight Boss Enemy

The primary boss encounter. Has two attack modes:

| Attack | Trigger | Details |
|--------|---------|---------|
| Quick Attack | In attack range | Standard melee swing using sword collider |
| Special Attack | 25% chance, 6 s cooldown | Area-of-effect rock trail; travels from start point to end point, deals 20 damage, 12 m range |

**Special Attack flow:**
1. Boss charges (visual charge FX spawned).
2. A hitbox travels linearly from the boss toward the player's last position.
3. `OverlapBox` checks every 0.05 s along the path.
4. Each target can only be hit once per attack (HashSet tracking).
5. Stun on hit (system stubbed, pending implementation).

**Boss design intent:** The special attack forces the player to reposition continuously rather than tanking damage. Upgrading armor or timing a dash through the rock trail are the two main counters.

---

## 7. World & Level Design

### 7.1 Procedural Terrain Generation

The world is generated at runtime using **Perlin noise** on a tile grid.

| Parameter | Value |
|-----------|-------|
| Grid Size | 32 × 32 tiles |
| Noise Scale | 10 |
| Low Threshold (water) | 0.35 |
| High Threshold (cliffs) | 0.75 |
| Grass Spawn Chance | 35 % |
| Tree Spawn Chance | 15 % |
| Top Object Spawn Chance | 8 % |

Tiles fall into three height bands: **water**, **land**, and **elevated terrain**. An occupancy map prevents objects from overlapping on the same tile.

### 7.2 Scenes

| Scene | Purpose |
|-------|---------|
| **SampleScene** | Primary gameplay — procedural world, enemies, exploration |
| **MainCity** | Hub area — Blacksmith NPC, Teleporter NPC, safe zone |
| **EnemyTesting** | Developer scene for enemy AI iteration |

### 7.3 Water Zones

- Player **cannot dash** in water (dash is disabled by `PlayerWaterDetection`).
- Intended to create zones of vulnerability — enemy encounters near water are more dangerous.

### 7.4 Environment Props

- Grass tiles, large grass blocks, trees, fences, bridges, ramps, platforms.
- Lily pads in water zones.
- Props spawn at randomised positions and rotations within valid tiles.

---

## 8. Progression & Economy

### 8.1 Gold

- Earned by defeating enemies.
- Displayed in the HUD and shop UI.
- Spent at the Blacksmith to upgrade weapons or armor.
- Events fire on gold change, keeping UI always in sync.

### 8.2 Upgrades

All upgrades are multiplicative and permanent for the session.

| Parameter | Value |
|-----------|-------|
| Cost Multiplier per Level | 1.5× |
| Stat Multiplier per Upgrade | 1.2× |

**Weapon upgrade effects:** `minDamage` and `maxDamage` × 1.2 per level.  
**Armor upgrade effects:** `defense` and `maxHealthBonus` × 1.2 per level.

The Blacksmith shop UI shows **current stats** and **next-level stats** side-by-side so the player can make informed decisions.

### 8.3 Items

| Category | Fields | Notes |
|----------|--------|-------|
| WeaponItem | minDamage, maxDamage, attackSpeed | Upgradeable |
| ArmorItem | defense, maxHealthBonus | Upgradeable |
| ConsumablesItem | healthRestore | Stub — not yet integrated |

Items are Unity **ScriptableObjects**, decoupled from runtime systems.

---

## 9. NPC & Interaction System

### 9.1 Interaction

- The player can interact with any object implementing `IInteractable` within **3 m**.
- The closest valid target is selected automatically.
- Interaction is only possible in the `Normal` player state.
- Entering interaction state locks movement and combat.

**Input:** F (keyboard) / Y (gamepad)

### 9.2 Blacksmith NPC

- Opens a **grid-based shop UI** (navigable with DPad / left stick).
- Displays available weapons and armor as item slots.
- Selecting a slot shows an **UpgradeDescription** panel with current stats, next-level stats, and upgrade cost.
- Confirms upgrade on button press if the player has sufficient gold.

### 9.3 Teleporter NPC

- Opens a **fast-travel UI**.
- Allows the player to jump between registered locations.
- Useful for returning to the main gameplay scene from the city hub.

---

## 10. User Interface

### 10.1 HUD (In-Game)

| Element | System |
|---------|--------|
| Health Bar | Slider — updated via `OnPlayerHealthChanged` event |
| Stamina Bar | Slider — updated via `OnPlayerStaminaChanged` event |
| Gold Counter | Text — updated via `OnPlayerGoldChanged` event |

### 10.2 Shop UI (Blacksmith)

| Element | Description |
|---------|-------------|
| Item Grid | Scrollable grid of weapon/armor slots |
| Upgrade Description Panel | Shows item name, current stats, next-level stats, and cost |
| Player Stats Panel | Live display of gold, health, damage, attack speed, defense |
| Upgrade Button | Triggers `UpgradeManager.TryUpgradeItem()` |

### 10.3 Interaction Prompt

Displayed above the closest interactable object when the player is in range. Hidden when out of range or in a non-Normal state.

---

## 11. Controls

| Action | Keyboard | Gamepad |
|--------|----------|---------|
| Move | WASD / Arrow Keys | Left Stick |
| Sprint | Left Shift | Left Trigger |
| Attack | Left Mouse Button | X Button |
| Jump | Space | A Button |
| Dash | E | B Button |
| Interact | F | Y Button |
| Upgrade | U | Right Bumper |

The game supports **dynamic device switching** — picking up a controller or pressing a keyboard key instantly switches the active input profile. Input deadzone: 0.1.

---

## 12. Audio

> **Status: Not yet implemented.**

Planned audio features:
- Background ambient tracks per zone type (outdoor, city hub, boss arena).
- Combat sound effects: sword swings, hit impacts, enemy alerts.
- UI feedback: upgrade success, insufficient gold.
- Environmental: water ambiance, footstep differentiation by surface.

---

## 13. Technical Architecture

### 13.1 Patterns Used

| Pattern | Usage |
|---------|-------|
| **Service Locator** | `ServiceLocator` provides global access to `IPlayerStatsService`, `IUpgradeService`, `IInputService` without tight coupling. |
| **State Machine** | `PlayerStateManager` gates which actions are legal (movement, attack, dash, interact). |
| **Event System** | `EventManager` static `UnityEvent` fields broadcast stat changes to any listener. |
| **Component Composition** | Player is split into `PlayerAttack`, `PlayerHealth`, `PlayerStamina`, `PlayerMovementController`, etc., each independently testable. |
| **ScriptableObjects** | `ItemBase`, `WeaponItem`, `ArmorItem` are data assets decoupled from game logic. |
| **Interface-based Design** | `IDamageable`, `IInteractable`, `INPCBehaviour` allow substitutable implementations. |

### 13.2 Key Interfaces

| Interface | Contract |
|-----------|----------|
| `IDamageable` | `void TakeDamage(float damage)` |
| `IInteractable` | `DisplayName`, `CanInteract`, `OnInteract()` |
| `INPCBehaviour` | `OnInteract()`, `OnInteractionEnd()`, `GetUIPanel()`, `OnInteractionUpdate()` |
| `IPlayerStatsService` | Full stat get/set + gold management + equipment events |
| `IUpgradeService` | `TryUpgradeItem(ItemBase)` + upgrade events |
| `IInputService` | `GetInputAction(key)`, `IsUsingController` |

### 13.3 Rendering

- Unity **Universal Render Pipeline (URP)**.
- Post-processing via `DefaultVolumeProfile`.
- Per-NPC render cameras available for UI portrait views.

---

## 14. Known Gaps & Planned Features

| Feature | Status | Notes |
|---------|--------|-------|
| Health regen / consumables | Stub | `ConsumablesItem.healthRestore` exists but nothing consumes it |
| Full inventory UI | Partial | Item slots exist; full inventory screen not implemented |
| Enemy stun on Boss special | Partial | Code is commented out in `KnightBossSpecialAttackHitbox.cs` |
| Save / Load system | Missing | No persistence; all progress lost on exit |
| Audio system | Missing | No SFX or music; architecture hooks not yet placed |
| Player health HUD | Broken | `PlayerStatsUI.UpdateHealthUI()` throws `NotImplementedException` |
| Advanced particle FX | Minimal | Placeholder dash particles; no hit sparks or death effects |
| Consumable drop system | Missing | Enemies do not drop items or consumables |
| Multiple boss variants | Planned | Only Knight Boss exists |
| Multiplayer / co-op | Not planned | Single-player only |
