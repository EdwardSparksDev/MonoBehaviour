using UnityEngine;


public enum BackgroundMusicType
{
    MAIN,
    UNDERGROUND,
    STAGE_CLEAR,
    STARPOWER,
    GAME_OVER
}


public class AudioManager : MonoBehaviour, IGameService
{
    #region Variables & Properties

    #region Local
    private AudioSource[] sources;              //The available audio sources

    private BackgroundMusicType currentType;    //The current audio type being played
    #endregion

    #region SerializeField
    [Header("Audio Clips")]
    [SerializeField] private AudioClip ac_Main;
    [SerializeField] private AudioClip ac_Underground;
    [SerializeField] private AudioClip ac_StageClear;
    [SerializeField] private AudioClip ac_StarPower;
    [SerializeField] private AudioClip ac_GameOver;
    [SerializeField] private AudioClip ac_TimeRunningOut;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        sources = GetComponents<AudioSource>();
    }


    private void Start()
    {
        ServiceLocator.Instance.Register<AudioManager>(this);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Main method for playing requested audio
    /// </summary>
    /// <param name="type">The requested audio type</param>
    /// <param name="loop">Should the requested audio loop</param>
    public void PlayBackgroundMusic(BackgroundMusicType type, bool loop)
    {
        switch (type)
        {
            case BackgroundMusicType.MAIN:
                if (sources[0].clip != ac_Main)
                    sources[0].clip = ac_Main;
                currentType = BackgroundMusicType.MAIN;
                break;

            case BackgroundMusicType.UNDERGROUND:
                if (sources[0].clip != ac_Underground)
                    sources[0].clip = ac_Underground;
                currentType = BackgroundMusicType.UNDERGROUND;
                break;

            case BackgroundMusicType.STAGE_CLEAR:
                if (sources[0].clip != ac_StageClear)
                    sources[0].clip = ac_StageClear;
                currentType = BackgroundMusicType.STAGE_CLEAR;
                break;

            case BackgroundMusicType.STARPOWER:
                if (sources[0].clip != ac_StarPower)
                    sources[0].clip = ac_StarPower;
                currentType = BackgroundMusicType.STARPOWER;
                break;

            case BackgroundMusicType.GAME_OVER:
                sources[0].clip = ac_GameOver;
                break;
        }

        sources[0].loop = loop;
        sources[0].Play();
    }


    /// <summary>
    /// Stops the currently played background music and returns its type
    /// </summary>
    /// <returns></returns>
    public BackgroundMusicType StopBackgroundMusic()
    {
        sources[0].Stop();
        return currentType;
    }


    /// <summary>
    /// Pauses and unpauses the current background music
    /// </summary>
    /// <param name="status"></param>
    public void Pause(bool status)
    {
        if (status)
        {
            sources[0].Pause();
            sources[1].Play();
        }
        else
            sources[0].UnPause();
    }


    /// <summary>
    /// Switches the main background music when time is running out
    /// </summary>
    public void TimeRunningOut()
    {
        ac_Main = ac_TimeRunningOut;
        if (currentType == BackgroundMusicType.MAIN)
        {
            sources[0].clip = ac_Main;
            sources[0].Play();
        }
    }


    /// <summary>
    /// Toggles the score calculation audio effect
    /// </summary>
    /// <param name="status">The toggle status</param>
    public void ToggleScoreCalculation(bool status)
    {
        if (status)
            sources[2].Play();
        else
            sources[2].Stop();
    }
    #endregion
}
