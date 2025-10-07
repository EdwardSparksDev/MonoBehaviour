public static class Save
{
    #region Variables & Properties
    private static int score;
    private static int coins;
    private static int lives;
    #endregion


    #region Methods
    /// <summary>
    /// Saves data in script at runtime
    /// </summary>
    /// <param name="newScore">The score to be saved</param>
    /// <param name="newCoins">The coins to be saved</param>
    /// <param name="newLives">The lives to be saved</param>
    public static void SaveData(int newScore, int newCoins, int newLives)
    {
        score = newScore;
        coins = newCoins;
        lives = newLives;
    }


    /// <summary>
    /// Loads data from script at runtime
    /// </summary>
    /// <param name="oldScore">The variable where to load the score into</param>
    /// <param name="oldCoins">The variable where to load the coins into</param>
    /// <param name="oldLives">The variable where to load the lives into</param>
    public static void LoadData(ref int oldScore, ref int oldCoins, ref int oldLives)
    {
        oldScore = score;
        oldCoins = coins;
        oldLives = lives;
    }
    #endregion
}
