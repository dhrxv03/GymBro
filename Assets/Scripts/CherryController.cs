using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private GameObject bonusPrefab;
    [SerializeField] private float spawnDelay = 5f;
    [SerializeField] private float moveSpeed = 3f;

    private GameObject bonus;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float elapsed;
    private float duration;
    private float waitTime;
    private bool hadBonus;
    private float previousAngle = -1f;

    private void Update()
    {
        AdvanceBonus(Time.deltaTime);
    }

    private void AdvanceBonus(float deltaTime)
    {
        if (bonus == null)
        {
            // Also starts a fresh wait when the player collects the item later.
            if (hadBonus) { waitTime = 0f; hadBonus = false; }
            waitTime += deltaTime;
            if (waitTime >= spawnDelay) SpawnBonus();
            return;
        }

        elapsed += deltaTime;
        bonus.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
        if (elapsed >= duration)
        {
            Destroy(bonus);
            bonus = null;
            waitTime = 0f;
            hadBonus = false;
        }
    }

    private void SpawnBonus()
    {
        float angle = Random.Range(0f, 360f);
        // Avoid repeating almost the same starting direction on successive trips.
        if (previousAngle >= 0f && Mathf.Abs(Mathf.DeltaAngle(previousAngle, angle)) < 15f)
            angle = (angle + 45f) % 360f;
        previousAngle = angle;
        Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f);
        float x = Mathf.Abs(direction.x), y = Mathf.Abs(direction.y);
        float mapDistance = Mathf.Min(x > .0001f ? (level.Columns / 2f + 1f) / x : float.MaxValue,
            y > .0001f ? (level.Rows / 2f + 1f) / y : float.MaxValue);
        startPosition = direction * mapDistance;

        // Finish beyond both the map and the camera, allowing for sprite size.
        float cameraHeight = gameCamera.orthographicSize;
        float cameraWidth = cameraHeight * gameCamera.aspect;
        float cameraDistance = Mathf.Min(x > .0001f ? (cameraWidth + 1f) / x : float.MaxValue,
            y > .0001f ? (cameraHeight + 1f) / y : float.MaxValue);
        endPosition = -direction * Mathf.Max(mapDistance, cameraDistance);
        elapsed = 0f;
        duration = Vector3.Distance(startPosition, endPosition) / moveSpeed;
        bonus = Instantiate(bonusPrefab, startPosition, Quaternion.identity);
        hadBonus = true;
        waitTime = 0f;
    }
}
