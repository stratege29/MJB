# Scene Setup Instructions for Unity 6

## Quick Setup (Automatic)

To get the game running immediately, follow these steps:

### 1. Add Bootstrap Scripts to Scene

In Unity Editor:

1. **Create Empty GameObject**:
   - Right-click in Hierarchy
   - Create Empty
   - Name it "SceneBootstrapper"

2. **Add SceneBootstrapper Component**:
   - Select the "SceneBootstrapper" GameObject
   - In Inspector, click "Add Component"
   - Search for "Scene Bootstrapper"
   - Add the component

3. **Add AutoGameStarter**:
   - Create another Empty GameObject
   - Name it "AutoGameStarter"
   - Add the "Auto Game Starter" component

4. **Add Compatibility Test** (Optional):
   - Create another Empty GameObject
   - Name it "Unity6CompatibilityTest"
   - Add the "Unity 6 Compatibility Test" component

### 2. Save and Play

1. **Save the Scene**: Ctrl+S (Windows) or Cmd+S (Mac)
2. **Press Play**: The scene will automatically create:
   - Player (brown capsule)
   - Ground/Path (sandy colored)
   - Lane markers (white lines)
   - All managers (GameManager, InputManager, etc.)
   - Camera with follow script
   - Lighting

## What You Should See

After pressing Play:

✅ **Player**: Brown capsule character in the center lane
✅ **Ground**: Sandy-colored path extending forward
✅ **Lane Markers**: White lines showing the 3 lanes
✅ **Camera**: Third-person view following the player
✅ **Controls**: On-screen instructions in the top-left
✅ **Console**: Success messages showing all systems working

## Controls

**KEYBOARD CONTROLS** (No Input System conflicts):
- **A/D Keys**: Change lanes
- **W Key**: Jump
- **S Key**: Slide
- **SPACE**: Shoot ball (hold for charged shot)

## Troubleshooting

### If nothing appears:
1. Check that SceneBootstrapper has "Setup On Awake" enabled
2. Look at Console for error messages
3. Ensure all scripts are in the Assets/Scripts folder

### If controls don't work:
1. Check that KeyboardInputManager was created (see Console)
2. Click in the Game view to focus it
3. Use A/D/W/S/SPACE keys (not mouse/touch)

### If you see errors:
1. Make sure all script files are present in Assets/Scripts/
2. Check that packages are properly resolved
3. Restart Unity if needed

## Manual Setup (Alternative)

If automatic setup doesn't work, you can manually add:

1. **GameManager**: Empty GameObject + GameManager component
2. **Unity6InputManager**: Empty GameObject + Unity6InputManager component
3. **Player**: Capsule primitive + PlayerController + ShootingSystem
4. **Camera**: Camera + CameraFollow component
5. **Ground**: Cube primitive scaled to (6, 1, 100)

## Files Required

Make sure these files exist in Assets/Scripts/:
- SceneBootstrapper.cs
- AutoGameStarter.cs
- Unity6InputManager.cs
- PlayerController.cs
- GameManager.cs
- ShootingSystem.cs
- CameraFollow.cs
- Ball.cs
- (Other game scripts)

## Expected Console Output

When working correctly, you should see:
```
=== Scene Bootstrapper Starting ===
✓ Created GameManager
✓ Created Unity6InputManager
✓ Created UIManager
✓ Created AdaptivePerformanceManager
✓ Created Ground
✓ Created Lane Markers
✓ Created Player with components
✓ Camera configured
✓ Game settings configured
=== Scene Bootstrap Complete ===
✓ Game started!
```

Your Unity 6 "Just Play Mariam" prototype is now ready to play! 🎮