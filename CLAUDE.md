# Winner's Rule - Development Instructions

## Project Overview
Winner's Rule is a competitive party game where players compete in dynamic minigames with rule cards that modify gameplay. The current focus is on the "Gravity Flip Dodge" minigame - a survival game with dynamic gravity changes.

## Key Game Systems

### Gravity Flip Dodge Minigame
- **Core Mechanics**: Player dodges items while gravity flips periodically
- **Player Features**: Movement, dashing, collision detection, death animations
- **Rule Integration**: Cards modify player stats (speed, dash cooldown, hit points, etc.)
- **Animation System**: Idle, walking, dashing, death states with gravity-responsive sprite rotation

### Rule Card System  
- **Game-Specific Cards**: Different minigames have different card pools
- **Active Effects**: Visual UI showing current rule effects with tooltips
- **Stat Modification**: Temporary changes that reset between rounds
- **Current Cards**: Quick Recovery, Momentum Master, Iron Constitution

### Technical Architecture
- **Object Pooling**: Optimized spawning system for items
- **Config Reset System**: Prevents permanent stat changes between rounds  
- **MCP Integration**: Unity editor automation for rapid development
- **Scene Flow**: MainMenu → Lobby → GravityFlipDodge → Results → Draft → Loop

## Development Guidelines

### Unity MCP Usage
- Use the Unity MCP whenever you need to interact with Unity editor
- Prefer MCP tools for asset creation, scene management, and GameObject manipulation

### Code Standards
- NEVER create files unless absolutely necessary for achieving the goal
- ALWAYS prefer editing existing files to creating new ones  
- NEVER proactively create documentation files (*.md) or README files unless explicitly requested
- Follow existing code patterns and conventions in the codebase
- Maintain the established architecture (GameStateManager, MinigameConfig, etc.)

### Animation System
- Player animations: Idle, Walking, Dashing, Death (trigger-based)
- Sprite rotation system for gravity feedback
- Box collider adjustment for different animation states
- Death animation plays once then transitions to results

### Current Development Focus
- Expanding rule card variety for Gravity Flip Dodge
- Polish and balance iteration
- Audio and visual effects integration
- Preparing for additional minigame development
- stay simple and concise in all implementations of features