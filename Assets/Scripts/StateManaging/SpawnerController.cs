using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnerController : MonoBehaviour
{
    [Header("Configuration")]
    public MinigameConfig config;
    public SpawnTable spawnTable;
    
    [Header("Spawn Positions")]
    public Transform topSpawner;
    public Transform bottomSpawner;
    
    [Header("Spawn Area")]
    public float spawnWidth = 20f;
    public float spawnMargin = 0.5f;
    
    // Object pools
    private Dictionary<GameObject, Queue<Item>> hazardPools = new Dictionary<GameObject, Queue<Item>>();
    private Dictionary<GameObject, Queue<Item>> goodPools = new Dictionary<GameObject, Queue<Item>>();
    
    // Active items tracking
    private List<Item> activeItems = new List<Item>();
    
    // Spawning state
    private float hazardSpawnAccumulator = 0f;
    private float goodSpawnAccumulator = 0f;
    private bool spawnFromTop = true;
    
    // References
    private GravityFlipController gravityController;
    private Camera mainCamera;
    
    // Burst wave state
    private bool inBurstWave = false;
    private float nextBurstTime = 0f;
    
    // Events
    public System.Action<Item, GravityFlipPlayerController> OnItemCollectedEvent;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        gravityController = FindFirstObjectByType<GravityFlipController>();
        mainCamera = Camera.main;

        // Set up camera-based spawn width if not manually set
        if (spawnWidth <= 0f && mainCamera != null)
        {
            // Expand spawn width beyond camera bounds to cover full scene
            spawnWidth = mainCamera.orthographicSize * mainCamera.aspect * 2f;
            Debug.Log(spawnWidth);

        }
        
        // Subscribe to gravity flip events
        if (gravityController != null)
        {
            gravityController.OnGravityFlipped += OnGravityFlipped;
        }
        
        // Initialize spawn positions if not set
        SetupSpawnPositions();
        
        // Initialize pools
        InitializePools();
        
        // Set up burst timing
        if (config.useBurstWaves)
        {
            nextBurstTime = Time.time + config.burstInterval;
        }
    }
    
    void SetupSpawnPositions()
    {
        if (mainCamera == null) return;
        
        float cameraHeight = mainCamera.orthographicSize;
        float spawnY = cameraHeight + 1f; // Above/below camera view
        
        if (topSpawner == null)
        {
            GameObject topSpawnerObj = new GameObject("TopSpawner");
            topSpawnerObj.transform.SetParent(transform);
            topSpawnerObj.transform.position = new Vector3(0, spawnY, 0);
            topSpawner = topSpawnerObj.transform;
        }
        
        if (bottomSpawner == null)
        {
            GameObject bottomSpawnerObj = new GameObject("BottomSpawner");
            bottomSpawnerObj.transform.SetParent(transform);
            bottomSpawnerObj.transform.position = new Vector3(0, -spawnY, 0);
            bottomSpawner = bottomSpawnerObj.transform;
        }
    }
    
    void InitializePools()
    {
        if (spawnTable == null) return;
        
        // Initialize hazard pools
        foreach (var hazard in spawnTable.hazards)
        {
            if (hazard.prefab != null)
            {
                hazardPools[hazard.prefab] = new Queue<Item>();
                PrewarmPool(hazard.prefab, hazardPools[hazard.prefab], 5);
            }
        }
        
        // Initialize good pools
        foreach (var good in spawnTable.goods)
        {
            if (good.prefab != null)
            {
                goodPools[good.prefab] = new Queue<Item>();
                PrewarmPool(good.prefab, goodPools[good.prefab], 5);
            }
        }
    }
    
    void PrewarmPool(GameObject prefab, Queue<Item> pool, int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            Item item = obj.GetComponent<Item>();
            if (item == null)
            {
                item = obj.AddComponent<Item>();
            }
            obj.SetActive(false);
            pool.Enqueue(item);
        }
    }
    
    void Update()
    {
        UpdateSpawning();
        UpdateBurstWaves();
    }
    
    void UpdateSpawning()
    {
        float deltaTime = Time.deltaTime;
        
        // Calculate difficulty-ramped spawn rates
        float gameProgress = Time.time / config.durationSeconds;
        float rampMultiplier = 1f + config.spawnRateRampFactor * gameProgress;
        
        float currentHazardRate = config.hazardSpawnRate * rampMultiplier;
        float currentGoodRate = config.goodSpawnRate * rampMultiplier;
        
        // Apply burst multiplier if in burst wave
        if (inBurstWave)
        {
            currentHazardRate *= config.burstRateMultiplier;
            currentGoodRate *= config.burstRateMultiplier;
        }
        
        // Poisson process spawning
        hazardSpawnAccumulator += currentHazardRate * deltaTime;
        goodSpawnAccumulator += currentGoodRate * deltaTime;
        
        // Spawn hazards
        while (hazardSpawnAccumulator >= 1f && activeItems.Count < config.maxSimultaneous)
        {
            SpawnHazard();
            hazardSpawnAccumulator -= 1f;
        }
        
        // Spawn goods
        while (goodSpawnAccumulator >= 1f && activeItems.Count < config.maxSimultaneous)
        {
            SpawnGood();
            goodSpawnAccumulator -= 1f;
        }
    }
    
    void UpdateBurstWaves()
    {
        if (!config.useBurstWaves) return;
        
        // Check for burst start
        if (!inBurstWave && Time.time >= nextBurstTime)
        {
            StartCoroutine(BurstWave());
        }
    }
    
    IEnumerator BurstWave()
    {
        inBurstWave = true;
        yield return new WaitForSeconds(config.burstDuration);
        inBurstWave = false;
        
        // Schedule next burst
        nextBurstTime = Time.time + config.burstInterval;
    }
    
    void SpawnHazard()
    {
        if (spawnTable?.hazards == null || spawnTable.hazards.Length == 0) return;
        
        SpawnItem hazardData = spawnTable.GetRandomHazard();
        if (hazardData?.prefab == null) return;
        
        Item item = GetPooledItem(hazardData.prefab, hazardPools);
        if (item != null)
        {
            SetupSpawnedItem(item, hazardData, ItemType.Hazard);
        }
    }
    
    void SpawnGood()
    {
        if (spawnTable?.goods == null || spawnTable.goods.Length == 0) return;
        
        SpawnItem goodData = spawnTable.GetRandomGood();
        if (goodData?.prefab == null) return;
        
        Item item = GetPooledItem(goodData.prefab, goodPools);
        if (item != null)
        {
            SetupSpawnedItem(item, goodData, ItemType.Good);
        }
    }
    
    Item GetPooledItem(GameObject prefab, Dictionary<GameObject, Queue<Item>> pools)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<Item>();
        }
        
        Queue<Item> pool = pools[prefab];
        
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            // Create new item if pool is empty
            GameObject obj = Instantiate(prefab, transform);
            Item item = obj.GetComponent<Item>();
            if (item == null)
            {
                item = obj.AddComponent<Item>();
            }
            return item;
        }
    }
    
    void SetupSpawnedItem(Item item, SpawnItem spawnData, ItemType itemType)
    {
        // Choose spawn position based on current gravity
        Transform spawner = spawnFromTop ? topSpawner : bottomSpawner;
        
        // Random X position across spawn width
        float randomX = Random.Range(-spawnWidth / 2f + spawnMargin, spawnWidth / 2f - spawnMargin);
        Vector3 spawnPos = spawner.position + new Vector3(randomX, 0, 0);
        
        // Calculate speed with difficulty ramp
        float gameProgress = Time.time / config.durationSeconds;
        float speedMultiplier = 1f + config.speedRampFactor * gameProgress;
        float speed = Random.Range(spawnData.speedMin, spawnData.speedMax) * speedMultiplier;
        
        // Calculate size
        float size = Random.Range(spawnData.sizeMin, spawnData.sizeMax);
        
        // Get movement direction
        Vector2 direction = gravityController != null ? gravityController.GetSpawnDirection() : Vector2.down;
        
        // Setup item
        item.transform.position = spawnPos;
        item.Initialize(itemType, direction, speed, size);
        item.gameObject.SetActive(true);
        
        // Track active item
        activeItems.Add(item);
    }
    
    void OnGravityFlipped(bool isGravityDown)
    {
        spawnFromTop = isGravityDown; // When gravity is down, spawn from top (and vice versa)
    }
    
    public void OnItemCollected(Item item, GravityFlipPlayerController player)
    {
        OnItemCollectedEvent?.Invoke(item, player);
        
        // Remove from active tracking
        activeItems.Remove(item);
    }
    
    public void ReturnItemToPool(Item item)
    {
        // Remove from active tracking
        activeItems.Remove(item);
        
        // Reset item state
        item.ResetForPool();
        item.gameObject.SetActive(false);
        
        // Return to appropriate pool
        GameObject prefab = GetOriginalPrefab(item);
        if (prefab != null)
        {
            if (item.itemType == ItemType.Hazard && hazardPools.ContainsKey(prefab))
            {
                hazardPools[prefab].Enqueue(item);
            }
            else if (item.itemType == ItemType.Good && goodPools.ContainsKey(prefab))
            {
                goodPools[prefab].Enqueue(item);
            }
        }
    }
    
    GameObject GetOriginalPrefab(Item item)
    {
        // Try to find matching prefab - this is a simple implementation
        // In a more complex system, you might store the original prefab reference on the item
        
        if (spawnTable == null) return null;
        
        // Check hazards
        foreach (var hazard in spawnTable.hazards)
        {
            if (hazard.prefab != null && hazard.prefab.name == item.name.Replace("(Clone)", ""))
            {
                return hazard.prefab;
            }
        }
        
        // Check goods
        foreach (var good in spawnTable.goods)
        {
            if (good.prefab != null && good.prefab.name == item.name.Replace("(Clone)", ""))
            {
                return good.prefab;
            }
        }
        
        return null;
    }
    
    public void ClearAllItems()
    {
        // Return all active items to pools
        while (activeItems.Count > 0)
        {
            ReturnItemToPool(activeItems[0]);
        }
    }
    
    public void PauseSpawning()
    {
        enabled = false;
    }
    
    public void ResumeSpawning()
    {
        enabled = true;
    }
}