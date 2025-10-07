using PhysicsExtensions;
using UnityEngine;


public class Koopa : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    private AudioSource[] sources;  //The koopa audio sources

    private bool shelled;           //Is the koopa currently in shell mode
    private bool pushed;            //Has the koopa been pushed in shell mode
    #endregion

    #region SerializeField
    [Header("Koopa Parameters")]
    [SerializeField] private LayerMask stompMask;
    [SerializeField] private float maxStompAngle;
    [SerializeField] private float shellSpeed;
    [SerializeField] private Sprite spt_Shell;

    [SerializeField] private LayerMask damageMask;
    [SerializeField] private float deathDelay;
    [SerializeField] private bool destroyPushedShellOffScreen;

    [SerializeField] private int awardedScore;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        sources = GetComponents<AudioSource>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If the koopa is getting hit
        if (!shelled && (stompMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            PlayerState playerState = collision.gameObject.GetComponent<PlayerState>();

            if (playerState.HasStarpower)
                Hit();
            else if (collision.transform.Dot(transform, Vector2.down, maxStompAngle))
                EnterShell(playerState);
            else
                playerState.Hit();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Enables koopa's shell mode
        if (shelled && (stompMask.value & (1 << other.gameObject.layer)) > 0)
        {
            if (!pushed)
            {
                //Push
                Vector2 direction = new Vector2(transform.position.x - other.transform.position.x, 0f);
                PushShell(direction);
            }
            else
            {
                //Hit
                PlayerState playerState = other.GetComponent<PlayerState>();

                if (playerState.HasStarpower)
                    Hit();
                else
                    playerState.Hit();
            }
        }
        else if ((damageMask.value & (1 << other.gameObject.layer)) > 0)
            Hit();
    }


    private void OnBecameInvisible()
    {
        //Deletes shell if pushed outside screen boundaries and <destroyPushedShellOffScreen> is enabled
        if (pushed && destroyPushedShellOffScreen)
            Destroy(gameObject);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Enables koopa's shell mode
    /// </summary>
    /// <param name="playerState">The reference to the playerState</param>
    private void EnterShell(PlayerState playerState)
    {
        shelled = true;
        sources[0].Play();
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(transform.position,
            (int)(playerState.StompingSpree > 0 ? awardedScore * Mathf.Pow(2, playerState.StompingSpree) : awardedScore));
        playerState.StompingSpree++;

        GetComponent<EntityMovement>().enabled = false;
        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<SpriteRenderer>().sprite = spt_Shell;
        GetComponent<Rigidbody2D>().isKinematic = true;
    }


    /// <summary>
    /// Pushes the koopa's shell while in shell mode
    /// </summary>
    /// <param name="direction">The push direction</param>
    private void PushShell(Vector2 direction)
    {
        pushed = true;
        sources[1].Play();
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(transform.position, awardedScore);

        GetComponent<Rigidbody2D>().isKinematic = false;

        EntityMovement movement = GetComponent<EntityMovement>();
        movement.Direction = direction.normalized;
        movement.Speed = shellSpeed;
        movement.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Shell");
    }


    /// <summary>
    /// Kills the koompa playing the death animation
    /// </summary>
    public void Hit()
    {
        sources[0].Play();
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(transform.position, awardedScore * 2);

        GetComponent<AnimatedSprite>().enabled = false;
        GetComponent<DeathAnimation>().enabled = true;
        GetComponent<SpriteRenderer>().flipY = true;

        Destroy(gameObject, deathDelay);
    }
    #endregion
}
