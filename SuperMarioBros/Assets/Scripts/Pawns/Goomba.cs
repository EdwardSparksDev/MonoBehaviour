using PhysicsExtensions;
using UnityEngine;


public class Goomba : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    private AudioSource source;     //The goomba audio source
    #endregion

    #region SerializeField
    [Header("Goomba Parameters")]
    [SerializeField] private LayerMask stompMask;
    [SerializeField] private float maxStompAngle;
    [SerializeField] private float destroyDelay;
    [SerializeField] private Sprite spt_Flat;

    [SerializeField] private LayerMask damageMask;
    [SerializeField] private float deathDelay;

    [SerializeField] private int awardedScore;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If the goomba is getting hit
        if ((stompMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            PlayerState playerState = collision.gameObject.GetComponent<PlayerState>();

            if (playerState.HasStarpower)
                Hit();
            else if (collision.transform.Dot(transform, Vector2.down, maxStompAngle))
                Flatten(playerState);
            else
                playerState.Hit();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //If the goomba is hit by a shell
        if ((damageMask.value & (1 << other.gameObject.layer)) > 0)
            Hit();
    }
    #endregion


    #region Methods
    /// <summary>
    /// Flattens and destroys the goomba
    /// </summary>
    /// <param name="playerState">The reference to the playerState</param>
    private void Flatten(PlayerState playerState)
    {
        source.Play();
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(transform.position,
            (int)(playerState.StompingSpree > 0 ? awardedScore * Mathf.Pow(2, playerState.StompingSpree) : awardedScore));
        playerState.StompingSpree++;

        GetComponent<Collider2D>().enabled = false;
        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = spt_Flat;
        Destroy(gameObject, destroyDelay);
    }


    /// <summary>
    /// Kills the goomba playing the death animation
    /// </summary>
    public void Hit()
    {
        source.Play();
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(transform.position, awardedScore);

        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        GetComponent<SpriteRenderer>().flipY = true;

        Destroy(gameObject, deathDelay);
    }
    #endregion
}
