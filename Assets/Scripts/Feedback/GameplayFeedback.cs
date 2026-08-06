using System.Collections;
using UnityEngine;

public class GameplayFeedback : MonoBehaviour
{
    private const string PlayerColorProperty = "_BaseColor";

    [Header("References")]
    [SerializeField] private SwipeMovement playerMovement;
    [SerializeField] private GemSpawner gemSpawner;
    [SerializeField] private MeshRenderer playerRenderer;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Light sceneLight;

    [Header("Wall Hit")]
    [SerializeField] private float wallShakeDuration = 0.12f;
    [SerializeField] private float wallShakeAmount = 0.1f;
    [SerializeField] private Color wallFlashColor = new Color(1f, 0.35f, 0.2f);

    [Header("Gem Collected")]
    [SerializeField] private Color gemFlashColor = new Color(0.1f, 1f, 0.8f);
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float lightIntensityMultiplier = 1.6f;

    private Material playerMaterial;
    private Vector3 cameraStartPosition;
    private Color playerStartColor;
    private Color lightStartColor;
    private float lightStartIntensity;

    private Coroutine cameraShakeRoutine;
    private Coroutine playerFlashRoutine;
    private Coroutine lightFlashRoutine;

    private void Awake()
    {
        playerMaterial = playerRenderer.material;
        playerStartColor = playerMaterial.GetColor(PlayerColorProperty);

        cameraStartPosition = gameplayCamera.transform.localPosition;

        lightStartColor = sceneLight.color;
        lightStartIntensity = sceneLight.intensity;
    }

    private void OnEnable()
    {
        playerMovement.WallHit += OnWallHit;
        gemSpawner.GemCollected += OnGemCollected;
    }

    private void OnDisable()
    {
        playerMovement.WallHit -= OnWallHit;
        gemSpawner.GemCollected -= OnGemCollected;

        StopAllCoroutines();
        cameraShakeRoutine = null;
        playerFlashRoutine = null;
        lightFlashRoutine = null;
        RestoreVisuals();
    }

    private void OnWallHit()
    {
        RestartCoroutine(ref cameraShakeRoutine, ShakeCamera());
        RestartCoroutine(ref playerFlashRoutine, FlashPlayer(wallFlashColor));
    }

    private void OnGemCollected()
    {
        RestartCoroutine(ref playerFlashRoutine, FlashPlayer(gemFlashColor));
        RestartCoroutine(ref lightFlashRoutine, FlashLight());
    }

    private void RestartCoroutine(ref Coroutine currentRoutine, IEnumerator newRoutine)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(newRoutine);
    }

    private IEnumerator ShakeCamera()
    {
        float elapsedTime = 0f;

        while (elapsedTime < wallShakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * wallShakeAmount;
            gameplayCamera.transform.localPosition =
                cameraStartPosition + new Vector3(offset.x, 0f, offset.y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameplayCamera.transform.localPosition = cameraStartPosition;
        cameraShakeRoutine = null;
    }

    private IEnumerator FlashPlayer(Color flashColor)
    {
        float elapsedTime = 0f;
        playerMaterial.SetColor(PlayerColorProperty, flashColor);

        while (elapsedTime < flashDuration)
        {
            float progress = elapsedTime / flashDuration;
            playerMaterial.SetColor(
                PlayerColorProperty,
                Color.Lerp(flashColor, playerStartColor, progress));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerMaterial.SetColor(PlayerColorProperty, playerStartColor);
        playerFlashRoutine = null;
    }

    private IEnumerator FlashLight()
    {
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            float progress = elapsedTime / flashDuration;
            sceneLight.color = Color.Lerp(gemFlashColor, lightStartColor, progress);
            sceneLight.intensity = Mathf.Lerp(
                lightStartIntensity * lightIntensityMultiplier,
                lightStartIntensity,
                progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        sceneLight.color = lightStartColor;
        sceneLight.intensity = lightStartIntensity;
        lightFlashRoutine = null;
    }

    private void RestoreVisuals()
    {
        if (playerMaterial != null)
        {
            playerMaterial.SetColor(PlayerColorProperty, playerStartColor);
        }

        if (gameplayCamera != null)
        {
            gameplayCamera.transform.localPosition = cameraStartPosition;
        }

        if (sceneLight != null)
        {
            sceneLight.color = lightStartColor;
            sceneLight.intensity = lightStartIntensity;
        }
    }

    private void OnDestroy()
    {
        if (playerMaterial != null)
        {
            Destroy(playerMaterial);
        }
    }
}
