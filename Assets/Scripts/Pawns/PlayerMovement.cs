using PhysicsExtensions;
using System.Collections;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private Rigidbody2D rb;                             //The player character rigidbody
    private Camera cam;                                 //The main camera
    private CapsuleCollider2D col;                      //The player capsule collide 2d
    private AudioSource[] sources;                      //The player audio sources
    private PlayerState playerState;                    //The current player state
    private WaitForSeconds fireDelay;                   //The firerate delay
    private WaitForSeconds fireAnimDuration;            //The firing animation duration
    private WaitForSeconds stompingSpreeResetTimer;     //The stomping spree reset delay timer

    private Vector2 rigidbodyPosition;          //The current player character rigidbody position
    private Vector2 bottomLeftScreenCorner;     //The bottom left screen to world point screen corner
    private Vector2 topRightScreenCorner;       //The top right screen to world point screen corner
    private Vector2 velocity;                   //The current player character velocity

    private bool canMove = true;                //Can the character currently move
    private float moveSpeed;                    //The character moving speed
    private float horizontalInput;              //The user horizontal input axis value
    private bool falling;                       //Is the player character currently falling
    private float gravityMultiplier;            //The player character current gravity multiplier
    private float spawnDirection;               //The projectile spawn direction based on current speed
    private bool canFire = true;                //Whether or not the player can currently fire
    private bool firing;                        //Is the character currently firing

    [HideInInspector] public bool HoldingFlag;
    #endregion

    #region SerializeField
    [Header("Movement and Jump")]
    [Tooltip("The player character movement speed")]
    [SerializeField] private float walkSpeed;
    [Tooltip("The player character sprinting speed")]
    [SerializeField] private float sprintSpeed;
    [Tooltip("How quickly will the player build up speed")]
    [SerializeField] private float velocityResponsiveness;
    [Tooltip("The player maximum jump height")]
    [SerializeField] private float maxJumpHeight;
    [Tooltip("The total time it will take the player to jump and fall back to the ground")]
    [SerializeField] private float maxJumpTotalTime;

    [Space(20), Header("Ground Check")]
    [Tooltip("The ground check circle cast radius")]
    [SerializeField] private float groundedCheckRadius;
    [Tooltip("The circle cast distance")]
    [SerializeField] private float groundedCheckDistance;
    [Tooltip("The layers which should be checked for ground")]
    [SerializeField] private LayerMask groundCheckMask;

    [Space(20), Header("Gravity")]
    [Tooltip("How faster should the player character fall to the ground")]
    [SerializeField] private float gravityFallMultiplier;
    [Tooltip("The divider reducing the player character terminal velocity")]
    [SerializeField] private float terminalVelocityDivider;
    [Tooltip("The layers which shouldn't trigger falling on head collision")]
    [SerializeField] private LayerMask headHitNoFallMask;
    [Tooltip("The max angle (relative to the hit object) at which hitting the character's head will result in falling")]
    [SerializeField] private float fallTriggeringAngle;

    [Space(20), Header("Bounce")]
    [Tooltip("Which layers should make the player character bounce upwards")]
    [SerializeField] private LayerMask bounceMask;
    [Tooltip("The maximum angle of the collision at which the player character will bounce")]
    [SerializeField] private float maxBounceAngle;
    [Tooltip("The player character bounce multiplier")]
    [SerializeField] private float bounceMultiplier;
    [Tooltip("After how many seconds should the stomping spree be reset")]
    [SerializeField] private float stompingSpreeResetDelay;

    [Space(20), Header("Firing")]
    [Tooltip("The fireflower particle to be shot")]
    [SerializeField] private GameObject fireParticle;
    [Tooltip("The offset from the center of the character where the fireflower particle will spawn")]
    [SerializeField] private Vector3 fireSpawnOffset;
    [Tooltip("The fireflower power up firerate")]
    [SerializeField] private float maxShotsPerSecond;
    [Tooltip("The fireflower firing animation duration")]
    [SerializeField] private float fireAnimationDuration;
    #endregion

    #region Properties
    public bool CanMove { get => canMove; set => canMove = value; }
    public Vector2 Velocity => velocity;
    public float JumpForce => (2f * maxJumpHeight) / (maxJumpTotalTime / 2f);    //The maxJumpHeight is multiplied by 2 because we must also consider the fall
    public float Gravity => (-2f * maxJumpHeight) / Mathf.Pow((maxJumpTotalTime / 2f), 2);    //The time is set to the power of two because the unit is m/s^2
    public bool Grounded { get; private set; }
    public bool Jumping { get; private set; }
    public bool Running => Mathf.Abs(velocity.x) > .25f || Mathf.Abs(horizontalInput) > .25f;
    public bool Sliding => (horizontalInput > 0f && velocity.x < 0f) || (horizontalInput < 0f && velocity.x > 0f);
    public bool Crouching { get; private set; }
    public bool Firing => firing;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        sources = GetComponentsInChildren<AudioSource>();
        playerState = GetComponent<PlayerState>();
        cam = Camera.main;

        if (1f / maxShotsPerSecond - fireAnimationDuration > 0f)
            fireDelay = new WaitForSeconds(1f / maxShotsPerSecond - fireAnimationDuration);
        else
            fireDelay = null;
        fireAnimDuration = new WaitForSeconds(fireAnimationDuration);
        stompingSpreeResetTimer = new WaitForSeconds(stompingSpreeResetDelay);
    }


    private void OnEnable()
    {
        rb.isKinematic = false;
        velocity = Vector2.zero;
        Jumping = false;
        Crouching = false;
        horizontalInput = 0f;
        moveSpeed = walkSpeed;
    }


    private void OnDisable()
    {
        rb.isKinematic = true;
        velocity = Vector2.zero;
        Jumping = false;
        Crouching = false;
        horizontalInput = 1f;
    }


    private void FixedUpdate()
    {
        rigidbodyPosition = rb.position;
        rigidbodyPosition += velocity * Time.fixedDeltaTime;

        bottomLeftScreenCorner = cam.ScreenToWorldPoint(Vector2.zero);
        topRightScreenCorner = cam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        //Checks if the player is trying to go off screen. If so reset his velocity.x, as if he has hit a wall
        if (rigidbodyPosition.x < bottomLeftScreenCorner.x + transform.localScale.x / 2)
            velocity.x = 0f;

        //Clamps the new player position to stay inside the camera view
        rigidbodyPosition.x = Mathf.Clamp(rigidbodyPosition.x, bottomLeftScreenCorner.x + transform.localScale.x / 2,
            topRightScreenCorner.x + transform.localScale.x / 2);

        rb.MovePosition(rigidbodyPosition);
    }


    private void Update()
    {
        if (!canMove) return;

        HorizontalMovement();

        Fire();

        Grounded = rb.Raycast(groundedCheckRadius, Vector2.down, groundedCheckDistance, groundCheckMask);
        if (Grounded)
            JumpingMovement();

        ApplyGravity();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((bounceMask.value & (1 << collision.gameObject.layer)) > 0 && !playerState.HasStarpower)
        {
            if (transform.Dot(collision.transform, Vector2.down, maxBounceAngle))
            {
                velocity.y = JumpForce * bounceMultiplier;
                Jumping = true;
            }
        }
        //If collision object is not in the <noFallHeadTriggerMask>, start falling
        else if ((headHitNoFallMask.value & (1 << collision.gameObject.layer)) <= 0)
        {
            if (transform.Dot(collision.transform, Vector2.up, fallTriggeringAngle))
                velocity.y = 0f;
        }
    }
    #endregion


    #region Methods
    /// <summary>
    /// The ground-based horizontal movement management
    /// </summary>
    private void HorizontalMovement()
    {
        Crouch();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            moveSpeed = sprintSpeed;
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            moveSpeed = walkSpeed;

        //The velocity is slowly added depending on the <moveSpeed> and <velocityResponsiveness>
        velocity.x = Mathf.MoveTowards(velocity.x, horizontalInput * moveSpeed, velocityResponsiveness * moveSpeed * Time.deltaTime);

        //If something is hit towards the velocity direction, reset the velocity.x to zero
        if (rb.Raycast(groundedCheckRadius, Vector2.right * velocity.x, groundedCheckDistance, groundCheckMask))
            velocity.x = 0f;

        //Flip sprite based on velocity.x
        if (transform.eulerAngles.y != 0f && velocity.x > 0f)
            transform.rotation = Quaternion.Euler(Vector3.zero);
        else if (transform.eulerAngles.y != 180f && velocity.x < 0f)
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }


    /// <summary>
    /// Handles crouching limiting the horizontal input
    /// </summary>
    private void Crouch()
    {
        //If the player is big try to crouch
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) && !playerState.IsSmall)
        {
            if (!Jumping)
            {
                if (!Crouching)
                {
                    Crouching = true;
                    col.size = new Vector2(col.size.x, 1.5f);
                    col.offset = new Vector2(0f, .25f);
                }
                if (horizontalInput != 0f)
                    horizontalInput = 0f;
            }
        }
        else
        {
            if (Crouching)
            {
                Crouching = false;
                col.size = new Vector2(col.size.x, 2f);
                col.offset = new Vector2(0f, .5f);
            }
            horizontalInput = Input.GetAxis("Horizontal");
        }
    }


    /// <summary>
    /// Handles the fireflower power up firing
    /// </summary>
    private void Fire()
    {
        if (playerState.HasFireFlower && canFire && !Crouching && (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Z)))
        {
            if (transform.eulerAngles.y == 0f)
                spawnDirection = 1f;
            else
                spawnDirection = -1f;

            sources[0].Play();
            Instantiate(fireParticle, transform.position + new Vector3(fireSpawnOffset.x * spawnDirection,
                fireSpawnOffset.y, 0f), Quaternion.identity)
                .GetComponent<ProjectileMovement>().Direction = new Vector2(spawnDirection, 0f);

            //Firerate delay
            StartCoroutine(FireDelayCR());
        }
    }


    /// <summary>
    /// Limits the fireflower power up firerate
    /// </summary>
    /// <returns></returns>
    private IEnumerator FireDelayCR()
    {
        firing = true;
        canFire = false;
        yield return fireAnimDuration;
        firing = false;
        yield return fireDelay;
        canFire = true;
    }


    /// <summary>
    /// Handles the jump
    /// </summary>
    private void JumpingMovement()
    {
        //Reset stomping spree if player is grounded
        if (playerState.StompingSpree > 0 && Grounded)
            StartCoroutine(ResetStompingSpree());

        //Prevents velocity buildup while grounded
        velocity.y = Mathf.Max(velocity.y, 0f);
        Jumping = velocity.y > 0f;

        if (Input.GetButtonDown("Jump"))
        {
            velocity.y = JumpForce;
            Jumping = true;

            //Audio
            if (playerState.IsSmall)
                sources[2].Play();
            else
                sources[1].Play();
        }
    }


    /// <summary>
    /// Constantly applies artificial gravity to the player based on current falling condition and jumping button hold
    /// </summary>
    private void ApplyGravity()
    {
        falling = velocity.y < 0f || !Input.GetButton("Jump");
        gravityMultiplier = falling ? gravityFallMultiplier : 1f;

        velocity.y += Gravity * gravityMultiplier * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, Gravity / terminalVelocityDivider);
    }


    /// <summary>
    /// Sets back the player velocity after a power up pickup
    /// </summary>
    /// <param name="newVelocity">The new velocity to be set</param>
    /// <param name="delay">The set delay</param>
    public void SetVelocity(Vector2 newVelocity, float delay = 0f)
    {
        StartCoroutine(SetVelocityCR(newVelocity, delay));
    }


    /// <summary>
    /// The velocity change coroutine
    /// </summary>
    /// <param name="newVelocity">The new velocity to be set</param>
    /// <param name="delay">The set delay</param>
    /// <returns></returns>
    private IEnumerator SetVelocityCR(Vector2 newVelocity, float delay)
    {
        yield return new WaitForSeconds(delay);

        velocity = newVelocity;
    }


    /// <summary>
    /// Resets the stomping spree after <stompingSpreeResetDelay> seconds
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResetStompingSpree()
    {
        yield return stompingSpreeResetTimer;
        playerState.StompingSpree = 0;
    }
    #endregion
}
