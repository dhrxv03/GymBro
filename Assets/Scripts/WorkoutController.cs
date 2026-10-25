using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Level 2 trades energy between a short escape dash and four workout stations.
public class WorkoutController : MonoBehaviour
{
    [SerializeField] private GameController game;
    [SerializeField] private PacStudentController player;
    [SerializeField] private Text statusText;
    [SerializeField] private Text hintText;
    [SerializeField] private Transform[] stations;
    [SerializeField] private SpriteRenderer[] stationIcons;
    [SerializeField] private Text[] stationLabels;
    private bool[] completed;
    private int energy = 8;
    private int workouts;
    private float dashTime;
    private float rechargeTime;
    private SpriteRenderer playerSprite;
    public bool IsDashing => dashTime > 0f;
    public float MovementMultiplier => IsDashing ? 2f : 1f;

    private void Awake()
    {
        completed = new bool[stations.Length];
        playerSprite = player.GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        if (game.IsPlaying)
        {
            dashTime = Mathf.Max(0f, dashTime - Time.deltaTime);
            // Slow recovery prevents a lost life or exhausted map from making the goal impossible.
            rechargeTime += Time.deltaTime;
            if (rechargeTime >= 3f) { rechargeTime -= 3f; GainEnergy(); }
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) TryDash();
                if (keyboard.eKey.wasPressedThisFrame) TryWorkout();
            }
        }
        playerSprite.color = IsDashing ? new Color(.3f, 1f, 1f) : Color.white;
        statusText.text = "ENERGY " + energy + "/12     WORKOUTS " + workouts + "/4";
        int nearby = NearbyStation();
        hintText.text = IsDashing ? "DASH ACTIVE - protected from junk food" :
            nearby >= 0 ? (energy >= 8 ? "Press E: finish workout (-8 energy)" : "Workout needs 8 energy. Collect protein or recharge.") :
            "Finish all 4 | WASD move | SPACE dash (4) | E workout (8)";
    }
    public void GainEnergy() { energy = Mathf.Min(12, energy + 1); }
    public bool TryDash()
    {
        if (!game.IsPlaying || IsDashing || energy < 4 || !player.IsMoving) return false;
        energy -= 4; dashTime = .6f; game.PlayDashSound(); return true;
    }
    private int NearbyStation()
    {
        for (int i = 0; i < stations.Length; i++)
            if (!completed[i] && Vector3.Distance(player.transform.position, stations[i].position) <= .8f) return i;
        return -1;
    }
    public bool TryWorkout()
    {
        int index = NearbyStation();
        if (!game.IsPlaying || index < 0 || energy < 8) return false;
        energy -= 8; completed[index] = true; workouts++;
        stationIcons[index].color = new Color(.3f, 1f, .65f);
        stationLabels[index].text = "DONE";
        game.AwardWorkout();
        if (workouts == stations.Length) game.CompleteWorkoutCircuit();
        return true;
    }
    public void ResetAfterDeath()
    {
        dashTime = 0f; energy = Mathf.Max(4, energy); rechargeTime = 0f;
        playerSprite.color = Color.white;
    }
}
