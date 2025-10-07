using PhysicsExtensions;
using System;
using UnityEngine;


public class ProjectileMovement : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private Rigidbody2D rb;                     //The projectile rigidbody2D

    private IDamageable iDamageable;            //Reference to the hit IDamageable interface

    private Vector2 velocity;                   //The projectile velocity
    private Vector2 startingBouncePosition;     //The projectile bounce starting position

    public Action OnHit;                        //The projectile OnHit event

    private bool bouncing;                      //Is the projectile currently bouncing
    private float bounceTimer;                  //The projectile bounce timer
    #endregion

    #region SerializeField
    [Header("Physics")]
    [SerializeField] private float speed;
    [SerializeField] private Vector2 direction;

    [Space(20), Header("Damage and Trajectory")]
    [SerializeField] private LayerMask damageMask;
    [SerializeField] private float bounceTriggerAngle;
    [SerializeField] private float bounceDuration;
    [SerializeField] private float bounceReach;
    [SerializeField] private float bounceHeight;
    #endregion

    #region Properties
    public float Speed { get => speed; set => speed = value; }
    public Vector2 Direction { get => direction; set => direction = value; }
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }


    private void FixedUpdate()
    {
        if (speed == 0f) return;

        //45° movement
        velocity.x = direction.x * speed;
        velocity.y = -Mathf.Abs(velocity.x);

        //If the projectile is bouncing, follow a sinusoidal trajectory, else move in a 45° downward angle
        if (bouncing)
        {
            bounceTimer += Time.fixedDeltaTime;
            rb.MovePosition(startingBouncePosition + new Vector2(direction.x * bounceTimer / bounceDuration * bounceReach,
                bounceHeight * Mathf.Sin(Mathf.PI * bounceTimer / bounceDuration)));

            if (bounceTimer > bounceDuration)
                bouncing = false;
        }
        else
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        BoxCollider2D col = collision.collider as BoxCollider2D;

        //Deal damage
        if ((damageMask.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out iDamageable))
                iDamageable.Hit();
            OnHit();
        }
        else
        {
            //If the projectile hits a surface on its upward face keep bouncing, else call OnHit
            if (col != null && col.IsUpwardFaceContact(transform))
            {
                bounceTimer = 0f;
                startingBouncePosition = transform.position;
                bouncing = true;
            }
            else
                OnHit();
        }
    }
    #endregion
}
