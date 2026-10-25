using UnityEngine;

public static class HighScoreStore
{
    private static string Key(string level) { return "GymBro." + level; }
    public static int GetScore(string level) { return PlayerPrefs.GetInt(Key(level) + ".BestScore", 0); }
    public static float GetTime(string level) { return PlayerPrefs.GetFloat(Key(level) + ".BestTime", 0f); }

    public static void SaveIfBetter(string level, int score, float time)
    {
        bool hasRecord = PlayerPrefs.HasKey(Key(level) + ".BestScore");
        if (!hasRecord || score > GetScore(level) || (score == GetScore(level) && time < GetTime(level)))
        {
            PlayerPrefs.SetInt(Key(level) + ".BestScore", score);
            PlayerPrefs.SetFloat(Key(level) + ".BestTime", time);
            PlayerPrefs.Save();
        }
    }
}
