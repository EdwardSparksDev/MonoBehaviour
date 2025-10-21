using System.Collections;
using UnityEngine;


public class PlayerState : MonoBehaviour, IDamageable
{
    #region Variables & Properties

    #region Local
    private DeathAnimation deathAnimation;          //The reference to the death animation script
    private CapsuleCollider2D capsuleCollider;      //The reference to the player capsule collider
    private Coroutine flashingCR;                   //The damage taken flashing sprite coroutine
    private Coroutine invincibilityCR;              //The invincibility window coroutine

    [HideInInspector] public bool HittingBlock;     //Is the player currently hitting a block
    [HideInInspector] public int StompingSpree;     //The player stomping spree
    #endregion

    #region SerializeField
    [Header("PlayerState References")]
    [Tooltip("The reference to the fire flower Mario SpriteRenderer")]
    [SerializeField] private PlayerSpriteRenderer fireRenderer;
    [Tooltip("The reference to the big Mario SpriteRenderer")]
    [SerializeField] private PlayerSpriteRenderer bigRenderer;
    [Tooltip("The reference to the small Mario SpriteRenderer")]
    [SerializeField] private PlayerSpriteRenderer smallRenderer;
    [Tooltip("The reference to the damage taken sound effect")]
    [SerializeField] private AudioClip ac_Damage;
    [Tooltip("The reference to the death sound effect")]
    [SerializeField] private AudioClip ac_Death;

    [Space(20), Header("PlayerState Parameters")]
    [SerializeField] private float deathReloadDelay;
    [SerializeField] private float hitTransformAnimationTime;
    [SerializeField] private int hitTransformAnimationFramesSwitchPerSecond;
    [SerializeField] private int starPowerFramesSwitchPerSecond;
    [SerializeField] private float scaleAnimationDuration;
    [SerializeField] private float invincibilityWindowDuration;
    [SerializeField] private float flashingAnimationDuration;
    [SerializeField] private float flashingAnimationDelay;
    [SerializeField] private float damageMovementBlockingDelay;
    [Space(20)]
    [SerializeField] private Vector2 smallMarioCollisionSize;
    [SerializeField] private Vector2 smallMarioCollisionOffset;
    [SerializeField] private Vector2 bigMarioCollisionSize;
    [SerializeField] private Vector2 bigMarioCollisionOffset;
    #endregion

    #region Properties
    public bool IsBig => bigRenderer == ActiveRenderer;
    public bool IsSmall => smallRenderer == ActiveRenderer;
    public bool HasFireFlower => fireRenderer == ActiveRenderer;
    public bool IsDead => deathAnimation.enabled;
    public bool HasStarpower { get; private set; }
    public PlayerSpriteRenderer ActiveRenderer { get; private set; }
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        deathAnimation = GetComponent<DeathAnimation>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        ActiveRenderer = smallRenderer;
    }


    private void Start()
    {
        smallRenderer.SpriteRenderer.sharedMaterial.SetFloat("_InvertColors", 0);
        bigRenderer.SpriteRenderer.sharedMaterial.SetFloat("_InvertColors", 0);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Handles received damage
    /// </summary>
    public void Hit()
    {
        if (!IsDead && !HasStarpower)
        {
            if (IsBig || HasFireFlower)
                Shrink();
            else
                Death();
        }
    }


    /// <summary>
    /// Kills the player, resetting the level
    /// </summary>
    public void Death()
    {
        AudioSource.PlayClipAtPoint(ac_Death, transform.position);
        ServiceLocator.Instance.Get<AudioManager>().StopBackgroundMusic();

        bigRenderer.enabled = false;
        smallRenderer.enabled = false;
        deathAnimation.enabled = true;

        ServiceLocator.Instance.Get<GameManager>().ResetLevel(deathReloadDelay);
    }


    /// <summary>
    /// Evolves the player to the bigger state
    /// </summary>
    public void Grow()
    {
        if (IsBig)
        {
            GetFireFlower();
            return;
        }

        if (invincibilityCR != null)
            StopCoroutine(invincibilityCR);

        smallRenderer.enabled = false;
        bigRenderer.enabled = true;
        ActiveRenderer = bigRenderer;

        capsuleCollider.size = bigMarioCollisionSize;
        capsuleCollider.offset = bigMarioCollisionOffset;

        StartCoroutine(ScaleAnimation((scaleAnimationDuration), smallRenderer, bigRenderer));
        invincibilityCR = StartCoroutine(DamageInvincibility(invincibilityWindowDuration));
    }


    /// <summary>
    /// Evolves the player to the fireflower state
    /// </summary>
    public void GetFireFlower()
    {
        if (HasFireFlower)
            return;
        else if (IsSmall)
        {
            Grow();
            return;
        }

        if (invincibilityCR != null)
            StopCoroutine(invincibilityCR);

        bigRenderer.enabled = false;
        fireRenderer.enabled = true;
        ActiveRenderer = fireRenderer;

        capsuleCollider.size = bigMarioCollisionSize;
        capsuleCollider.offset = bigMarioCollisionOffset;

        StartCoroutine(ScaleAnimation((scaleAnimationDuration), bigRenderer, fireRenderer));
        invincibilityCR = StartCoroutine(DamageInvincibility(invincibilityWindowDuration));
    }


    /// <summary>
    /// Shrinks the player to the smaller state
    /// </summary>
    private void Shrink()
    {
        AudioSource.PlayClipAtPoint(ac_Damage, transform.position);

        if (flashingCR != null)
            StopCoroutine(flashingCR);
        if (invincibilityCR != null)
            StopCoroutine(invincibilityCR);

        smallRenderer.enabled = false;
        fireRenderer.enabled = false;
        bigRenderer.enabled = false;

        PlayerSpriteRenderer previousRenderer = null;
        if (ActiveRenderer == fireRenderer)
        {
            previousRenderer = fireRenderer;
            ActiveRenderer = bigRenderer;
        }
        else
        {
            previousRenderer = bigRenderer;
            ActiveRenderer = smallRenderer;
        }

        if (ActiveRenderer == smallRenderer)
        {
            capsuleCollider.size = smallMarioCollisionSize;
            capsuleCollider.offset = smallMarioCollisionOffset;
        }

        StartCoroutine(ScaleAnimation((scaleAnimationDuration), previousRenderer, ActiveRenderer));
        invincibilityCR = StartCoroutine(DamageInvincibility(invincibilityWindowDuration * 2f));
        flashingCR = StartCoroutine(SpriteFlashing(ActiveRenderer.SpriteRenderer, flashingAnimationDuration, flashingAnimationDelay));
    }


    /// <summary>
    /// The character scaling animation corouting
    /// </summary>
    /// <param name="animationTime">The animation length</param>
    /// <param name="disabledRenderer">The playerSpriteRenderer to be disabled</param>
    /// <param name="enabledRenderer">The playerSpriteRenderer to be enabled</param>
    /// <returns></returns>
    private IEnumerator ScaleAnimation(float animationTime, PlayerSpriteRenderer disabledRenderer, PlayerSpriteRenderer enabledRenderer)
    {
        float elapsed = 0f;
        float animationTrigger = 0f;
        float frameTime = 1f / hitTransformAnimationFramesSwitchPerSecond;

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            animationTrigger += Time.deltaTime;

            if (animationTrigger >= frameTime)
            {
                disabledRenderer.enabled = !disabledRenderer.enabled;
                enabledRenderer.enabled = !disabledRenderer.enabled;
                animationTrigger = 0f;
            }

            yield return null;
        }

        fireRenderer.enabled = false;
        bigRenderer.enabled = false;
        smallRenderer.enabled = false;
        ActiveRenderer.enabled = true;
    }


    /// <summary>
    /// The sprite flashing animation coroutine
    /// </summary>
    /// <param name="renderer">The sprite renderer to flash</param>
    /// <param name="animationTime">The flashing animation duration</param>
    /// <param name="delay">The flashing animation activation delay</param>
    /// <returns></returns>
    private IEnumerator SpriteFlashing(SpriteRenderer renderer, float animationTime, float delay = 0f)
    {
        float elapsed = 0f;
        float flashesPerSecond = 16f;
        float animationTrigger = 0f;
        float flashTime = 1f / flashesPerSecond;

        yield return new WaitForSeconds(delay);

        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            animationTrigger += Time.deltaTime;

            if (animationTrigger >= flashTime)
            {
                renderer.enabled = !renderer.enabled;
                animationTrigger = 0f;
            }

            yield return null;
        }

        renderer.enabled = true;
    }


    /// <summary>
    /// The damage invincibility window activator coroutine
    /// </summary>
    /// <param name="invincibilityWindow">The invincibility duration</param>
    /// <returns></returns>
    private IEnumerator DamageInvincibility(float invincibilityWindow)
    {
        gameObject.layer = LayerMask.NameToLayer("Invincible");

        PlayerMovement movement = GetComponent<PlayerMovement>();
        Vector2 storedVelocity = movement.Velocity;

        movement.enabled = false;

        yield return new WaitForSeconds(damageMovementBlockingDelay);

        movement.enabled = true;
        movement.SetVelocity(storedVelocity);

        yield return new WaitForSeconds(invincibilityWindow - damageMovementBlockingDelay);

        gameObject.layer = LayerMask.NameToLayer("Player");
    }


    /// <summary>
    /// Activates the starpower powerup
    /// </summary>
    /// <param name="duration">The activation duration</param>
    public void ActivateStarpower(float duration = 10f)
    {
        StartCoroutine(StarpowerAnimation(duration));
    }


    /// <summary>
    /// Coroutine for the activation of the star power up effect
    /// </summary>
    /// <param name="duration">The effect duration</param>
    /// <returns></returns>
    private IEnumerator StarpowerAnimation(float duration)
    {
        HasStarpower = true;
        BackgroundMusicType previousClipType = ServiceLocator.Instance.Get<AudioManager>().StopBackgroundMusic();
        ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.STARPOWER, false);

        float elapsed = 0f;
        float animationTrigger = 0f;
        float frameTime = 1f / starPowerFramesSwitchPerSecond;
        bool inverted = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            animationTrigger += Time.deltaTime;

            if (animationTrigger >= frameTime)
            {
                //Invert sprite material colors
                if (inverted)
                    ActiveRenderer.SpriteRenderer.sharedMaterial.SetFloat("_InvertColors", 1);
                else
                    ActiveRenderer.SpriteRenderer.sharedMaterial.SetFloat("_InvertColors", 0);

                inverted = !inverted;

                animationTrigger = 0f;
            }

            yield return null;
        }

        //Reset sprite material color to default
        ActiveRenderer.SpriteRenderer.sharedMaterial.SetFloat("_InvertColors", 0);

        HasStarpower = false;
        ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(previousClipType, true);
    }


    /// <summary>
    /// Prevents multiple blocks from being hit all at once
    /// </summary>
    public void EnableMultipleBlocksHitPrevention()
    {
        StartCoroutine(EnableMultipleBlocksHitPreventionCR());
    }


    /// <summary>
    /// Coroutine that prevents multiple blocks from being hit all at once
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnableMultipleBlocksHitPreventionCR()
    {
        HittingBlock = true;
        yield return new WaitForSeconds(.2f);
        HittingBlock = false;
    }
    #endregion
}
