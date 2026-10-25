using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class PacStudentController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
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
        cell = level.WorldToGrid(transform.position);
        transform.position = level.GridToWorld(cell);
        // Face right, but wait for the first key press before moving.
        animator.Play(walkingState, 0, 0f);
        animator.Update(0f);
        UpdateEffects();
    }

    private void Update()
    {
        ReadInput();
        AdvanceMovement(Time.deltaTime);
        UpdateEffects();
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
        if (lastInput != Vector2Int.zero && level.CanPlayerEnter(cell + lastInput))
            currentInput = lastInput;
        else if (currentInput == Vector2Int.zero || !level.CanPlayerEnter(cell + currentInput))
            return false;

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
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            // Treat tiny floating-point differences as reaching the tile centre.
            if (elapsed >= duration - .000001f)
            {
                transform.position = endPosition;
                cell = targetCell;
                isMoving = false;
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
