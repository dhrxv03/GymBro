using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GhostStateController))]
public class GhostController : MonoBehaviour
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private GameController game;
    [SerializeField] private PacStudentController player;
    [SerializeField, Range(1, 4)] private int ghostNumber = 1;
    private GhostStateController state;
    private Vector3 spawn;
    private Vector3 stepStart, stepEnd;
    private Vector2Int cell, direction;
    private float progress;
    private bool moving, leaving, reachedOutside;
    private int exitStage;
    private readonly Vector2Int[] directions = { Vector2Int.right, Vector2Int.down, Vector2Int.left, Vector2Int.up };
    public float Speed => player.MoveSpeed * .9f * (state.Mode == GhostMode.Normal ? 1f : .5f);

    private void Awake()
    {
        state = GetComponent<GhostStateController>();
        spawn = transform.position;
    }
    public void ResetMovement()
    {
        moving = false; leaving = true; reachedOutside = false; exitStage = 0; progress = 0f;
        direction = Vector2Int.zero;
        cell = level.WorldToGrid(transform.position);
    }
    private void Update()
    {
        if (game.IsPlaying) AdvanceMovement(Time.deltaTime);
    }
    public void AdvanceMovement(float deltaTime)
    {
        float remaining = deltaTime;
        while (remaining > .000001f && game.IsPlaying)
        {
            if (state.Mode == GhostMode.Dead)
            {
                moving = false;
                float distance = Vector3.Distance(transform.position, spawn);
                float used = Mathf.Min(remaining, distance / Speed);
                transform.position = Vector3.MoveTowards(transform.position, spawn, Speed * used);
                remaining -= used;
                if (Vector3.Distance(transform.position, spawn) > .00001f) return;
                transform.position = spawn;
                state.Revive(game.ScaredTime);
                ResetMovement();
                game.GhostReturned();
                continue;
            }
            if (!moving) BeginStep();
            if (!moving) return; // Only possible for a malformed map with no valid neighbours.
            float length = Vector3.Distance(stepStart, stepEnd);
            float usedTime = Mathf.Min(remaining, (1f - progress) * length / Speed);
            Vector3 before = transform.position;
            progress = Mathf.Min(1f, progress + usedTime * Speed / length);
            transform.position = Vector3.Lerp(stepStart, stepEnd, progress);
            remaining -= usedTime;
            if (progress >= .999999f)
            {
                transform.position = stepEnd; cell = level.WorldToGrid(stepEnd); moving = false;
            }
            game.GhostMoved(state, before, transform.position);
        }
    }
    private void BeginStep()
    {
        Vector2Int next;
        if (leaving)
        {
            // Use the nearest central gate column, then travel straight through the assigned gate.
            int gateX = ghostNumber <= 2 ? 13 : 14;
            int outsideRow = ghostNumber % 2 == 1 ? 11 : 17;
            next = exitStage == 0 ? new Vector2Int(gateX, 14) : new Vector2Int(gateX, outsideRow);
            if (cell == next)
            {
                exitStage++;
                if (exitStage == 1) next = new Vector2Int(gateX, outsideRow);
                else { leaving = false; BeginStep(); return; }
            }
            // One grid interval at a time, including the gate while leaving the pen.
            Vector2Int difference = next - cell;
            next = cell + new Vector2Int(difference.x == 0 ? 0 : (int)Mathf.Sign(difference.x),
                difference.x != 0 || difference.y == 0 ? 0 : (int)Mathf.Sign(difference.y));
        }
        else next = cell + ChooseDirection();
        if (next == cell) return;
        direction = next - cell;
        stepStart = transform.position; stepEnd = level.GridToWorld(next);
        progress = 0f; moving = true;
        state.Face(direction);
    }
    public bool CanEnter(Vector2Int position)
    {
        // The outer tunnel cells are teleporters, so ghosts turn back before entering them.
        if (position.x <= 0 || position.x >= level.Columns - 1) return false;
        return level.CanPlayerEnter(position) && !(position.x >= 11 && position.x <= 16 && position.y >= 13 && position.y <= 15);
    }
    // A small grid search gets ghost 4 out to the boundary; no Unity navigation tools.
    private int DistanceToOutside(Vector2Int start, Vector2Int blocked)
    {
        var queue = new Queue<Vector2Int>();
        var distances = new Dictionary<Vector2Int, int>();
        queue.Enqueue(start); distances[start] = 0;
        while (queue.Count > 0)
        {
            var position = queue.Dequeue();
            if (position == new Vector2Int(26, 27)) return distances[position];
            foreach (var option in directions)
            {
                var next = position + option;
                if (next == blocked || !CanEnter(next) || distances.ContainsKey(next)) continue;
                distances[next] = distances[position] + 1; queue.Enqueue(next);
            }
        }
        return int.MaxValue;
    }
    private Vector2Int ChooseDirection()
    {
        var valid = new List<Vector2Int>();
        foreach (var option in directions) if (CanEnter(cell + option)) valid.Add(option);
        if (valid.Count > 1) valid.Remove(-direction);
        if (valid.Count == 0) return Vector2Int.zero;
        if (state.Mode != GhostMode.Normal) reachedOutside = false;
        if (state.Mode == GhostMode.Normal && ghostNumber == 4)
        {
            if (cell == new Vector2Int(26, 27)) reachedOutside = true;
            if (!reachedOutside)
            {
                Vector2Int best = valid[0]; int shortest = int.MaxValue;
                foreach (var option in valid)
                {
                    int distance = DistanceToOutside(cell + option, cell);
                    if (distance < shortest) { shortest = distance; best = option; }
                }
                return best;
            }
            // Keep the outside wall on the left to circle it clockwise (grid y points down).
            Vector2Int left = new Vector2Int(direction.y, -direction.x);
            Vector2Int[] priority = { left, direction, -left, -direction };
            foreach (var option in priority) if (valid.Contains(option)) return option;
        }
        if (state.Mode != GhostMode.Normal || ghostNumber == 1 || ghostNumber == 2)
        {
            bool flee = state.Mode != GhostMode.Normal || ghostNumber == 1;
            float currentDistance = (level.GridToWorld(cell) - player.transform.position).sqrMagnitude;
            var preferred = new List<Vector2Int>();
            foreach (var option in valid)
            {
                float distance = (level.GridToWorld(cell + option) - player.transform.position).sqrMagnitude;
                if (flee ? distance >= currentDistance : distance <= currentDistance) preferred.Add(option);
            }
            // If every route violates the distance preference, keep moving without reversing.
            if (preferred.Count > 0) valid = preferred;
        }
        return valid[Random.Range(0, valid.Count)];
    }
}
