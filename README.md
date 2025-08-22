# Winner's Rule

A competitive party game built in Unity where players compete in dynamic minigames with ever-changing rules.  
Players compete in short minigames, and each round's winner drafts a temporary rule card that changes how the next round plays. Cards provide powerful advantages but include meaningful drawbacks, forcing strategic choices.

---

## 🎮 Gameplay Loop
1. **Play a minigame** – compete in dynamic challenges with current active rules.  
2. **Determine a winner** – scores update based on performance.  
3. **Winner's draft** – winner chooses one of three game-specific rule cards.  
   - Examples: Quick Recovery (+150% move speed, dash limited to once per gravity cycle),  
     Momentum Master (+300% dash speed, +300% dash cooldown),  
     Iron Constitution (+6 hit points, +90% hazard spawn rate).  
4. **Next round** – active rules modify gameplay, expired rules clear.  
5. **Final score** – after multiple rounds, highest total score wins.

---

## 🛠 Current Features

### Minigames
- **Gravity Flip Dodge**: Survival game with dynamic gravity flips, item spawning, and dash mechanics
- **2D Racing**: Classic race-to-the-goal minigame (legacy)

### Game Systems
- **Dynamic Rule Cards**: Game-specific cards that modify gameplay mechanics
- **Active Effects UI**: Visual display of currently active rule effects with tooltips
- **Round Management**: Complete lobby → game → results → draft → next round loop
- **Score System**: Performance-based scoring with draft bonus rewards

### Player Features
- **Advanced Animation System**: Idle, walking, dashing, and death animations
- **Smooth Sprite Rotation**: Visual feedback for gravity changes
- **Dynamic Physics**: Gravity-responsive movement with realistic collision detection
- **Box Collider Adjustment**: Prevents floating appearance in different animation states

### Technical Features
- **Object Pooling**: Optimized item spawning system
- **Poisson Process Spawning**: Realistic item generation patterns
- **Config Reset System**: Prevents permanent stat modifications between rounds
- **MCP Integration**: Unity editor automation and tooling

---

## 🚧 Roadmap

### Short Term
- **Expand Gravity Flip Cards**: Add 4-5 more unique rule cards for variety
- **Audio System**: Sound effects for actions, ambient music, audio feedback
- **Visual Polish**: Particle effects, screen shake, visual juice improvements
- **Balance Iteration**: Refine card effects based on playtesting

### Medium Term  
- **Additional Minigames**: 2-3 new game modes (aim-based, reflex, physics puzzles)
- **Multiplayer Support**: Local multiplayer implementation and testing
- **UI/UX Polish**: Improved animations, better visual hierarchy, accessibility
- **Save System**: Persistent player stats and unlockable content

### Long Term
- **Networking**: Online multiplayer capability
- **Tournament Mode**: Bracket-style competitive play
- **Level Editor**: Community-created minigame content
- **Steam Integration**: Achievements, leaderboards, workshop support

---

## ▶️ How to Run
- **Requirements**: Unity 2022.3 LTS or later
- **Setup**: Open project in Unity and let packages resolve
- **Play**: Run `MainMenu` scene for main menu navigation
- **Controls**: WASD for movement, Space for dash/actions
- **Scenes**: 
  - `MainMenu`: Game entry point and navigation
  - `Lobby`: Player setup and game configuration  
  - `GravityFlipDodge`: Main minigame scene
  - `Results`: Score display and progression  

---

## 📄 License
Development prototype for testing and feedback. All rights reserved.
