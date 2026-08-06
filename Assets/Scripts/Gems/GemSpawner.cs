using System.Collections.Generic;
using UnityEngine;

public class GemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform player;
    [SerializeField] private GemPickup gemPrefab;

    [Header("Spawn Points")]
    [SerializeField] private List<Vector3> spawnPoints = new List<Vector3>();
    [SerializeField, Min(1)] private int maxActiveGems = 5;
    [SerializeField, Min(0f)] private float respawnDelay = 0.5f;
    [SerializeField, Min(0f)] private float minimumPlayerDistance = 1.25f;

    [Header("Gem")]
    [SerializeField, Min(1)] private int pointsPerGem = 1;
    [SerializeField] private Color gemColor = new Color(0.1f, 1f, 0.8f, 1f);
    [SerializeField] private Vector3 gemScale = new Vector3(0.35f, 0.45f, 0.35f);
    [SerializeField, Min(0.01f)] private float collectionDistance = 0.75f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField, Min(0f)] private float hoverHeight = 0.12f;
    [SerializeField, Min(0f)] private float hoverSpeed = 2f;

    private readonly List<GemPickup> activeGems = new List<GemPickup>();
    private readonly HashSet<int> occupiedSpawnPoints = new HashSet<int>();
    private readonly List<int> availableSpawnPoints = new List<int>();

    private Material runtimeGemMaterial;
    private float nextSpawnTime;
    private bool isSpawning = true;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (player == null)
        {
            SwipeMovement playerMovement = FindFirstObjectByType<SwipeMovement>();
            if (playerMovement != null)
            {
                player = playerMovement.transform;
            }
        }

        if (gameManager != null)
        {
            gameManager.EndGame += StopSpawning;
        }
    }

    private void Start()
    {
        if (player == null || spawnPoints.Count == 0)
        {
            Debug.LogError("GemSpawner requires a Player and at least one spawn point.", this);
            enabled = false;
            return;
        }

        if (gemPrefab == null)
        {
            CreateRuntimeGemMaterial();
        }

        SpawnUntilFull();
    }

    private void Update()
    {
        if (!isSpawning || activeGems.Count >= maxActiveGems || Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnOneGem();
    }

    public void Collect(GemPickup gem)
    {
        if (gem == null || !activeGems.Remove(gem))
        {
            return;
        }

        occupiedSpawnPoints.Remove(gem.SpawnPointIndex);

        if (gameManager != null)
        {
            gameManager.AddScore(pointsPerGem);
        }
        else
        {
            GameSessionData.GetOrCreate().AddScore(pointsPerGem);
        }

        Destroy(gem.gameObject);
        nextSpawnTime = Time.time + respawnDelay;
    }

    private void SpawnUntilFull()
    {
        while (activeGems.Count < maxActiveGems && SpawnOneGem())
        {
        }
    }

    private bool SpawnOneGem()
    {
        BuildAvailableSpawnPointList();
        if (availableSpawnPoints.Count == 0)
        {
            return false;
        }

        int randomListIndex = Random.Range(0, availableSpawnPoints.Count);
        int spawnPointIndex = availableSpawnPoints[randomListIndex];
        Vector3 spawnPosition = transform.TransformPoint(spawnPoints[spawnPointIndex]);

        GemPickup gem = gemPrefab != null
            ? Instantiate(gemPrefab, spawnPosition, Quaternion.identity, transform)
            : CreateRuntimeGem(spawnPosition);

        gem.Initialize(
            this,
            player,
            spawnPointIndex,
            collectionDistance,
            rotationSpeed,
            hoverHeight,
            hoverSpeed);

        activeGems.Add(gem);
        occupiedSpawnPoints.Add(spawnPointIndex);
        return true;
    }

    private void BuildAvailableSpawnPointList()
    {
        availableSpawnPoints.Clear();
        float minimumDistanceSquared = minimumPlayerDistance * minimumPlayerDistance;

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (occupiedSpawnPoints.Contains(i))
            {
                continue;
            }

            Vector3 worldPosition = transform.TransformPoint(spawnPoints[i]);
            if ((player.position - worldPosition).sqrMagnitude < minimumDistanceSquared)
            {
                continue;
            }

            availableSpawnPoints.Add(i);
        }
    }

    private GemPickup CreateRuntimeGem(Vector3 position)
    {
        GameObject gemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gemObject.name = "Gem";
        gemObject.transform.SetParent(transform);
        gemObject.transform.SetPositionAndRotation(position, Quaternion.Euler(45f, 45f, 0f));
        gemObject.transform.localScale = gemScale;

        Collider gemCollider = gemObject.GetComponent<Collider>();
        if (gemCollider != null)
        {
            Destroy(gemCollider);
        }

        MeshRenderer meshRenderer = gemObject.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = runtimeGemMaterial;

        return gemObject.AddComponent<GemPickup>();
    }

    private void CreateRuntimeGemMaterial()
    {
        Shader gemShader = Shader.Find("Universal Render Pipeline/Lit");
        if (gemShader == null)
        {
            gemShader = Shader.Find("Standard");
        }

        runtimeGemMaterial = new Material(gemShader) { name = "Runtime Gem Material" };
        runtimeGemMaterial.SetColor("_BaseColor", gemColor);
    }

    private void StopSpawning()
    {
        isSpawning = false;
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.EndGame -= StopSpawning;
        }

        if (runtimeGemMaterial != null)
        {
            Destroy(runtimeGemMaterial);
        }
    }

    private void OnValidate()
    {
        maxActiveGems = Mathf.Max(1, maxActiveGems);
        pointsPerGem = Mathf.Max(1, pointsPerGem);
        collectionDistance = Mathf.Max(0.01f, collectionDistance);
        gemScale = new Vector3(
            Mathf.Max(0.01f, gemScale.x),
            Mathf.Max(0.01f, gemScale.y),
            Mathf.Max(0.01f, gemScale.z));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gemColor;
        float gizmoSize = Mathf.Max(0.15f, gemScale.x);

        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(spawnPoints[i]), gizmoSize);
        }
    }
}
