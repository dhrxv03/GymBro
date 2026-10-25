using UnityEngine;

public class CherryController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private GameController game;
    public GameObject Bonus => bonus;
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
        // Count the intro towards the first five-second spawn delay, without moving during it.
        if (game.State == RoundState.Countdown) waitTime += Time.deltaTime;
        else if (game.IsPlaying) AdvanceBonus(Time.deltaTime);
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

        Vector3 before = bonus.transform.position;
        elapsed += deltaTime;
        bonus.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
        game.BonusMoved(before, bonus.transform.position);
        if (bonus == null) return;
        if (elapsed >= duration)
        {
            Destroy(bonus);
            bonus = null;
            waitTime = 0f;
            hadBonus = false;
        }
    }

    public void CollectBonus()
    {
        if (bonus == null) return;
        Destroy(bonus);
        bonus = null;
        hadBonus = false;
        waitTime = 0f;
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
