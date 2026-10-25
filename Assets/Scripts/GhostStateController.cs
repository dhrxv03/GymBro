using UnityEngine;

public enum GhostMode { Normal, Scared, Recovering, Dead }

[RequireComponent(typeof(Animator))]
public class GhostStateController : MonoBehaviour
{
    [SerializeField] private string characterName;
    private Animator animator;
    private Vector3 spawnPosition;
    private Vector2Int facing = new Vector2Int(0, 1);
    public GhostMode Mode { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
    }
    public void SetMode(GhostMode mode)
    {
        Mode = mode;
        string state = mode == GhostMode.Normal ? "Normal" + DirectionName() : mode == GhostMode.Scared ? "Scared" + DirectionName() : mode.ToString();
        animator.speed = 1f;
        animator.Play(characterName + "_" + state, 0, 0f);
    }
    public void SetGlobalMode(float scaredTime)
    {
        if (Mode == GhostMode.Dead) return;
        GhostMode next = scaredTime <= 0f ? GhostMode.Normal : scaredTime <= 3f ? GhostMode.Recovering : GhostMode.Scared;
        if (Mode != next) SetMode(next);
    }
    private string DirectionName() => facing.x > 0 ? "Right" : facing.x < 0 ? "Left" : facing.y < 0 ? "Up" : "Down";
    public void Face(Vector2Int direction)
    {
        if (facing == direction) return;
        facing = direction;
        if (Mode == GhostMode.Normal || Mode == GhostMode.Scared) SetMode(Mode);
    }
    public void Eaten() { SetMode(GhostMode.Dead); }
    public void Revive(float scaredTime)
    {
        SetMode(scaredTime <= 0f ? GhostMode.Normal : scaredTime <= 3f ? GhostMode.Recovering : GhostMode.Scared);
    }
    public void Tick(float deltaTime, float scaredTime) { SetGlobalMode(scaredTime); }
    public void Freeze() { animator.speed = 0f; }
    public void ResetGhost()
    {
        transform.position = spawnPosition;
        facing = new Vector2Int(0, 1);
        GetComponent<GhostController>().ResetMovement();
        SetMode(GhostMode.Normal);
    }
}
