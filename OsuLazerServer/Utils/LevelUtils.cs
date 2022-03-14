namespace OsuLazerServer.Utils;

public class LevelUtils
{
    public static long GetRequiredScoreForLevel(int level)
    {
        /*if (level < 100)
        {
            if (level >= 2)
                return 5000 / 3 * (4 * (level * 3) - 3 * (level ^ 2) - level) + 1.25 * (2 ^ (level - 60));
        }*/
        return 0;
    }
}