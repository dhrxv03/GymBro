using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject manualLevel;
    [SerializeField] private Camera gameCamera;
    [Tooltip("Index matches the map legend. Leave index 0 empty.")]
    [SerializeField] private GameObject[] tilePrefabs = new GameObject[9];

    private int[,] levelMap =
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0}
    };

    // Directions are right, down, left, up (array rows increase downwards).
    private readonly int[] rowStep = { 0, 1, 0, -1 };
    private readonly int[] columnStep = { 1, 0, -1, 0 };

    private void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        // Prepare and validate everything before removing the saved level.
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null || tilePrefabs == null || tilePrefabs.Length < 9)
        {
            Debug.LogError("Assign the camera and all eight tile prefabs.", this);
            return;
        }

        int[,] map = MirrorMap(levelMap);
        for (int row = 0; row < map.GetLength(0); row++)
        {
            for (int column = 0; column < map.GetLength(1); column++)
            {
                int tile = map[row, column];
                if (tile < 0 || tile > 8 || (tile != 0 && tilePrefabs[tile] == null))
                {
                    Debug.LogError("The map has an invalid tile or an unassigned prefab.", this);
                    return;
                }
            }
        }

        List<int>[,] rotations = FindRotations(map);
        if (!SolveRotations(map, rotations))
        {
            Debug.LogError("These wall tiles cannot form matching connections. Check the map.", this);
            return;
        }

        if (manualLevel != null)
        {
            manualLevel.SetActive(false);
            Destroy(manualLevel); // Runtime only: the saved scene remains intact.
        }

        var level = new GameObject("GeneratedLevel");
        int rows = map.GetLength(0);
        int columns = map.GetLength(1);
        for (int row = 0; row < rows; row++)
        {
            var rowGroup = new GameObject("Row_" + row.ToString("D2"));
            rowGroup.transform.SetParent(level.transform, false);
            for (int column = 0; column < columns; column++)
            {
                int tile = map[row, column];
                if (tile == 0) continue;
                Vector3 position = new Vector3(column - (columns - 1) / 2f, (rows - 1) / 2f - row, 0f);
                int angle = IsWall(tile) ? rotations[row, column][0] * 90 : 0;
                GameObject piece = Instantiate(tilePrefabs[tile], position, Quaternion.Euler(0f, 0f, angle), rowGroup.transform);
                piece.name = tilePrefabs[tile].name + "_R" + row.ToString("D2") + "_C" + column.ToString("D2");
                piece.transform.localScale = Vector3.one;
            }
        }

        gameCamera.orthographic = true;
        gameCamera.transform.position = new Vector3(0f, 0f, -10f);
        gameCamera.transform.rotation = Quaternion.identity;
        gameCamera.orthographicSize = Mathf.Max(rows / 2f + 1f, (columns / 2f + 1f) / gameCamera.aspect);
    }

    private int[,] MirrorMap(int[,] quadrant)
    {
        int rows = quadrant.GetLength(0) * 2 - 1;
        int columns = quadrant.GetLength(1) * 2;
        int[,] map = new int[rows, columns];
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
                map[row, column] = quadrant[Mathf.Min(row, rows - 1 - row), Mathf.Min(column, columns - 1 - column)];
        return map;
    }

    private bool IsWall(int tile)
    {
        return tile == 1 || tile == 2 || tile == 3 || tile == 4 || tile == 7 || tile == 8;
    }

    // 0 = closed edge, 1 = single line, 2 = double line.
    private int Connection(int tile, int rotation, int direction)
    {
        int side = (direction + rotation) % 4;
        if (tile == 1) return side < 2 ? 2 : 0;
        if (tile == 2) return side % 2 == 0 ? 2 : 0;
        if (tile == 3) return side < 2 ? 1 : 0;
        if (tile == 4 || tile == 8) return side % 2 == 0 ? 1 : 0;
        if (tile == 7) return side % 2 == 0 ? 2 : side == 1 ? 1 : 0;
        return 0;
    }

    private List<int>[,] FindRotations(int[,] map)
    {
        int rows = map.GetLength(0), columns = map.GetLength(1);
        var choices = new List<int>[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int tile = map[row, column];
                if (!IsWall(tile)) continue;
                choices[row, column] = new List<int>();
                int count = tile == 2 || tile == 4 || tile == 8 ? 2 : 4;
                for (int rotation = 0; rotation < count; rotation++)
                {
                    bool valid = true;
                    for (int direction = 0; direction < 4; direction++)
                    {
                        if (Connection(tile, rotation, direction) == 0) continue;
                        int r = row + rowStep[direction], c = column + columnStep[direction];
                        if (r < 0 || r >= rows || c < 0 || c >= columns)
                        {
                            // Outside walls can continue off the sides beside a tunnel.
                            if (!(tile == 2 && direction % 2 == 0 && row > 0 && row < rows - 1)) valid = false;
                        }
                        else if (!IsWall(map[r, c])) valid = false;
                    }
                    // The specification fixes the top-left outside corner's orientation.
                    if (row == 0 && column == 0 && tile == 1 && rotation != 0) valid = false;
                    if (valid) choices[row, column].Add(rotation);
                }
            }
        }
        return choices;
    }

    private bool MatchesNeighbours(int[,] map, List<int>[,] choices, int row, int column, int rotation)
    {
        for (int direction = 0; direction < 4; direction++)
        {
            int r = row + rowStep[direction], c = column + columnStep[direction];
            if (r < 0 || r >= map.GetLength(0) || c < 0 || c >= map.GetLength(1) || choices[r, c] == null) continue;
            bool match = false;
            foreach (int neighbourRotation in choices[r, c])
                if (Connection(map[row, column], rotation, direction) == Connection(map[r, c], neighbourRotation, (direction + 2) % 4)) match = true;
            if (!match) return false;
        }
        return true;
    }

    private bool SolveRotations(int[,] map, List<int>[,] choices)
    {
        // Keep removing impossible rotations until neighbouring tiles agree.
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int column = 0; column < map.GetLength(1); column++)
                {
                    var options = choices[row, column];
                    if (options == null) continue;
                    for (int i = options.Count - 1; i >= 0; i--)
                        if (!MatchesNeighbours(map, choices, row, column, options[i])) { options.RemoveAt(i); changed = true; }
                    if (options.Count == 0) return false;
                }
            }
        }

        // An unusual map can leave several valid options. Try each on a copy.
        for (int row = 0; row < map.GetLength(0); row++)
        {
            for (int column = 0; column < map.GetLength(1); column++)
            {
                if (choices[row, column] == null || choices[row, column].Count == 1) continue;
                foreach (int rotation in choices[row, column])
                {
                    var attempt = new List<int>[map.GetLength(0), map.GetLength(1)];
                    for (int r = 0; r < map.GetLength(0); r++)
                        for (int c = 0; c < map.GetLength(1); c++)
                            if (choices[r, c] != null) attempt[r, c] = new List<int>(choices[r, c]);
                    attempt[row, column] = new List<int> { rotation };
                    if (!SolveRotations(map, attempt)) continue;
                    for (int r = 0; r < map.GetLength(0); r++)
                        for (int c = 0; c < map.GetLength(1); c++) choices[r, c] = attempt[r, c];
                    return true;
                }
                return false;
            }
        }
        return true;
    }
}
