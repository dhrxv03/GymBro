using UnityEngine;

public enum GhostMode { Normal, Scared, Recovering, Dead }

[RequireComponent(typeof(Animator))]
public class GhostStateController : MonoBehaviour
{
    [SerializeField] private string characterName;
    private Animator animator;
    private Vector3 spawnPosition;
    private float deadTime;
    public GhostMode Mode { get; private set; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
    }
    public void SetMode(GhostMode mode)
    {
        Mode = mode;
        string state = mode == GhostMode.Normal ? "NormalDown" : mode == GhostMode.Scared ? "ScaredDown" : mode.ToString();
        animator.speed = 1f;
        animator.Play(characterName + "_" + state, 0, 0f);
    }
    public void SetGlobalMode(float scaredTime)
    {
        if (Mode == GhostMode.Dead) return;
        GhostMode next = scaredTime <= 0f ? GhostMode.Normal : scaredTime <= 3f ? GhostMode.Recovering : GhostMode.Scared;
        if (Mode != next) SetMode(next);
    }
    public void Eaten() { deadTime = 3f; SetMode(GhostMode.Dead); }
    public void Tick(float deltaTime, float scaredTime)
    {
        if (Mode == GhostMode.Dead)
        {
            deadTime -= deltaTime;
            if (deadTime <= 0f)
                SetMode(scaredTime <= 0f ? GhostMode.Normal : scaredTime <= 3f ? GhostMode.Recovering : GhostMode.Scared);
        }
        else SetGlobalMode(scaredTime);
    }
    public void Freeze() { animator.speed = 0f; }
    public void ResetGhost()
    {
        transform.position = spawnPosition;
        deadTime = 0f;
        SetMode(GhostMode.Normal);
    }
}
