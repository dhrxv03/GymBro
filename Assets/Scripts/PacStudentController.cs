using UnityEngine;

[RequireComponent(typeof(Animator), typeof(AudioSource))]
public class PacStudentController : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 3f;
    [SerializeField] private AudioClip movementSound;

    // Clockwise route around the top-left inner block, in world coordinates.
    private readonly Vector3[] corners =
    {
        new Vector3(-12.5f, 13f, 0f),
        new Vector3(-7.5f, 13f, 0f),
        new Vector3(-7.5f, 9f, 0f),
        new Vector3(-12.5f, 9f, 0f)
    };

    private readonly string[] walkingStates =
    {
        "PacStudent_WalkRight", "PacStudent_WalkDown",
        "PacStudent_WalkLeft", "PacStudent_WalkUp"
    };

    private Animator animator;
    private AudioSource movementSource;
    private int currentCorner;
    private float elapsedTime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        movementSource = GetComponent<AudioSource>();
        movementSource.playOnAwake = false;
        movementSource.loop = true;
        movementSource.spatialBlend = 0f;
        movementSource.clip = movementSound;
    }

    private void OnEnable()
    {
        currentCorner = 0;
        elapsedTime = 0f;
        transform.position = corners[0];
        // Disable the automatic animation showcase while following the route.
        animator.SetBool("FollowingRoute", true);
        animator.Play(walkingStates[0], 0, 0f);
    }

    private void Update()
    {
        if (moveSpeed <= 0f || Time.deltaTime <= 0f)
        {
            movementSource.Stop();
            animator.speed = 0f;
            return;
        }

        animator.speed = 1f;
        if (movementSound != null && !movementSource.isPlaying)
            movementSource.Play();

        AdvanceMovement(Time.deltaTime);
    }

    private void AdvanceMovement(float deltaTime)
    {
        elapsedTime += deltaTime;
        int nextCorner = (currentCorner + 1) % corners.Length;
        float duration = Vector3.Distance(corners[currentCorner], corners[nextCorner]) / moveSpeed;

        // Keep leftover frame time at corners so speed is independent of frame rate.
        while (elapsedTime >= duration)
        {
            elapsedTime -= duration;
            currentCorner = nextCorner;
            nextCorner = (currentCorner + 1) % corners.Length;
            animator.Play(walkingStates[currentCorner], 0, 0f);
            duration = Vector3.Distance(corners[currentCorner], corners[nextCorner]) / moveSpeed;
        }

        transform.position = Vector3.Lerp(corners[currentCorner], corners[nextCorner], elapsedTime / duration);
    }

    private void OnDisable()
    {
        movementSource.Stop();
        animator.speed = 1f;
        animator.SetBool("FollowingRoute", false);
    }
}
