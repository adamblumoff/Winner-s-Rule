# Player Sprite Selection Implementation Plan

Based on my research, here's how we can implement player sprite selection in a simple way:

## Current System Analysis
- **Existing Sprites**: One character with full animations (Idle, Run, Jump, Slide, Dead) in `Assets/Sprites/GravityPlayer/`
- **Animation System**: Uses Animator with parameters (`isMoving`, `isDashing`, `Death` trigger)
- **Player Controller**: Uses SpriteRenderer and Animator components
- **Lobby Structure**: Already has arrays for 4 players with name inputs and toggles

## Implementation Approach

### 1. Character Sprite Database
- Create a `CharacterSpriteDatabase` ScriptableObject to hold available character options
- Each character entry contains:
  - Character name (e.g., "Knight", "Wizard", "Rogue")
  - Animator Controller (with all required animation states)
  - Preview sprite (for lobby UI selection)

### 2. Lobby UI Enhancement
- Add sprite selection UI elements next to each player's setup:
  - Small preview image showing current selected character
  - Left/right arrow buttons to cycle through available characters
  - Or dropdown menu with character names
- Store selection in new `playerSelectedSprites` array in LobbyUI

### 3. GameStateManager Extension
- Add `playerSelectedCharacters` array (similar to `playerNames`)
- Pass character selections from lobby to GameStateManager via `StartMatch()`
- Persist character choices throughout the game session

### 4. Player Controller Modification
- Modify `GravityFlipPlayerController.Start()` to:
  - Check GameStateManager for current player's selected character
  - Replace default Animator Controller with selected character's Animator
  - Update SpriteRenderer to use selected character's sprites

### 5. Character Asset Creation
- Initially: Create 3-4 additional character variants using existing animation structure
- Each character needs its own Animator Controller with identical parameters but different sprites
- Keep same animation timing and state machine structure for consistency

## Benefits of This Approach
- **Simple**: Reuses existing animation system and timing
- **Scalable**: Easy to add more characters later
- **Consistent**: All characters behave identically (gameplay balance maintained)
- **User-Friendly**: Clear visual selection in lobby
- **Minimal Code Changes**: Leverages existing architecture

## Implementation Steps
1. Create CharacterSpriteDatabase ScriptableObject
2. Design additional character sprite variants
3. Create Animator Controllers for each character
4. Enhance Lobby UI with character selection
5. Update GameStateManager to store character choices
6. Modify Player Controller to apply selected character
7. Test character selection and persistence across game flow

This approach keeps it simple while providing meaningful visual customization that players will enjoy!