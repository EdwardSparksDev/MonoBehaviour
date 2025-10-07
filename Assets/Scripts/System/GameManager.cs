using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour, IGameService
{
    #region Variables & Properties

    #region Local
    private Coroutine timerCR;                                  //Reference to the timer coroutine
    private PlayerState playerState;                            //Reference to the playerState
    private PlayerMovement playerMovement;                      //Reference to the playerMovement
    private EntityMovement[] entitiesMovement;                  //References to all spawned entities movement scripts
    private WaitForSeconds oneSecond = new WaitForSeconds(1);   //The timer decrease delay

    private int score;      //The player score
    private int coins;      //The player coins
    private int lives;      //The player lives
    #endregion

    #region SerializeField
    [Header("Gamemode Parameters")]
    [SerializeField] private bool enableLevelLoadingScreen;
    [SerializeField] private int startingTimer;
    [SerializeField] private int startingLives;

    [SerializeField] private float gameStartDelay;
    [SerializeField] private float gameOverLoadDelay;

    [SerializeField] private int timeRunningOutTrigger;
    [SerializeField] private float scoreCalculationTimeDelta;

    [SerializeField] private int awardedScorePerSecondLeft;
    #endregion

    #region Properties
    public int World { get; private set; }
    public int Stage { get; private set; }
    public int Timer { get; private set; }
    #endregion

    #endregion


    #region Mono
    private IEnumerator Start()
    {
        ServiceLocator.Instance.Register(this);
        Save.LoadData(ref score, ref coins, ref lives);
        Cursor.lockState = CursorLockMode.Locked;

        playerState = FindObjectOfType<PlayerState>();
        playerMovement = FindObjectOfType<PlayerMovement>();
        entitiesMovement = FindObjectsOfType<EntityMovement>();

        World = 1;
        Stage = 1;
        Timer = startingTimer;
        if (lives <= 0)
            yield return StartCoroutine(ResetGame());
        else
            yield return new WaitForEndOfFrame();

        ServiceLocator.Instance.Get<GameUI>().UpdateScore(score);
        ServiceLocator.Instance.Get<GameUI>().UpdateCoins(coins);
        ServiceLocator.Instance.Get<GameUI>().UpdateTimer(Timer);
        ServiceLocator.Instance.Get<GameUI>().UpdateLives(lives);
        ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.MAIN, true);

        timerCR = StartCoroutine(TimerCR());
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }
    #endregion


    #region Methods
    /// <summary>
    /// Initializes all actors by enabling/disabling their movement
    /// </summary>
    /// <param name="canMove">Can the actors move</param>
    private void InitializeActors(bool canMove)
    {
        playerMovement.CanMove = canMove;
        for (int i = 0; i < entitiesMovement.Length; i++)
            entitiesMovement[i].CanMove = canMove;
    }


    /// <summary>
    /// Resets the level on death
    /// </summary>
    public void ResetLevel()
    {
        lives--;
        if (lives <= 0)
            StartCoroutine(GameOver());
        else
        {
            Save.SaveData(score, coins, lives);
            LoadLevel(World, Stage);
        }
    }


    /// <summary>
    /// Resets the level on death after a set delay
    /// </summary>
    /// <param name="delay">The reset delay</param>
    public void ResetLevel(float delay)
    {
        StopTimer();
        Invoke(nameof(ResetLevel), delay);
    }


    /// <summary>
    /// Loads the selected level
    /// </summary>
    /// <param name="world">The world to load</param>
    /// <param name="stage">The stage to load</param>
    public void LoadLevel(int world, int stage)
    {
        SceneManager.LoadScene($"{world}-{stage}");
    }


    /// <summary>
    /// Resets the current game
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResetGame()
    {
        InitializeActors(false);

        score = 0;
        coins = 0;
        lives = startingLives;

        if (enableLevelLoadingScreen)
        {
            ServiceLocator.Instance.Get<GameUI>().ToggleLevelStartingScreen(true, lives);

            yield return new WaitForSeconds(gameStartDelay);

            ServiceLocator.Instance.Get<GameUI>().ToggleLevelStartingScreen(false, default);
        }

        InitializeActors(true);
    }


    /// <summary>
    /// Resets the current game to the default start
    /// </summary>
    /// <returns></returns>
    private IEnumerator GameOver()
    {
        ServiceLocator.Instance.Get<GameUI>().GameOver();
        ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.GAME_OVER, false);
        yield return new WaitForSeconds(gameOverLoadDelay);

        lives = -1;
        Save.SaveData(score, coins, lives);
        LoadLevel(World, Stage);
    }


    /// <summary>
    /// Adds a coin to the counter
    /// </summary>
    public void AddCoin()
    {
        coins++;

        if (coins >= 100)
        {
            AddLife();
            coins = 0;
        }

        ServiceLocator.Instance.Get<GameUI>().UpdateCoins(coins);
    }


    /// <summary>
    /// Adds a life to the counter
    /// </summary>
    public void AddLife()
    {
        lives++;
        ServiceLocator.Instance.Get<GameUI>().UpdateLives(lives);
    }


    /// <summary>
    /// Adds the given score to the score counter
    /// </summary>
    /// <param name="score">The score to add</param>
    public void AddScore(int score)
    {
        this.score += score;
        ServiceLocator.Instance.Get<GameUI>().UpdateScore(this.score);
    }


    /// <summary>
    /// Toggles the pause menu
    /// </summary>
    private void TogglePause()
    {
        if (Time.timeScale <= 0)
        {
            Time.timeScale = 1;
            ServiceLocator.Instance.Get<AudioManager>().Pause(false);
            ServiceLocator.Instance.Get<GameUI>().TogglePause(false);
            Cursor.lockState = CursorLockMode.Locked;
            playerMovement.CanMove = true;
        }
        else
        {
            Time.timeScale = 0;
            ServiceLocator.Instance.Get<AudioManager>().Pause(true);
            ServiceLocator.Instance.Get<GameUI>().TogglePause(true);
            Cursor.lockState = CursorLockMode.None;
            playerMovement.CanMove = false;
        }
    }


    /// <summary>
    /// The game timer coroutine
    /// </summary>
    /// <returns></returns>
    private IEnumerator TimerCR()
    {
        while (Timer > 0)
        {
            yield return oneSecond;
            Timer--;
            ServiceLocator.Instance.Get<GameUI>().UpdateTimer(Timer);

            if (Timer <= 0)
            {
                StopCoroutine(timerCR);
                playerState.Death();
            }
            else if (Timer == timeRunningOutTrigger)
                ServiceLocator.Instance.Get<AudioManager>().TimeRunningOut();
        }
    }


    /// <summary>
    /// Stops the game timer
    /// </summary>
    public void StopTimer()
    {
        if (timerCR != null)
            StopCoroutine(timerCR);
    }


    /// <summary>
    /// Calculates final game score based on time left
    /// </summary>
    /// <returns></returns>
    public IEnumerator CalculateFinalScore()
    {
        int lastTimerDigit = Timer % 10;

        float elapsedTime = Timer * scoreCalculationTimeDelta;
        WaitForSeconds extraTime = new WaitForSeconds(Mathf.Clamp(gameOverLoadDelay - elapsedTime, gameOverLoadDelay / 2f, gameOverLoadDelay));
        WaitForSeconds scoreTimeDelta = new WaitForSeconds(scoreCalculationTimeDelta);

        ServiceLocator.Instance.Get<AudioManager>().ToggleScoreCalculation(true);

        //Gradually decrease timer and increase score
        for (int i = Timer; i > 0; i--)
        {
            Timer--;
            score += awardedScorePerSecondLeft;
            ServiceLocator.Instance.Get<GameUI>().UpdateTimer(Timer);
            ServiceLocator.Instance.Get<GameUI>().UpdateScore(score);
            yield return scoreTimeDelta;
        }

        ServiceLocator.Instance.Get<AudioManager>().ToggleScoreCalculation(false);
        ServiceLocator.Instance.Get<Castle>().RaiseFlag();

        yield return StartCoroutine(ServiceLocator.Instance.Get<FireworksMortar>().FireFireworksCR(lastTimerDigit));
        yield return extraTime;

        lives = -1;
        Save.SaveData(score, coins, lives);
        LoadLevel(World, Stage);
    }
    #endregion
}
