# Urban Obstacle System for "Just Play Mariam"

## Overview
A comprehensive urban obstacle system that adds authentic city elements to Mariam's soccer adventure through the streets, collecting money for her grandmother.

## New Urban Obstacle Types

### 🏠 Static Urban Obstacles
1. **Trash Bins** 🗑️
   - Plastic (weak) and Metal (strong) variants
   - Can drop coins when destroyed
   - Different colors and scales

2. **Street Signs** 🚦
   - Wobble when hit
   - Fall over when destroyed
   - Yellow warning color

3. **Vendor Carts** 🛒
   - Drop multiple coins/fruits when destroyed
   - Chain reaction possible
   - Realistic vendor cart appearance

4. **Fire Hydrants** 🚰
   - Create water spray effect when destroyed
   - Red color, sturdy build
   - Require charged shots

5. **Parked Cars** 🚗
   - Mostly indestructible (windows can break)
   - Only spawn in side lanes
   - Realistic car proportions

6. **Flower Pots** 🪴
   - Small, easy to destroy
   - Scatter terracotta pieces
   - Decorative urban elements

### 🚴 Dynamic Urban Obstacles
1. **Delivery Scooters** 🛵
   - Cross lanes horizontally
   - Avoidable (driver dodges)
   - Orange delivery theme

2. **Stray Cats** 🐈
   - Patrol back and forth
   - Run away when ball approaches
   - Small, cute animations

3. **Shopping Carts** 🛒
   - Roll when hit with physics
   - Can be destroyed with charged shots
   - Realistic metal appearance

4. **Skateboarders** 🛹
   - Move across lanes
   - Jump over balls (avoidable)
   - Teen character representation

5. **Pigeons** 🕊️
   - Flying obstacles
   - Scatter when player approaches
   - Ground-pecking behavior

### 🌆 Environmental Hazards
- **Open Manholes** 🕳️ - Must jump over
- **Water Puddles** 💧 - Slow movement, splash effects

## Enhanced Features

### Movement Patterns
- **Static**: Traditional obstacles that don't move
- **CrossingLanes**: Move horizontally across all lanes
- **Patrol**: Move back and forth in limited area
- **Rolling**: Physics-based movement when hit
- **Wobbling**: Gentle swaying motion
- **Flying**: Elevated movement patterns

### Collision Behaviors
- **Destroyable**: Can be broken by soccer ball
- **Indestructible**: Cannot be destroyed, ball bounces
- **Avoidable**: NPC dodges the ball
- **Bouncy**: Ball ricochets off in different direction

### Special Effects System
- **Material-Specific Effects**: Wood splinters, metal sparks, glass shards
- **Water Spray**: Fire hydrants create realistic water effects
- **Coin Drops**: Vendor carts and trash bins reward players
- **Chain Reactions**: Some obstacles trigger nearby destruction
- **Screen Shake**: Impact feedback for major collisions

### Visual & Audio
- **Dynamic Textures**: Pristine → Damaged → Critical states
- **Particle Effects**: Appropriate to material type
- **Sound Effects**: Impact sounds matching materials
- **Trail Effects**: Different for each obstacle interaction

## Difficulty Scaling
- **Level 0**: Basic obstacles (trash bins, cats, signs)
- **Level 1**: Medium obstacles (metal bins, scooters, carts)
- **Level 2+**: Advanced obstacles (cars, skateboarders)

Difficulty increases every 30 seconds of gameplay.

## Lane-Specific Spawning
- **Side Lanes Only**: Parked cars (too big for center)
- **All Lanes**: Most small obstacles
- **Preferred Lanes**: Some obstacles favor certain positions

## Integration with Existing Systems

### Auto-Aim Compatibility
- Avoidable obstacles are skipped by auto-aim system
- Different destruction effects based on obstacle type
- Maintains lane-based targeting accuracy

### Scoring System
- Coin collection bonuses from vendor carts
- Destruction combo multipliers
- Avoidance points for dynamic obstacles

### Physics Integration
- Rolling obstacles use Unity physics
- Bounce mechanics for indestructible items
- Realistic collision responses

## Urban Theme Elements
The obstacles create an authentic city street experience:
- **Morning**: Fresh vendor carts, clean streets
- **Busy**: More scooters and pedestrians
- **Residential**: Cats, flower pots, parked cars
- **Commercial**: Delivery vehicles, shopping areas

## Technical Implementation

### Files Created
- `UrbanObstacle.cs` - Main urban obstacle behavior
- `ObstacleMovement.cs` - Movement pattern system
- `ObstacleEffects.cs` - Visual and audio effects
- Updated `ObstacleSpawner.cs` - Enhanced spawning logic
- Updated `Ball.cs` - Urban obstacle interactions

### Configuration
The system is fully configurable through Unity Inspector:
- Spawn weights for each obstacle type
- Movement speeds and patterns
- Effect intensities and colors
- Lane preferences and restrictions

## Gameplay Impact
This system transforms the basic obstacle course into a living, breathing city environment where Mariam's journey feels authentic and engaging. Players must adapt to both static hazards and dynamic NPCs while collecting coins to help her grandmother.

The urban theme creates emotional connection - every obstacle feels like a real part of Mariam's neighborhood adventure rather than abstract game mechanics.