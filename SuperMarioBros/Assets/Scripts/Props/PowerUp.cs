using UnityEngine;


public enum PowerUpType
{
    COIN,
    EXTRA_LIFE,
    MAGIC_MUSHROOM,
    STAR,
    FIRE_FLOWER
}


public class PowerUp : MonoBehaviour
{
    #region Variables & Properties
    [Header("PowerUp References")]
    [Tooltip("The sound to be played on pickup")]
    [SerializeField] private AudioClip ac_Collected;

    [Space(20), Header("PowerUp Parameters")]
    [Tooltip("The power up type")]
    [SerializeField] private PowerUpType type;
    [Tooltip("The layers which can pick up the power up")]
    [SerializeField] private LayerMask collectMask;

    [SerializeField] private int awardedScoreOnPickup;
    #endregion


    #region Mono
    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((collectMask.value & (1 << other.gameObject.layer)) > 0)
            Collect(other.gameObject);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Method used for power up collection
    /// </summary>
    /// <param name="collector">The actor collecting the power up</param>
    private void Collect(GameObject collector)
    {
        switch (type)
        {
            case PowerUpType.COIN:
                ServiceLocator.Instance.Get<GameManager>().AddCoin();
                ServiceLocator.Instance.Get<GameManager>().AddScore(awardedScoreOnPickup);
                break;

            case PowerUpType.EXTRA_LIFE:
                ServiceLocator.Instance.Get<GameManager>().AddLife();
                break;

            case PowerUpType.MAGIC_MUSHROOM:
                collector.GetComponent<PlayerState>().Grow();
                ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(collector.transform.position, awardedScoreOnPickup);
                break;

            case PowerUpType.STAR:
                collector.GetComponent<PlayerState>().ActivateStarpower();
                break;

            case PowerUpType.FIRE_FLOWER:
                ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(collector.transform.position, awardedScoreOnPickup);
                PlayerState playerState = collector.GetComponent<PlayerState>();

                if (playerState.IsSmall)
                    playerState.Grow();
                else
                    playerState.GetFireFlower();
                break;
        }

        if (ac_Collected != null)
            AudioSource.PlayClipAtPoint(ac_Collected, transform.position);

        Destroy(gameObject);
    }
    #endregion
}