using UnityEngine;

public class GemPickup : MonoBehaviour
{
    private GemSpawner owner;
    private Transform player;
    private float collectionDistanceSquared;
    private float rotationSpeed;
    private float hoverHeight;
    private float hoverSpeed;
    private float baseHeight;
    private float animationOffset;
    private bool isCollected;

    public int SpawnPointIndex { get; private set; }

    public void Initialize(
        GemSpawner gemOwner,
        Transform playerTransform,
        int spawnPointIndex,
        float collectionDistance,
        float gemRotationSpeed,
        float gemHoverHeight,
        float gemHoverSpeed)
    {
        owner = gemOwner;
        player = playerTransform;
        SpawnPointIndex = spawnPointIndex;
        collectionDistanceSquared = collectionDistance * collectionDistance;
        rotationSpeed = gemRotationSpeed;
        hoverHeight = gemHoverHeight;
        hoverSpeed = gemHoverSpeed;
        baseHeight = transform.position.y;
        animationOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        Vector3 position = transform.position;
        position.y = baseHeight + Mathf.Sin(Time.time * hoverSpeed + animationOffset) * hoverHeight;
        transform.position = position;

        if (isCollected || player == null)
        {
            return;
        }

        if ((player.position - transform.position).sqrMagnitude > collectionDistanceSquared)
        {
            return;
        }

        isCollected = true;
        owner.Collect(this);
    }
}
