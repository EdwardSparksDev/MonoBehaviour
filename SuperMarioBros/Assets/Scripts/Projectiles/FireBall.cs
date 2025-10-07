using System.Collections;
using UnityEngine;


public class FireBall : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private AnimatedSprite explosionAnimation;      //The reference to the explosion animation
    private WaitForSeconds rotationDelay;           //The fireball rotation delay
    private Coroutine rotationCR;                   //The fireball rotation animation coroutine
    private ProjectileMovement fireballMovement;    //Reference to the fireball movement
    private Rigidbody2D rb;                         //The fireball rigidbody2D

    private bool exploding;                         //Is the fireball currently exploding
    #endregion

    #region SerializeField
    [Header("Fireball Parameters")]
    [SerializeField] private int rotationsPerSecond;
    [SerializeField] private float destructionDelay;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        explosionAnimation = GetComponent<AnimatedSprite>();
        fireballMovement = GetComponent<ProjectileMovement>();
        rb = GetComponent<Rigidbody2D>();
        rotationDelay = new WaitForSeconds(1f / rotationsPerSecond);

        rotationCR = StartCoroutine(Rotate());
    }


    private void OnEnable()
    {
        fireballMovement.OnHit += Explode;
    }


    private void OnDisable()
    {
        fireballMovement.OnHit -= Explode;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Rotates the fireball sprite
    /// </summary>
    /// <returns></returns>
    private IEnumerator Rotate()
    {
        while (true)
        {
            yield return rotationDelay;
            transform.localRotation = Quaternion.Euler(0f, 0f, transform.localEulerAngles.z - 90f);
        }
    }


    /// <summary>
    /// Makes the fireball explode on impact
    /// </summary>
    private void Explode()
    {
        if (!exploding)
            StartCoroutine(ExplodeCR());
    }


    /// <summary>
    /// Coroutine for fireball explosion handling
    /// </summary>
    /// <returns></returns>
    private IEnumerator ExplodeCR()
    {
        exploding = true;
        fireballMovement.Speed = 0f;
        Destroy(rb);
        Destroy(GetComponent<CircleCollider2D>());
        StopCoroutine(rotationCR);
        explosionAnimation.enabled = true;

        yield return new WaitForSeconds(destructionDelay);
        Destroy(gameObject);
    }
    #endregion
}
