# UniGame.Context

## 📦 Installation

To install the UniGame.Context package via git, add the following dependency to your Unity project's `Packages/manifest.json` file:

```json
{
  "dependencies": {
    "com.unigame.contextdata" : "https://github.com/UnioGame/unigame.context.git",
  }
}
```

## 📋 Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Core Concepts](#core-concepts)
- [Context Types](#context-types)
- [Scene Context System](#scene-context-system)
- [Async Data Sources](#async-data-sources)
- [Service Management](#service-management)
- [Serializable Context](#serializable-context)
- [Reactive Programming](#reactive-programming)
- [Advanced Features](#advanced-features)
- [Editor Tools](#editor-tools)
- [Best Practices](#best-practices)
- [Examples](#examples)

## 🚀 Features

### Core Features

- ✅ **Scene-based Context System** - Automatic scene lifecycle management
- ✅ **Addressable Integration** - Load context data from addressables
- ✅ **Auto Lifecycle Management** - Automatic cleanup and disposal
- ✅ **Async Data Sources** - Asynchronous context data loading with timeout support
- ✅ **Context Broadcasting** - Message passing between contexts
- ✅ **Global Context** - Static context access for global data sharing


## ⚡ Quick Start

### Basic Context Usage

```csharp
using UniGame.Core.Runtime;
using UniModules.UniGame.Context.Runtime.Context;

public class GameController : MonoBehaviour
{
    private EntityContext _gameContext;
    private LifeTimeDefinition _lifeTime = new();

    private void Start()
    {
        // Create context
        _gameContext = new EntityContext();
        _gameContext.AddTo(_lifeTime);

        // Publish data
        _gameContext.Publish(new PlayerData { Name = "Player1", Level = 1 });
        _gameContext.Publish(new GameSettings { Difficulty = 2 });

        // Retrieve data
        var playerData = _gameContext.Get<PlayerData>();
        var settings = _gameContext.Get<GameSettings>();

        Debug.Log($"Player: {playerData.Name}, Level: {playerData.Level}");
    }

    private void OnDestroy() => _lifeTime.Terminate();
}

[Serializable]
public class PlayerData
{
    public string Name;
    public int Level;
}

[Serializable]
public class GameSettings
{
    public int Difficulty;
}
```

### Reactive Context Subscription

```csharp
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    private LifeTimeDefinition _lifeTime = new();

    private void Start()
    {
        // Get scene context
        var sceneContext = this.GetSceneContext();
        
        // Subscribe to health changes
        sceneContext.Receive<PlayerHealth>()
            .Subscribe(health => {
                healthSlider.value = health.Current / health.Max;
            })
            .AddTo(_lifeTime);
    }

    private void OnDestroy() => _lifeTime.Terminate();
}
```

### Global Context Access

```csharp
public class GlobalDataManager : MonoBehaviour
{
    private async void Start()
    {
        // Set global context
        var gameContext = new EntityContext();
        GameContext.Context = gameContext;
        
        // Publish global data
        gameContext.Publish(new GlobalSettings { Language = "EN" });
        
        // Access from anywhere
        var globalContext = await GameContext.GetContextAsync();
        var settings = globalContext.Get<GlobalSettings>();
    }
}
```

## 🧩 Core Concepts

### IContext Interface

The foundation of the context system:

```csharp
public interface IContext : IMessageReceiver, IMessagePublisher, IDisposableContext
{
    bool HasValue { get; }
    TData Get<TData>();
    bool Contains<TData>();
    bool Remove<TData>();
}
```

### EntityContext

Main implementation of the context system:

```csharp
var context = new EntityContext();

// Publishing data
context.Publish(new GameState { Level = 1 });
context.Publish<IGameService>(new GameService());

// Retrieving data
var gameState = context.Get<GameState>();
var gameService = context.Get<IGameService>();

// Checking existence
if (context.Contains<PlayerData>())
{
    var player = context.Get<PlayerData>();
}

// Reactive subscription
context.Receive<ScoreChanged>()
    .Subscribe(score => UpdateUI(score))
    .AddTo(lifeTime);
```

### Context Connections

Merge multiple contexts for data sharing:

```csharp
var gameContext = new EntityContext();
var uiContext = new EntityContext();
var playerContext = new EntityContext();

// Create connection
var connection = gameContext.Merge(uiContext, playerContext);

// Data published to any context is available in all connected contexts
gameContext.Publish(new GameSettings());
uiContext.Publish(new ThemeData());

// All contexts can access all data
var settings = connection.Get<GameSettings>(); // Available from uiContext too
var theme = connection.Get<ThemeData>(); // Available from gameContext too

// Cleanup
connection.Dispose();
```

## 🎮 Context Types

### 1. EntityContext
General purpose context for any data:

```csharp
var context = new EntityContext();
context.Publish(anyObject);
context.Publish<IInterface>(implementation);
```

### 2. Scene Context
Automatically managed per-scene context:

```csharp
// Get current scene context
var sceneContext = this.GetSceneContext();

// Get specific scene context
var specificContext = SceneContextSystem.GetContext("MainMenuScene");

// Listen to scene context changes
this.NotifyOnSceneContext("GameplayScene")
    .Subscribe(context => {
        // Scene became active
        InitializeGameplay(context);
    })
    .AddTo(lifeTime);
```

### 3. Context Assets (Serializable)
Designer-friendly ScriptableObject approach:

```csharp
[CreateAssetMenu(menuName = "Game/Context/Player Settings")]
public class PlayerSettingsAsset : ContextAsset
{
    [SerializeField] private PlayerSettings defaultSettings;
    
    protected override PlayerSettings CreateValue() => defaultSettings;
}
```

## 🏗️ Scene Context System

### Automatic Scene Management

```csharp
public class GameSceneController : MonoBehaviour
{
    private void Start()
    {
        // Scene context is automatically created
        var context = this.GetSceneContext();
        
        // Publish scene-specific data
        context.Publish(new LevelData { Id = 1, Name = "Forest Level" });
        context.Publish<ILevelService>(new ForestLevelService());
        
        // Context automatically cleanup when scene unloads
    }
}
```

### Scene Status Monitoring

```csharp
public class SceneStatusMonitor : MonoBehaviour
{
    private void Start()
    {
        // Monitor active scene context
        this.NotifyActiveSceneContext()
            .Subscribe(context => {
                Debug.Log($"Active scene: {context.Name}, Status: {context.Status.Value}");
            })
            .AddTo(this);

        // Monitor specific scene status
        this.NotifyOnSceneContext("BossLevel")
            .Where(ctx => ctx.Status.Value == SceneStatus.Active)
            .Subscribe(context => {
                // Boss level became active
                PrepareBossFight();
            })
            .AddTo(this);
    }
}
```

## 🔄 Async Data Sources

### AsyncDataSources Asset

Manage multiple asynchronous data sources with timeout control:

```csharp
[CreateAssetMenu(menuName = "UniGame/Sources/AddressableAsyncSources")]
public class GameDataSources : AsyncDataSources
{
    // Configure in inspector:
    // - Multiple ScriptableObject sources
    // - Addressable async sources
    // - Timeout settings
    // - Per-source enable/disable
    // - Awaitable loading control
}

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AsyncDataSources dataSources;
    
    private async void Start()
    {
        var gameContext = new EntityContext();
        
        // Load all data sources asynchronously
        await dataSources.RegisterAsync(gameContext);
        
        Debug.Log("All game data loaded!");
    }
}
```

### ValueContainerDataSource

Reactive data container with automatic context binding:

```csharp
[CreateAssetMenu(menuName = "Game/Sources/Player Data")]
public class PlayerDataSource : ValueContainerDataSource<PlayerData>
{
    // Automatically publishes value changes to bound contexts
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private PlayerDataSource playerDataSource;
    
    private async void Start()
    {
        var context = this.GetSceneContext();
        
        // Register data source to context
        await playerDataSource.RegisterAsync(context);
        
        // Update player data
        var playerData = new PlayerData { Level = 5, Experience = 1200 };
        playerDataSource.SetValue(playerData);
        
        // All subscribers automatically receive updates
    }
}
```

### Custom Async Sources

```csharp
public class ConfigurationLoader : AsyncSource
{
    [SerializeField] private AssetReference configReference;
    [SerializeField] private string configUrl;
    
    public override async UniTask<IContext> RegisterAsync(IContext context)
    {
        // Load from addressables
        var configAsset = await configReference.LoadAssetTaskAsync(LifeTime);
        var localConfig = JsonUtility.FromJson<GameConfig>(configAsset.text);
        
        // Load from remote
        var remoteConfig = await LoadRemoteConfig(configUrl);
        
        // Merge and publish
        var finalConfig = MergeConfigs(localConfig, remoteConfig);
        context.Publish(finalConfig);
        
        return context;
    }
    
    private async UniTask<GameConfig> LoadRemoteConfig(string url)
    {
        // Implementation for remote loading
        await UniTask.Delay(1000); // Simulate network delay
        return new GameConfig();
    }
}
```

## 🛠️ Service Management

### ServiceDataSourceAsset

Advanced service management with sharing, lifecycle control, and profiling:

```csharp
public interface IPlayerService : IGameService
{
    int Level { get; }
    void AddExperience(int amount);
    IObservable<int> LevelChanged { get; }
}

[CreateAssetMenu(menuName = "Game/Services/Player Service")]
public class PlayerServiceAsset : ServiceDataSourceAsset<IPlayerService>
{
    protected override async UniTask<IPlayerService> CreateServiceInternalAsync(IContext context)
    {
        // Create service instance
        var service = new PlayerService();
        
        // Initialize with context data
        var playerData = await context.ReceiveFirstAsync<PlayerData>();
        service.Initialize(playerData);
        
        return service;
    }
}

public class PlayerService : IPlayerService
{
    private ReactiveProperty<int> _level = new();
    
    public int Level => _level.Value;
    public IObservable<int> LevelChanged => _level;
    
    public void Initialize(PlayerData data)
    {
        _level.Value = data.Level;
    }
    
    public void AddExperience(int amount)
    {
        // Experience logic
        var newLevel = CalculateLevel(amount);
        _level.Value = newLevel;
    }
    
    public void Dispose()
    {
        _level?.Dispose();
    }
}
```

### Shared Services

```csharp
[CreateAssetMenu(menuName = "Game/Services/Audio Service")]
public class AudioServiceAsset : ServiceDataSourceAsset<IAudioService>
{
    // Enable sharing in inspector: isSharedSystem = true
    // Service will be created once and shared across all contexts
    
    protected override async UniTask<IAudioService> CreateServiceInternalAsync(IContext context)
    {
        var audioService = new AudioService();
        await audioService.InitializeAsync();
        return audioService;
    }
}
```

## ⚡ Reactive Programming

### Observable Values

```csharp
public class ReactivePlayerStats : MonoBehaviour
{
    private ReactiveValue<int> _score = new();
    private ReactiveValue<float> _health = new();
    
    private void Start()
    {
        // Publish reactive values to context
        var context = this.GetSceneContext();
        context.Publish<IObservable<int>>(_score);
        context.Publish<IObservable<float>>(_health);
        
        // Update values
        _score.Value = 100;
        _health.Value = 80f;
    }
    
    public void AddScore(int points) => _score.Value += points;
    public void TakeDamage(float damage) => _health.Value -= damage;
}

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    
    private void Start()
    {
        var context = this.GetSceneContext();
        
        // Subscribe to score changes
        context.Get<IObservable<int>>()
            .Subscribe(score => scoreText.text = $"Score: {score}")
            .AddTo(this);
    }
}
```

### Async Context Extensions

```csharp
public class AsyncContextExample : MonoBehaviour
{
    private async void Start()
    {
        var context = this.GetSceneContext();
        
        // Wait for specific data to become available
        var playerData = await context.ReceiveFirstAsync<PlayerData>();
        Debug.Log($"Player data received: {playerData.Name}");
        
        // Wait for component from scene
        var audioSource = await context.ReceiveFirstFromSceneAsync<AudioSource>();
        audioSource.Play();
    }
}
```

## 💡 Best Practices

### 1. Lifecycle Management

```csharp
// ✅ Good - Always use LifeTime
public class ServiceManager : MonoBehaviour
{
    private LifeTimeDefinition _lifeTime = new();
    
    private void Start()
    {
        var context = new EntityContext();
        context.AddTo(_lifeTime);
        
        // All subscriptions will be cleaned up automatically
        context.Receive<GameEvent>()
            .Subscribe(HandleEvent)
            .AddTo(_lifeTime);
    }
    
    private void OnDestroy() => _lifeTime.Terminate();
}

// ❌ Bad - Memory leaks possible
public class BadServiceManager : MonoBehaviour
{
    private void Start()
    {
        var context = new EntityContext();
        context.Receive<GameEvent>().Subscribe(HandleEvent); // No cleanup!
    }
}
```

### 2. Interface-based Publishing

```csharp
// ✅ Good - Use interfaces for decoupling
context.Publish<IPlayerService>(new PlayerService());
context.Publish<IInventorySystem>(new InventorySystem());

var playerService = context.Get<IPlayerService>();

// ❌ Bad - Tight coupling to concrete types
context.Publish(new PlayerService());
var playerService = context.Get<PlayerService>();
```

### 3. Service Management

```csharp
// ✅ Good - Use ServiceDataSourceAsset for services
[CreateAssetMenu(menuName = "Game/Services/Player Service")]
public class PlayerServiceAsset : ServiceDataSourceAsset<IPlayerService>
{
    // Automatic shared instance management
    // Lifecycle control
    // Performance profiling
    // Proper error handling
}

// ❌ Bad - Manual service creation
public class BadServiceManager : MonoBehaviour
{
    private static IPlayerService _playerService; // Manual singleton!
    
    private void Start()
    {
        if (_playerService == null)
        {
            _playerService = new PlayerService(); // No lifecycle management!
        }
    }
}
```

## 📚 Examples

### Complete Game Bootstrap

```csharp
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AsyncDataSources gameDataSources;
    [SerializeField] private List<ServiceDataSourceAsset> coreServices;
    
    private async void Start()
    {
        // Initialize global context
        var gameContext = new EntityContext();
        GameContext.Context = gameContext;
        
        // Load core services
        await LoadServices(gameContext);
        
        // Load game data
        await gameDataSources.RegisterAsync(gameContext);
        
        // Connect to scene contexts
        ConnectSceneContexts(gameContext);
        
        Debug.Log("Game Bootstrap Complete!");
    }
    
    private async UniTask LoadServices(IContext context)
    {
        var serviceTasks = coreServices.Select(service => service.RegisterAsync(context));
        await UniTask.WhenAll(serviceTasks);
        
        Debug.Log($"Loaded {coreServices.Count} core services");
    }
    
    private void ConnectSceneContexts(IContext gameContext)
    {
        // Connect game context to all scene contexts
        this.NotifyOnAllSceneContext()
            .Subscribe(sceneContext => {
                var connection = gameContext.Merge(sceneContext);
                sceneContext.LifeTime.AddDispose(connection);
            })
            .AddTo(this);
    }
}
```

### Advanced Service System

```csharp
// Service Interface
public interface IInventoryService : IGameService
{
    IObservable<InventoryData> InventoryChanged { get; }
    bool AddItem(ItemData item);
    bool RemoveItem(string itemId, int quantity);
    ItemData GetItem(string itemId);
    UniTask SaveAsync();
}

// Service Implementation
public class InventoryService : IInventoryService
{
    private ReactiveProperty<InventoryData> _inventory = new();
    private ISaveService _saveService;
    
    public IObservable<InventoryData> InventoryChanged => _inventory;
    
    public void Initialize(ISaveService saveService, InventoryData initialData)
    {
        _saveService = saveService;
        _inventory.Value = initialData;
    }
    
    public bool AddItem(ItemData item)
    {
        var current = _inventory.Value;
        if (current.Items.Count >= current.MaxSlots) return false;
        
        current.Items.Add(item);
        _inventory.SetValueAndForceNotify(current);
        return true;
    }
    
    public async UniTask SaveAsync()
    {
        await _saveService.SaveDataAsync("inventory", _inventory.Value);
    }
    
    public void Dispose()
    {
        _inventory?.Dispose();
    }
}

// Service Asset
[CreateAssetMenu(menuName = "Game/Services/Inventory Service")]
public class InventoryServiceAsset : ServiceDataSourceAsset<IInventoryService>
{
    [SerializeField] private InventoryData defaultInventory;
    
    protected override async UniTask<IInventoryService> CreateServiceInternalAsync(IContext context)
    {
        // Wait for dependencies
        var saveService = await context.ReceiveFirstAsync<ISaveService>();
        
        // Load saved data or use default
        var inventoryData = await saveService.LoadDataAsync<InventoryData>("inventory") 
                           ?? defaultInventory;
        
        // Create and initialize service
        var service = new InventoryService();
        service.Initialize(saveService, inventoryData);
        
        return service;
    }
}
```

## 📄 License

MIT License - see LICENSE file for details.

## 🤝 Support

For questions and suggestions, contact the UniGame team.

## 🔗 Related Modules

- **UniGame.Core** - Core interfaces and utilities
- **UniGame.AddressableTools** - Addressable assets integration
- **UniGame.RX** - Reactive programming extensions
- **UniGame.UIToolkit** - UI system integration 