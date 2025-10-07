using PhysicsExtensions;
using System.Collections;
using UnityEngine;


public class BlockHit : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private AudioSource source;     //The block hit audio source

    private bool animating;         //Is the block currently getting animated
    #endregion

    #region SerializeField
    [Header("Block Parameters")]
    [SerializeField] private LayerMask breakMask;
    [SerializeField] private int maxHits;
    [SerializeField] private float maxBreakAngle;

    [SerializeField] private Sprite spt_Empty;

    [SerializeField] private float animationVerticalOffset;
    [SerializeField] private float animationDuration;

    [SerializeField] private GameObject[] items;
    [SerializeField] private GameObject destroyedParticles;

    [SerializeField] private LayerMask damageMask;
    [SerializeField] private float upwardsHitRadius;
    [SerializeField] private float upwardsHitDistance;

    [SerializeField] private int awardedScoreOnBreak;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If the block is hit from underneath
        if (collision.transform.Dot(transform, Vector2.up, maxBreakAngle) && collision.relativeVelocity.y > 0)
        {
            if (!animating && (breakMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                PlayerState playerState = collision.gameObject.GetComponent<PlayerState>();

                //Multi-block hit prevention
                if (playerState.HittingBlock)
                    return;
                else
                    playerState.EnableMultipleBlocksHitPrevention();

                //Hit and destroy block
                if (destroyedParticles != null && maxHits < 0 && !playerState.IsSmall)
                {
                    HitAbove();
                    Instantiate(destroyedParticles, transform.position, Quaternion.identity);
                    ServiceLocator.Instance.Get<GameManager>().AddScore(awardedScoreOnBreak);
                    Destroy(gameObject);
                }
                //Hit and raise block
                else
                {
                    //Block hit
                    source.Play();

                    if (maxHits != 0)
                    {
                        HitAbove();
                        Hit(playerState.IsSmall);
                    }
                }
            }
        }
    }
    #endregion


    #region Methods
    /// <summary>
    /// Method called when a block is hit from beneath
    /// </summary>
    /// <param name="IsPlayerSmall">Is the player currently small</param>
    private void Hit(bool IsPlayerSmall)
    {
        //Reveal hidden blocks
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.enabled == false)
            spriteRenderer.enabled = true;

        //Update empty blocks
        if (--maxHits == 0)
            spriteRenderer.sprite = spt_Empty;

        //Spawn block items
        if (items.Length > 0)
        {
            if (items.Length > 1 && !IsPlayerSmall)
                Instantiate(items[1], transform.position, Quaternion.identity);
            else
                Instantiate(items[0], transform.position, Quaternion.identity);
        }

        StartCoroutine(Animate());
    }


    /// <summary>
    /// Animates the hit block
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        animating = true;

        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * animationVerticalOffset;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);

        animating = false;
    }


    /// <summary>
    /// Method used for animating position changes
    /// </summary>
    /// <param name="from">The starting position</param>
    /// <param name="to">The ending position</param>
    /// <returns></returns>
    private IEnumerator Move(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;

            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = to;
    }


    /// <summary>
    /// Deals damage to actors above block when hit
    /// </summary>
    private void HitAbove()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, upwardsHitRadius, Vector2.up, upwardsHitDistance, damageMask);
        if (hit.collider != null)
        {
            IDamageable iDamageable;

            hit.collider.gameObject.TryGetComponent<IDamageable>(out iDamageable);
            if (iDamageable != null)
                iDamageable.Hit();
        }
    }
    #endregion
}
