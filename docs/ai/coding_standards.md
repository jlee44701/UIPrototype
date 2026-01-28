# Coding Standards & Patterns

## Naming Conventions

### ScriptableObjects
- **Script names**: End with `SO` suffix
  - ✅ Example: `DialogueSequenceSO.cs`, `CharacterSO.cs`, `FloatData.cs`
- **Asset instances**: Use descriptive names without suffix
  - ✅ Example: `TutorialDialogue.asset`, `PlayerCharacter.asset`

### Folders
- **UI-specific folders**: Use underscores for visibility in favorites plugin
  - ✅ Example: `UI_Runtime`, `UI_Scripts` (if needed)
- **Other folders**: Use PascalCase without underscores
  - ✅ Example: `CustomElements`, `DustProphet`, `Gameplay`

### General C# Conventions
- PascalCase for public members, methods, and types
- camelCase for private fields (prefix with underscore optional)
- UPPER_SNAKE_CASE for constants

## Architectural Patterns

### UI Architecture: MVVM (Model-View-ViewModel)
- **Model**: ScriptableObjects in `/Data/`
- **View**: UXML documents in `/UI/Documents/`
- **ViewModel**: C# classes in `/UI/Runtime/` that handle UI logic and data binding
- **Event-Driven**: Use events for UI-related communication
  - UI elements communicate via events
  - Keeps UI decoupled from game logic

### Game Logic Architecture: Data-Oriented Design

**Core Principles:**
- **Minimize OOP reliance**: Avoid deep inheritance hierarchies and scattered state
- **Favor static classes or instances**: Use static classes for stateless logic, or instantiate within MonoBehaviours
- **Keep side effects at top level**: All side effects should be in a single, visible location (typically a manager/coordinator)
- **Avoid event-driven for game logic**: Events create opaque dependencies; prefer explicit parameter passing
- **Accept more parameters**: Explicitly pass data rather than hiding dependencies

**Structure:**
```csharp
// ✅ GOOD: Logic in regular C# class, instantiated in MonoBehaviour
public class MiningCalculator {
    public MiningResult CalculateYield(float drillPower, MaterialType material, float depth) {
        // Pure logic, no side effects
        // All inputs explicit
    }
}

public class MiningManager : MonoBehaviour {
    private MiningCalculator _calculator = new();
    
    void Update() {
        // All side effects here - visible and traceable
        var result = _calculator.CalculateYield(drillPower, currentMaterial, depth);
        ApplyResult(result);  // Side effect clearly at top level
    }
}

// ✅ ALSO GOOD: Stateless static utility
public static class NoiseGenerator {
    public static float[,] GenerateNoiseMap(int width, int height, NoiseSettings settings) {
        // Pure function, no state
    }
}
```

```csharp
// ❌ AVOID: Logic scattered in MonoBehaviour with hidden side effects
public class MiningSystem : MonoBehaviour {
    void Update() {
        // Side effects buried in method calls
        CheckForMining();  // What does this do? Have to read implementation
    }
    
    private void CheckForMining() {
        // More hidden side effects
        UpdateResources();  // Have to hunt through multiple methods
    }
}
```

## MonoBehaviour Usage

### When to Use MonoBehaviour
- **Scene management**: Objects that need to exist in the scene
- **Unity lifecycle hooks**: When you need `Start()`, `Update()`, `OnTriggerEnter()`, etc.
- **Coordinate systems**: Top-level managers that orchestrate game logic

### When to Use Regular C# Classes
- **Game logic**: Pure logic that doesn't need Unity lifecycle
- **Calculations**: Math, procedural generation, data processing
- **State containers**: Simple data structures

**Pattern:**
```csharp
// Logic: Regular C# class
public class TerrainGenerator {
    public Terrain GenerateTerrain(GenerationParams params) {
        // All logic here, no Unity dependencies
    }
}

// Coordinator: MonoBehaviour
public class TerrainManager : MonoBehaviour {
    private TerrainGenerator _generator = new();
    
    void Start() {
        // All Unity interactions and side effects here
        var terrain = _generator.GenerateTerrain(params);
        ApplyTerrainToScene(terrain);
    }
}
```

## ScriptableObject Patterns

### Organization
- **Script definitions**: Can live in `/Scripts/ScriptableObjects/` or alongside related code
- **Asset instances**: Always in `/Data/` organized by type/system

### Data Binding with UI Toolkit
When exposing SO data to UI Toolkit's data binding:

```csharp
[CreateAssetMenu(fileName = "FloatData.asset", menuName = "Game/Variables/FloatData")]
public class FloatData : ScriptableObject {
    [SerializeField] private float _value;
    
    // Expose properties with [CreateProperty] for UI binding
    [CreateProperty]
    public float ValueUI => _value;
    
    [CreateProperty]
    public float MaxUI => 100f;
}
```

## Event System Usage

### Use Events For:
- ✅ UI communication (button clicks, panel transitions)
- ✅ Loose coupling between UI systems
- ✅ Broadcasting state changes to multiple listeners

### Avoid Events For:
- ❌ Game logic flow (makes debugging harder)
- ❌ Systems that need explicit control flow
- ❌ Direct communication between game systems

**Prefer explicit calls:**
```csharp
// ✅ GOOD: Explicit, traceable
public void ProcessMining(Vector3 position, float power) {
    var material = _terrain.GetMaterialAt(position);
    var result = _calculator.CalculateYield(power, material);
    _inventory.AddResources(result.resources);
    _ui.UpdateDisplay(result);
}

// ❌ AVOID: Hidden dependencies via events
public void ProcessMining(Vector3 position, float power) {
    MiningEvent.Invoke(position, power);  // Who's listening? What happens?
}
```

## Code Organization Preferences

### File Structure
- One public class per file
- File name matches class name
- Group related private helper classes in the same file if they're only used there

### Dependencies
- Make dependencies explicit through constructor injection or public properties
- Avoid Service Locator pattern or global singletons where possible
- If using managers, inject them or pass them as parameters

### Comments
- XML documentation for public APIs
- Inline comments for complex logic or non-obvious decisions
- TODO comments for incomplete features

## Unity-Specific Preferences

### Inspector Organization
- Use `[Header("Section Name")]` to organize inspector fields
- Use `[SerializeField]` for private fields that need inspector access
- Keep public fields minimal (prefer properties)

### Serialization
- Prefer ScriptableObjects for data that needs to be authored
- Use JSON/binary serialization for runtime save data
- Don't rely on Unity serialization for complex data structures

## Allocation Management

### Minimize Allocations
Avoid unnecessary memory allocations, especially in frequently-called code:

**Cache Collections:**
```csharp
// ✅ GOOD: Reuse collection
public class TerrainGenerator {
    private List<Chunk> _chunkBuffer = new List<Chunk>();
    
    public void GenerateChunks() {
        _chunkBuffer.Clear();  // Reuse instead of new List<Chunk>()
        // populate buffer
    }
}

// ❌ AVOID: Allocate every call
public void GenerateChunks() {
    var chunks = new List<Chunk>();  // New allocation every time
}
```

**Cache Objects:**
```csharp
// ✅ GOOD: Cache expensive objects
private StringBuilder _stringBuilder = new StringBuilder(256);

public string FormatStats(Stats stats) {
    _stringBuilder.Clear();
    _stringBuilder.Append("Health: ").Append(stats.health);
    return _stringBuilder.ToString();
}

// ❌ AVOID: Create new every time
public string FormatStats(Stats stats) {
    return "Health: " + stats.health;  // String concatenation creates garbage
}
```

**Avoid LINQ in Hot Paths:**
```csharp
// ✅ GOOD: Manual iteration (zero allocation)
public Entity FindClosest(Vector3 position) {
    Entity closest = null;
    float minDist = float.MaxValue;
    
    for (int i = 0; i < entities.Count; i++) {
        float dist = Vector3.Distance(position, entities[i].position);
        if (dist < minDist) {
            minDist = dist;
            closest = entities[i];
        }
    }
    return closest;
}

// ❌ AVOID: LINQ in Update/hot paths (allocates)
public Entity FindClosest(Vector3 position) {
    return entities.OrderBy(e => Vector3.Distance(position, e.position)).First();
}
```

**Note**: LINQ is fine for initialization, editor code, or infrequent operations. Just avoid in `Update()`, per-frame logic, or tight loops.

## Static State Management

### Be Intentional with Static State
Unity's domain reload behavior requires careful handling of static state.

**The Problem:**
- Static fields persist through Play Mode entry/exit in Editor (unless domain reload is enabled)
- Can cause bugs where state from previous play session affects next session
- Hard to debug "works first time, breaks second time" issues

**The Solution - Be Explicit:**

```csharp
// ✅ GOOD: Initialize static state explicitly
public static class NoiseCache {
    private static Dictionary<int, float[,]> _cache;
    
    // Called explicitly to reset state
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() {
        _cache = new Dictionary<int, float[,]>();
    }
    
    public static void ClearCache() {
        _cache?.Clear();
    }
}

// ✅ GOOD: Document static state intent
/// <summary>
/// Static registry of all terrain chunks.
/// NOTE: Static state - explicitly cleared on domain reload via Init()
/// </summary>
public static class TerrainRegistry {
    private static Dictionary<Vector2Int, Chunk> _chunks;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init() {
        _chunks = new Dictionary<Vector2Int, Chunk>();
    }
}
```

**When Static State is Appropriate:**
- ✅ Truly global registries/caches that need to persist
- ✅ Stateless utility classes (pure functions)
- ✅ Constants and readonly data
- ✅ When you explicitly want persistence through domain reload (and document why)

**When to Avoid Static State:**
- ❌ Game state that should reset between play sessions
- ❌ References to SceneObjects or Components (become null on scene changes)
- ❌ When instance-based approach is simpler

**Testing Static State:**
```csharp
// Add this to catch domain reload issues
#if UNITY_EDITOR
[InitializeOnLoadMethod]
static void EditorInit() {
    // This runs when scripts recompile in Editor
    // Use to verify static state is handled correctly
    UnityEngine.Debug.Log("Static state reinitialized");
}
#endif
```

### Editor vs Runtime Initialization
```csharp
public static class MySystem {
    private static bool _initialized;
    
    // For runtime (builds)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void RuntimeInit() {
        Init();
    }
    
    // For editor (play mode + recompile)
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInit() {
        Init();
    }
#endif
    
    static void Init() {
        if (_initialized) return;
        // Initialize static state
        _initialized = true;
    }
}
```

## Performance Considerations
- Cache component references (don't call `GetComponent` in `Update`)
- Use object pooling for frequently instantiated objects
- Profile before optimizing
- Keep `Update` methods lightweight
- Prefer array iteration over foreach when performance critical (avoids enumerator allocation)
- Use `stackalloc` for small temporary buffers when safe (C# 7.2+)
