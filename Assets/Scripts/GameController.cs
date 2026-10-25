using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum RoundState { Countdown, Playing, Dying, GameOver }

public class GameController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private PacStudentController player;
    [SerializeField] private CherryController cherry;
    [SerializeField] private GhostStateController[] ghosts;
    [SerializeField] private HUDController hud;
    [SerializeField] private GameAudioController audioController;
    [SerializeField] private ParticleSystem wallEffect;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private WorkoutController workout;
    private int score;
    private int lives = 3;
    private float gameTime;
    private float scaredTime;
    public RoundState State { get; private set; } = RoundState.Countdown;
    public float ScaredTime => scaredTime;
    public bool IsPlaying => State == RoundState.Playing;

    private IEnumerator Start()
    {
        hud.SetScore(0); hud.SetTime(0); hud.SetLives(lives); hud.SetScaredTime(0);
        foreach (var ghost in ghosts) { ghost.ResetGhost(); ghost.Freeze(); }
        audioController.PlayIntro();
        string[] messages = { "3", "2", "1", "GO!" };
        foreach (string message in messages)
        {
            hud.ShowMessage(message);
            yield return new WaitForSeconds(1f);
        }
        hud.HideMessage();
        State = RoundState.Playing;
        foreach (var ghost in ghosts) ghost.ResetGhost();
        RefreshMusic();
    }

    private void Update()
    {
        if (!IsPlaying) return;
        TickTimers(Time.deltaTime);
        PlayerMoved(player.transform.position, player.transform.position);
    }

    private void TickTimers(float deltaTime)
    {
        gameTime += deltaTime;
        scaredTime = Mathf.Max(0f, scaredTime - deltaTime);
        foreach (var ghost in ghosts) ghost.Tick(deltaTime, scaredTime);
        hud.SetTime(gameTime); hud.SetScaredTime(scaredTime);
        RefreshMusic();
    }

    private void RefreshMusic()
    {
        bool anyDead = false;
        foreach (var ghost in ghosts) if (ghost.Mode == GhostMode.Dead) anyDead = true;
        audioController.UpdateMusic(anyDead, scaredTime > 0f);
    }

    private void CollectCell(Vector2Int cell)
    {
        int tile = level.Collect(cell);
        if (tile == 0) return;
        score += tile == 6 ? 50 : 10;
        if (workout != null) workout.GainEnergy();
        if (tile == 6)
        {
            scaredTime = 10f;
            foreach (var ghost in ghosts) ghost.SetGlobalMode(scaredTime);
            hud.SetScaredTime(scaredTime);
            RefreshMusic();
        }
        hud.SetScore(score);
        if (workout == null && level.RemainingPellets == 0) EndGame();
    }

    // Check the travelled segment, so a long frame cannot skip a pickup or ghost.
    public void PlayerMoved(Vector3 from, Vector3 to)
    {
        if (!IsPlaying) return;
        Vector2Int first = level.WorldToGrid(from), last = level.WorldToGrid(to);
        if (NearSegment(level.GridToWorld(first), from, to, .45f)) CollectCell(first);
        if (!IsPlaying) return;
        if (last != first && NearSegment(level.GridToWorld(last), from, to, .45f)) CollectCell(last);
        if (!IsPlaying) return;
        if (cherry.Bonus != null && NearSegment(cherry.Bonus.transform.position, from, to, .5f)) CollectBonus();
        foreach (var ghost in ghosts)
        {
            if (ghost.Mode == GhostMode.Dead || !NearSegment(ghost.transform.position, from, to, .45f)) continue;
            if (ghost.Mode == GhostMode.Normal)
            {
                if (workout == null || !workout.IsDashing) { StartCoroutine(PlayerDeath()); return; }
                continue;
            }
            ghost.Eaten(); score += 300; hud.SetScore(score); audioController.PlayEnemy(); RefreshMusic();
        }
    }

    public void GhostReturned() { RefreshMusic(); }
    public void GhostMoved(GhostStateController ghost, Vector3 from, Vector3 to)
    {
        if (!IsPlaying || ghost.Mode == GhostMode.Dead || !NearSegment(player.transform.position, from, to, .45f)) return;
        if (ghost.Mode == GhostMode.Normal)
        {
            if (workout == null || !workout.IsDashing) StartCoroutine(PlayerDeath());
        }
        else { ghost.Eaten(); score += 300; hud.SetScore(score); audioController.PlayEnemy(); RefreshMusic(); }
    }

    public void BonusMoved(Vector3 from, Vector3 to)
    {
        if (IsPlaying && cherry.Bonus != null && NearSegment(player.transform.position, from, to, .5f)) CollectBonus();
    }
    private void CollectBonus()
    {
        cherry.CollectBonus(); score += 100; hud.SetScore(score); audioController.PlayBonus();
    }
    private static bool NearSegment(Vector3 point, Vector3 from, Vector3 to, float radius)
    {
        Vector3 segment = to - from;
        float t = segment.sqrMagnitude > 0f ? Mathf.Clamp01(Vector3.Dot(point - from, segment) / segment.sqrMagnitude) : 0f;
        return (point - (from + segment * t)).sqrMagnitude <= radius * radius;
    }

    public void HitWall(Vector3 position, Vector2Int direction)
    {
        if (!IsPlaying) return;
        wallEffect.transform.position = position + new Vector3(direction.x, -direction.y, 0f) * .4f;
        wallEffect.Play(); audioController.PlayWall();
    }

    private IEnumerator PlayerDeath()
    {
        State = RoundState.Dying;
        if (workout != null) workout.ResetAfterDeath();
        lives--; hud.SetLives(lives);
        foreach (var ghost in ghosts) ghost.Freeze();
        deathEffect.transform.position = player.transform.position;
        deathEffect.Play(); audioController.PlayDeath();
        player.PlayDeath(audioController.DeathDuration);
        yield return new WaitForSeconds(audioController.DeathDuration);
        if (lives == 0) { EndGame(); yield break; }
        scaredTime = 0f; hud.SetScaredTime(0f);
        foreach (var ghost in ghosts) ghost.ResetGhost();
        player.ResetPlayer();
        State = RoundState.Playing;
        RefreshMusic();
    }

    public void PlayDashSound() { audioController.PlayBonus(); }
    public void AwardWorkout()
    {
        score += 500; hud.SetScore(score); audioController.PlayBonus();
    }
    public void CompleteWorkoutCircuit() { EndGame("CIRCUIT COMPLETE"); }
    private void EndGame(string message = "GAME OVER")
    {
        if (State == RoundState.GameOver) return;
        State = RoundState.GameOver;
        player.StopMotion();
        foreach (var ghost in ghosts) ghost.Freeze();
        audioController.StopMusic();
        hud.ShowMessage(message);
        HighScoreStore.SaveIfBetter(SceneManager.GetActiveScene().name, score, gameTime);
        StartCoroutine(ReturnToMenu());
    }
    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }
}
