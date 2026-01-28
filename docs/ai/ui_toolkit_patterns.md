# UI Toolkit Patterns & Conventions

## Learning Approach

This project is a learning environment for UI Toolkit. Patterns are evolving as understanding deepens.

**Primary Reference**: Unity's **QUIZ U** project
- Following examples and patterns demonstrated in QUIZ U
- Conventions are not yet fully solidified
- Open to refinement as best practices emerge

## Custom Visual Elements

### Location
- All custom elements in `/UI/Runtime/CustomElements/`
- Organized by element type or related functionality

### Current Custom Elements

#### Bound Progress Elements
Base class for data-bound progress displays:
- `BoundProgressElementBase.cs` - Abstract base for progress UI
- `BarProgressElement.cs` - Linear progress bar
- `RadialProgress.cs` - Circular/radial progress indicator
- `ThresholdMeterElement.cs` - Progress with threshold markers

#### Dialogue Elements
- `DialogueElement.cs` - Custom dialogue display component
- Integrates with dialogue system

#### Other Elements
- `AnimatedTextFieldElement.cs` - Text fields with animation support
- `BarElement.cs` - Generic bar element
- `EllipseMesh.cs` - Custom mesh generation for UI

### Element Creation Pattern (Evolving)
When creating custom VisualElements:
1. Inherit from `VisualElement` or appropriate base class
2. Define USS class names as constants
3. Use `[UxmlElement]` attribute for UXML support
4. Implement data binding with `[CreateProperty]` where needed
5. Keep logic minimal - delegate to view models when possible

## Data Binding

### Exposing Data to UI
Use Unity's `[CreateProperty]` attribute to expose ScriptableObject properties to data binding:

```csharp
using Unity.Properties;

[CreateAssetMenu(fileName = "FloatData.asset", menuName = "Game/Variables/FloatData")]
public class FloatData : FloatVariable {
    [CreateProperty]
    public float ValueUI => Value;
    
    [CreateProperty]
    public float MaxUI => Max;
    
    [CreateProperty]
    public float MinUI => Min;
}
```

### Binding in UXML
```xml
<ui:VisualElement binding-path="someProperty" />
```

### Binding in Code
```csharp
// Set data source
element.dataSource = myScriptableObject;

// Or bind specific property
element.SetBinding(nameof(myProperty), new DataBinding {
    dataSourcePath = new PropertyPath(nameof(MyData.Value))
});
```

## USS Organization

### File Location
- All USS files in `/UI/Runtime/Styles/`
- May be organized by subsystem (e.g., `/Styles/Dialogue/`, `/Styles/General/`)

### Current Structure
```
UI/Runtime/Styles/
├── General_USS/          # General-purpose styles
└── [Subsystem styles]    # System-specific styles as needed
```

### Naming Conventions (Evolving)
Following USS best practices from Unity documentation:
- Use kebab-case for class names: `.my-custom-element`
- Prefix custom classes to avoid conflicts: `.game-button`, `.dp-panel` (dp = DustProphet)
- Use semantic names over visual names: `.primary-button` vs `.blue-button`

**Example:**
```css
/* General element styles */
.game-panel {
    background-color: rgb(30, 30, 30);
    border-radius: 5px;
    padding: 10px;
}

/* DustProphet-specific */
.dp-stat-bar {
    height: 20px;
    background-color: rgb(50, 50, 50);
}
```

## UXML Document Organization

### File Location
- All UXML files in `/UI/Documents/`
- Organized by subsystem matching Runtime structure
  - `/Documents/Dialogue/`
  - `/Documents/DustProphet/`
  - `/Documents/CustomElements/`

### Document Structure (Following QUIZ U patterns)
- Use templates for reusable components
- Keep documents focused (single responsibility)
- Reference USS for styling rather than inline styles where possible

**Example Template Pattern:**
```xml
<ui:UXML>
    <Template name="DialogueTemplate">
        <ui:VisualElement class="dialogue-container">
            <ui:Label name="speaker-name" class="dialogue-speaker" />
            <ui:Label name="dialogue-text" class="dialogue-text" />
        </ui:VisualElement>
    </Template>
</ui:UXML>
```

## UI State Management

### State Machine Pattern
Located in `/UI/Runtime/StateMachine/`

Current structure:
- `/StateMachine/States/` - State implementations
- `/StateMachine/Links/` - State transition logic
- `/StateMachine/Interfaces/` - State interfaces

This follows a state pattern for managing UI transitions and flows.

## Text Animation Integration

### Text Animator by Febucci
- Integration scripts in `/UI/Runtime/TextAnimator/`
- Used for dialogue text effects and UI polish
- `TypewriterAudio.cs` - Audio feedback for text reveal

## UI Converters

### Location
`/UI/Runtime/Converters/`

### Purpose
Data type converters for UI Toolkit binding system when direct binding isn't possible.

## UI Utilities

### Location
`/UI/Runtime/Utilities/`

### Common Utilities
- Helper functions for UI manipulation
- Common UI operations
- Extension methods for VisualElements

## Filter Effects

### Location
`/UI/Runtime/Filters/`

### Current Filters
- Pixel Glitch Sweep effect
- Custom shader-based UI effects

**Pattern:**
- Filter effects as ScriptableObjects or components
- Applied to UI via custom logic
- Can be event-driven (e.g., triggered on dialogue state change)

## Event-Driven UI Architecture

UI systems primarily use **events** for communication:

```csharp
// Example: UI Panel responding to game events
public class StatsPanel : MonoBehaviour {
    private void OnEnable() {
        GameEvents.OnStatsChanged += UpdateDisplay;
    }
    
    private void OnDisable() {
        GameEvents.OnStatsChanged -= UpdateDisplay;
    }
    
    private void UpdateDisplay(StatsData data) {
        // Update UI elements
    }
}
```

**Benefits for UI:**
- Decouples UI from game systems
- Multiple UI panels can respond to same event
- Easy to add/remove UI elements

## MVVM Implementation (UI-Specific)

### Model
ScriptableObjects representing data:
```csharp
// Model: In /Data/
[CreateAssetMenu]
public class MiningStatsSO : ScriptableObject {
    public float drillPower;
    public float heatLevel;
    public int resourcesCollected;
}
```

### View
UXML documents + USS styling:
```xml
<!-- View: In /UI/Documents/ -->
<ui:VisualElement class="stats-panel">
    <ui:Label binding-path="drillPower" name="drill-power-label" />
    <ui:ProgressBar binding-path="heatLevel" name="heat-bar" />
</ui:VisualElement>
```

### ViewModel
C# classes bridging Model and View:
```csharp
// ViewModel: In /UI/Runtime/
public class MiningStatsViewModel : MonoBehaviour {
    [SerializeField] private MiningStatsSO _stats;
    private UIDocument _document;
    
    private void OnEnable() {
        _document = GetComponent<UIDocument>();
        _document.rootVisualElement.dataSource = _stats;
        
        // Listen to changes
        GameEvents.OnMiningUpdate += UpdateStats;
    }
    
    private void UpdateStats(MiningData data) {
        _stats.drillPower = data.power;
        _stats.heatLevel = data.heat;
        // SO will notify bound UI automatically
    }
}
```

## Best Practices (In Development)

### Performance
- Cache VisualElement queries (don't query every frame)
- Use USS classes over inline styles
- Minimize rebuilds of visual trees

### Maintainability  
- Keep UXML documents under 200 lines
- Break complex UIs into templates
- Use meaningful names for USS classes and element names

### Debugging
- Use USS border colors temporarily to verify layout
- Unity UI Toolkit Debugger for live inspection
- Name all important elements for easier querying

## Open Questions / Areas to Explore

These are areas still being researched and refined:
- Best practices for complex animations
- Optimal state management for multi-screen UIs
- Performance optimization for heavy UI updates
- Integration patterns with legacy UI system (when needed)

---

**Note**: These patterns are evolving as the project progresses. When in doubt, refer to Unity's QUIZ U project examples and Unity's official UI Toolkit documentation.
