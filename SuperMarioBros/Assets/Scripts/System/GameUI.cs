using TMPro;
using UnityEngine;


public class GameUI : MonoBehaviour, IGameService
{
    #region Variables & Properties
    [Header("UI References")]
    [SerializeField] private GameObject levelScreen;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverMenu;

    [SerializeField] private TMP_Text txt_Score;
    [SerializeField] private TMP_Text txt_Coins;
    [SerializeField] private TMP_Text txt_Timer;
    [SerializeField] private TMP_Text txt_Lives;
    [SerializeField] private TMP_Text txt_StartingLives;
    #endregion


    #region Mono
    private void Start()
    {
        ServiceLocator.Instance.Register<GameUI>(this);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Updates the score text with the given value
    /// </summary>
    /// <param name="newValue">The new score value</param>
    public void UpdateScore(int newValue)
    {
        txt_Score.text = newValue.ToString();
    }


    /// <summary>
    /// Updates the coins text with the given value
    /// </summary>
    /// <param name="newValue">The new coins value</param>
    public void UpdateCoins(int newValue)
    {
        txt_Coins.text = newValue.ToString();
    }


    /// <summary>
    /// Updates the timer text with the given value
    /// </summary>
    /// <param name="newValue">The new timer value</param>
    public void UpdateTimer(int newValue)
    {
        txt_Timer.text = newValue.ToString();
    }


    /// <summary>
    /// Updates the lives text with the given value
    /// </summary>
    /// <param name="newValue">The new lives value</param>
    public void UpdateLives(int newValue)
    {
        txt_Lives.text = newValue.ToString();
    }


    /// <summary>
    /// Toggles the pause menu based on the requested status
    /// </summary>
    /// <param name="status">The new requested status</param>
    public void TogglePause(bool status)
    {
        if (status)
            pauseMenu.SetActive(true);
        else
            pauseMenu.SetActive(false);
    }


    /// <summary>
    /// Enables the game over menu
    /// </summary>
    public void GameOver()
    {
        gameOverMenu.SetActive(true);
    }


    /// <summary>
    /// Toggles the level starting screen
    /// </summary>
    /// <param name="status">The toggling status</param>
    /// <param name="startingLives">The player starting lives value</param>
    public void ToggleLevelStartingScreen(bool status, int startingLives)
    {
        if (status)
        {
            txt_StartingLives.text = startingLives.ToString();
            levelScreen.SetActive(true);
        }
        else
            levelScreen.SetActive(false);

    }
    #endregion
}
