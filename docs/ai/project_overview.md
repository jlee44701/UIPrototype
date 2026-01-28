# UIPrototype Project Overview

## Project Purpose

This is a **UI-focused prototype project** for a drilling/mining mini-game that will eventually be integrated into a larger game (separate Unity project). The project serves three main goals:

1. **Build the mini-game UI** - Create the user interface for the drilling machine game
2. **Develop reusable UI systems** - Build UI Toolkit systems (filters, dialogue, etc.) for use in the main game
3. **Learn UI Toolkit** - Hands-on learning and experimentation with Unity's UI Toolkit

## Game Concept

### The Mini-Game
- **Core Mechanic**: Driving around in a drilling machine (called "DustProphet")
- **World Generation**: Procedurally generated maps using layered noise maps
  - Similar to Minecraft's material distribution system
  - Different materials/blocks distributed based on height thresholds and other parameters
  - Goal: Keep it simple and flexible for design iteration

### Current Development Status
- ✅ **UI Systems**: Active development - dialogue system, filters, custom elements
- ✅ **Data Layer**: ScriptableObject groundwork established (primarily for UI testing)
- ⏳ **Game Mechanics**: Basic groundwork laid, needs further development
- ⏳ **Procedural Generation**: Research phase - evaluating approaches for noise-based terrain

## Tech Stack

### Unity
- **Version**: Unity 6000.0.23f1 (6.3.2f1)
- **Note**: Will update to newer 6.x versions as they release

### UI System
- **Primary**: Unity UI Toolkit (UXML/USS)
- **Future**: May incorporate legacy UI system for runtime elements (still recommended by Unity for some runtime use cases)

### Key Plugins
- **Obvious SOAP**: [ScriptableObject Architecture Pattern]
- **Text Animator by Febucci**: Text animation effects

## Project Structure

```
Assets/Project/
├── UI/
│   ├── Runtime/              # All UI scripts
│   │   ├── CustomElements/   # Custom VisualElements
│   │   ├── Dialogue/         # Dialogue system (scripts + USS)
│   │   ├── DustProphet/      # DustProphet-specific UI
│   │   ├── Filters/          # UI Toolkit filter effects
│   │   ├── StateMachine/     # UI state management
│   │   ├── Base/             # Base UI classes
│   │   ├── Converters/       # Data converters for binding
│   │   ├── TextAnimator/     # Text animation integration
│   │   ├── Utilities/        # UI helper scripts
│   │   └── Editor/           # Custom editor tools
│   ├── Documents/            # UXML files (organized by subsystem)
│   ├── Font/                 # Font assets
│   └── UI_Strings/           # Localization/text resources
├── Scripts/
│   ├── Gameplay/             # Game logic (non-UI)
│   │   ├── DustProphet/      # Vehicle/drilling systems
│   │   └── Mining/           # Mining mechanics
│   ├── Managers/             # Game managers
│   ├── Events/               # Event system
│   ├── Utilities/            # General helper scripts
│   ├── Audio/                # Audio management
│   ├── ScriptableObjects/    # SO script definitions
│   └── Editor/               # Editor extensions
├── Data/                     # ScriptableObject instances
│   ├── Dialogue/
│   │   ├── DialogueEvents/
│   │   └── DialogueSequences/
│   ├── DustProphet/
│   │   ├── MiningStats/
│   │   └── ViewModels/
│   └── Characters/
├── Prefabs/                  # Prefabs organized by type
├── Audio/                    # Audio files (SFX, Music, Voice)
├── Textures/                 # Sprites and textures
├── Scenes/                   # Unity scenes
└── Settings/                 # Project settings, input actions
```

## Key Systems

### Dialogue System
- Located in `/UI/Runtime/Dialogue/`
- Integrated with UI Toolkit
- Uses ScriptableObjects for dialogue data
- Event-driven architecture

### UI Filters
- Custom filter effects for UI Toolkit
- Example: Pixel glitch sweep effect
- Located in `/UI/Runtime/Filters/`

### Custom Visual Elements
- Reusable UI components built on VisualElement
- Examples: Progress bars (bar/radial), dialogue elements, threshold meters
- Located in `/UI/Runtime/CustomElements/`

### Data Binding
- Using Unity's UI Toolkit data binding
- `[CreateProperty]` attribute for exposing SO data to UI
- Example: `FloatData` with `ValueUI`, `MaxUI`, `MinUI` properties

## Development Focus Areas

### Current Focus (In Order)
1. UI Toolkit system development and learning
2. Dialogue system integration
3. Filter effects implementation
4. Custom UI element creation

### Near-Term Goals
1. Game mechanics implementation
2. Procedural generation system research and implementation
3. DustProphet vehicle mechanics

### Future Integration
- Eventually merge this mini-game into the larger main game project
- Reuse UI systems across both projects
