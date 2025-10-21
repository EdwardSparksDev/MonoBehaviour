using PhysicsExtensions;
using UnityEngine;


public class EntityMovement : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private Rigidbody2D rb;         //The entity rigidbody

    private Vector2 velocity;       //The entity velocity
    private bool canMove = true;    //Can the entity currently move
    #endregion

    #region SerializeField
    [Header("Physics")]
    [SerializeField] private float speed;
    [SerializeField] private Vector2 direction;

    [Header("Ground Check")]
    [SerializeField] private float groundedCheckRadius;
    [SerializeField] private float groundedCheckDistance;
    [SerializeField] public LayerMask groundCheckMask;
    #endregion

    #region Properties
    public bool CanMove { get => canMove; set => canMove = value; }
    public float Speed { get => speed; set => speed = value; }
    public Vector2 Direction { get => direction; set => direction = value; }
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enabled = false;
    }


    private void OnEnable()
    {
        rb.WakeUp();
    }


    private void OnDisable()
    {
        rb.velocity = Vector2.zero;
        rb.Sleep();
    }


    private void OnBecameVisible()
    {
        enabled = true;
    }


    private void OnBecameInvisible()
    {
        enabled = false;
    }


    private void FixedUpdate()
    {
        if (!canMove) return;

        velocity.x = direction.x * speed;
        velocity.y += Physics2D.gravity.y * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        //Boundaries detection
        if (rb.Raycast(groundedCheckRadius, direction, groundedCheckDistance, groundCheckMask))
            direction = -direction;

        //Ground velocity buildup prevention
        if (rb.Raycast(groundedCheckRadius, Vector2.down, groundedCheckDistance, groundCheckMask))
            velocity.y = Mathf.Max(velocity.y, 0f);
    }
    #endregion
}
