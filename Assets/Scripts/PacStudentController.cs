using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class PacStudentController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private GameController game;
    [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
    [SerializeField] private AudioClip movementSound;
    [SerializeField] private AudioClip pelletSound;
    [SerializeField] private ParticleSystem dust;

    // Grid directions: positive x is right; positive y is down the map.
    private Vector2Int lastInput;
    private Vector2Int currentInput;
    private Vector2Int cell;
    private Vector2Int targetCell;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float elapsed;
    private bool isMoving;
    private bool forceInwardStep;
    private Vector2Int lastBlockedDirection;
    private string walkingState = "PacStudent_WalkRight";
    private Animator animator;
    private AudioSource movementSource;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movementSource = GetComponent<AudioSource>();
        movementSource.playOnAwake = false;
        movementSource.loop = true;
        movementSource.spatialBlend = 0f;
    }

    private void Start()
    {
        if (level == null)
        {
            Debug.LogError("Assign the LevelGenerator to PacStudent.", this);
            enabled = false;
            return;
        }
        ResetPlayer();
    }

    public void ResetPlayer()
    {
        cell = new Vector2Int(1, 1);
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;
        lastBlockedDirection = Vector2Int.zero;
        isMoving = false;
        forceInwardStep = false;
        elapsed = 0f;
        transform.position = level.GridToWorld(cell);
        walkingState = "PacStudent_WalkRight";
        animator.Play(walkingState, 0, 0f);
        animator.Update(0f);
        UpdateEffects();
    }

    public void StopMotion()
    {
        isMoving = false;
        movementSource.Stop();
        animator.speed = 0f;
        if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void PlayDeath(float duration)
    {
        StopMotion();
        float clipLength = .333333f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
            if (clip.name == "PacStudent_Death") clipLength = clip.length;
        animator.speed = clipLength / duration;
        animator.Play("PacStudent_Death", 0, 0f);
    }

    private void Update()
    {
        if (!game.IsPlaying)
        {
            if (game.State != RoundState.Dying) StopMotion();
            return;
        }
        ReadInput();
        AdvanceMovement(Time.deltaTime);
        if (game.IsPlaying) UpdateEffects();
    }

    private void ReadInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.wKey.wasPressedThisFrame) lastInput = new Vector2Int(0, -1);
        if (keyboard.aKey.wasPressedThisFrame) lastInput = new Vector2Int(-1, 0);
        if (keyboard.sKey.wasPressedThisFrame) lastInput = new Vector2Int(0, 1);
        if (keyboard.dKey.wasPressedThisFrame) lastInput = new Vector2Int(1, 0);
        // Releasing a key never clears the queued direction.
    }

    private bool BeginStep()
    {
        if (forceInwardStep)
            forceInwardStep = false;
        else if (lastInput != Vector2Int.zero && level.CanPlayerEnter(cell + lastInput))
            currentInput = lastInput;
        else if (currentInput == Vector2Int.zero || !level.CanPlayerEnter(cell + currentInput))
        {
            Vector2Int blocked = currentInput != Vector2Int.zero ? currentInput : lastInput;
            if (blocked != Vector2Int.zero && blocked != lastBlockedDirection)
            {
                lastBlockedDirection = blocked;
                game.HitWall(transform.position, blocked);
            }
            return false;
        }
        lastBlockedDirection = Vector2Int.zero;

        targetCell = cell + currentInput;
        startPosition = level.GridToWorld(cell);
        endPosition = level.GridToWorld(targetCell);
        elapsed = 0f;
        isMoving = true;

        string nextState;
        if (currentInput.x > 0) nextState = "PacStudent_WalkRight";
        else if (currentInput.x < 0) nextState = "PacStudent_WalkLeft";
        else if (currentInput.y < 0) nextState = "PacStudent_WalkUp";
        else nextState = "PacStudent_WalkDown";
        if (walkingState != nextState)
        {
            walkingState = nextState;
            animator.Play(walkingState, 0, 0f);
        }

        AudioClip clip = level.HasPellet(targetCell) ? pelletSound : movementSound;
        if (movementSource.clip != clip)
        {
            movementSource.Stop();
            movementSource.clip = clip;
        }
        return true;
    }

    private void AdvanceMovement(float deltaTime)
    {
        float remaining = deltaTime;
        float duration = 1f / moveSpeed;
        // Carry unused frame time into the next tile, keeping speed consistent.
        while (remaining > 0f)
        {
            if (!isMoving && !BeginStep()) break;
            float step = Mathf.Min(remaining, duration - elapsed);
            elapsed += step;
            remaining -= step;
            Vector3 before = transform.position;
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            game.PlayerMoved(before, transform.position);
            if (!game.IsPlaying) return;
            // Treat tiny floating-point differences as reaching the tile centre.
            if (elapsed >= duration - .000001f)
            {
                transform.position = endPosition;
                cell = targetCell;
                isMoving = false;
                if (cell.y == level.Rows / 2 && (cell.x == 0 || cell.x == level.Columns - 1))
                {
                    // Teleport only at a tunnel end; never sweep collisions across the maze.
                    cell.x = cell.x == 0 ? level.Columns - 1 : 0;
                    transform.position = level.GridToWorld(cell);
                    forceInwardStep = true;
                }
            }
            else break;
        }
        // At an exact tile boundary, decide whether motion can continue.
        if (!isMoving) BeginStep();
    }

    private void UpdateEffects()
    {
        animator.speed = isMoving ? 1f : 0f;
        if (isMoving)
        {
            if (movementSource.clip != null && !movementSource.isPlaying) movementSource.Play();
            if (dust != null && !dust.isPlaying) dust.Play();
        }
        else
        {
            movementSource.Stop();
            if (dust != null && dust.isEmitting) dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void OnDisable()
    {
        if (movementSource != null) movementSource.Stop();
        if (animator != null) animator.speed = 0f;
        if (dust != null) dust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
