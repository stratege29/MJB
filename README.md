# Just Play Mariam - Unity 6 Prototype

## 📱 Project Overview
This is a Unity 6 prototype for "Just Play Mariam," an endless runner mobile game featuring Mariam as the main character. The prototype validates core gameplay mechanics including running, lane switching, jumping, sliding, and shooting, fully optimized for Unity 6000.2.3f1.

## 🎯 Gameplay Features
- **Auto-run**: Character automatically runs forward with progressive speed increase
- **Lane System**: Three-lane corridor for strategic movement
- **Movement Controls**:
  - Swipe Left/Right: Change lanes
  - Swipe Up: Jump over obstacles
  - Swipe Down: Slide under obstacles
- **Shooting System**:
  - Tap: Quick ball shot with auto-targeting
  - Tap & Hold: Charged shot that destroys multiple obstacles
- **Scoring System**:
  - +1 point per coin collected
  - +5 points per obstacle destroyed
  - Combo multiplier for consecutive successful actions
- **Progressive Difficulty**: Speed increases over time, spawn intervals decrease

## 📂 Project Structure
```
Assets/
├── Scripts/           # All C# gameplay scripts
├── Scenes/           # Unity scene files
├── Prefabs/          # Reusable game objects
├── Materials/        # Visual materials
├── Audio/           # Sound effects and music
└── UI/              # User interface assets
```

## 🛠 Setup Instructions

### 1. Unity 6 Setup
- **Unity Version**: Unity 6000.2.3f1 (Required)
- **Render Pipeline**: Universal Render Pipeline (URP) 
- **Input System**: New Input System (Legacy Input disabled)
- Open project directly with Unity 6 - all packages auto-configured

### 2. Scene Setup
1. Open `Assets/Scenes/MainGame.unity`
2. Add the `GameSetup` script to an empty GameObject in the scene
3. Run the scene - the GameSetup script will automatically configure all components

### 3. Manual Setup (Alternative)
If you prefer manual setup:

1. **Create Player**:
   - Add a Capsule primitive
   - Attach: `PlayerController`, `ShootingSystem`, `Rigidbody`
   - Tag as "Player"

2. **Configure Camera**:
   - Add `CameraFollow` script to Main Camera
   - Position behind and above player

3. **Add Managers**:
   - Create empty GameObjects with these scripts:
     - `GameManager`
     - `InputManager`
     - `UIManager`
     - `ObstacleSpawner`
     - `CollectibleManager`

4. **Setup Ground**:
   - Create stretched cube as ground
   - Tag as "Ground"

### 4. Required Tags
Create these tags in Unity Editor (Window > Project Settings > Tags & Layers):
- Player
- Obstacle
- Collectible
- Ball
- Ground

### 5. Build Settings
For mobile deployment:
- **Android**: Switch platform, configure package name
- **iOS**: Switch platform, configure bundle identifier
- **Player Settings**: Set company name, product name, icons

## 🎮 Controls

### Unity 6 Editor Testing (Mouse/Keyboard)
- **Mouse Drag**: Swipe gestures (New Input System)
- **Click**: Tap (InputAction callbacks)
- **Click & Hold**: Charged shot (Unity 6 enhanced detection)

### Mobile Controls (Unity 6 New Input System)
- **Swipe Left/Right**: Change lanes (Enhanced touch recognition)
- **Swipe Up**: Jump (Improved gesture detection)
- **Swipe Down**: Slide (Optimized response time)
- **Tap**: Quick shot (Reduced input latency)
- **Tap & Hold**: Charged shot (Better hold detection)

## 📊 Core Scripts Overview

### Gameplay Core
- **`GameManager.cs`**: Main game state, scoring, speed progression
- **`PlayerController.cs`**: Character movement, collision detection
- **`InputManager.cs`**: Touch/swipe input handling

### Shooting System
- **`ShootingSystem.cs`**: Ball shooting mechanics, auto-targeting
- **`Ball.cs`**: Projectile physics, collision, destruction effects

### World Generation
- **`ObstacleSpawner.cs`**: Procedural obstacle spawning
- **`Obstacle.cs`**: Obstacle behavior, destruction
- **`CollectibleManager.cs`**: Coin spawning system
- **`Collectible.cs`**: Collectible animation, collection

### Camera & UI
- **`CameraFollow.cs`**: Third-person camera tracking
- **`UIManager.cs`**: Menu, HUD, game over screens

### Utility
- **`GameSetup.cs`**: Automated scene configuration

## 🚀 Testing the Prototype

### In Unity Editor
1. Press Play in Unity Editor
2. Use mouse to simulate touch gestures
3. Test all movement and shooting mechanics

### Mobile Testing
1. Build for your target platform (Android/iOS)
2. Install on device
3. Test touch controls and performance

## 🎨 Customization

### Visual Improvements
- Replace primitive shapes with actual 3D models
- Add particle effects for destruction and collection
- Implement proper UI design with African cultural themes

### Gameplay Tuning
- Adjust speed progression in `GameManager`
- Modify obstacle spawn rates in `ObstacleSpawner`
- Balance scoring system and combo multipliers

### Audio Integration
- Add sound effects to destruction, collection, movement
- Implement background music with African-inspired themes

## 📋 Prototype Goals Checklist
- ✅ Auto-run forward movement
- ✅ Three-lane switching system
- ✅ Jump and slide mechanics
- ✅ Touch input (swipe/tap) controls
- ✅ Ball shooting with quick and charged shots
- ✅ Auto-targeting system
- ✅ Obstacle spawning and collision
- ✅ Collectible system with coins
- ✅ Scoring and combo multiplier
- ✅ Progressive speed increase
- ✅ Basic UI (menu, HUD, game over)
- ✅ Third-person camera
- ✅ Mobile-ready build configuration

## 🐛 Known Issues & Future Improvements
- Add particle effects for better visual feedback
- Implement proper 3D character model and animations
- Add sound effects and music
- Create more obstacle varieties
- Implement power-ups system
- Add level progression and achievements

## 📞 Support
For questions about the prototype implementation, refer to the individual script comments or Unity documentation for specific component usage.

---
**Status**: Unity 6 Upgrade Complete ✅  
**Build Target**: Android & iOS (Optimized)  
**Unity Version**: 6000.2.3f1 (Required)  
**Render Pipeline**: Universal Render Pipeline  
**Input System**: New Input System  
**Performance**: Mobile Optimized with Unity 6 Enhancements