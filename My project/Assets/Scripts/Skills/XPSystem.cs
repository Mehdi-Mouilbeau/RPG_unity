public static class XPSystem
{
    public const int MaxLevel = 30;

    // Index = level - 1. CumulativeXPThresholds[0] = 0 (level 1), [29] = 50000 (level 30).
    private static readonly int[] CumulativeXPThresholds =
    {
            0,   // L1
          100,   // L2
          200,   // L3
          350,   // L4
          500,   // L5
          700,   // L6
          950,   // L7
        1_250,   // L8
        1_600,   // L9
        2_000,   // L10
        2_600,   // L11
        3_300,   // L12
        4_200,   // L13
        5_300,   // L14
        6_600,   // L15
        8_100,   // L16
        9_800,   // L17
       11_700,   // L18
       13_500,   // L19
       15_000,   // L20
       17_500,   // L21
       20_200,   // L22
       23_100,   // L23
       26_200,   // L24
       29_600,   // L25
       33_400,   // L26
       37_600,   // L27
       42_200,   // L28
       46_000,   // L29
       50_000,   // L30
    };

    /// <summary>Returns the level corresponding to the given cumulative XP (1–30).</summary>
    public static int GetLevel(int xp)
    {
        int level = 1;
        for (int i = 1; i < CumulativeXPThresholds.Length; i++)
        {
            if (xp >= CumulativeXPThresholds[i])
                level = i + 1;
            else
                break;
        }
        return level;
    }

    /// <summary>Returns cumulative XP required to reach the given level.</summary>
    public static int CumulativeXPForLevel(int level)
    {
        if (level < 1) return 0;
        if (level > MaxLevel) level = MaxLevel;
        return CumulativeXPThresholds[level - 1];
    }

    /// <summary>Returns XP needed to go from currentLevel to currentLevel+1. Returns 0 at max level.</summary>
    public static int XPForNextLevel(int currentLevel)
    {
        if (currentLevel >= MaxLevel) return 0;
        return CumulativeXPThresholds[currentLevel] - CumulativeXPThresholds[currentLevel - 1];
    }

    public static bool IsMaxLevel(int level) => level >= MaxLevel;
}
