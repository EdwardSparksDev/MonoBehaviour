using UnityEngine;


public class PlayerSpriteRenderer : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private PlayerMovement movement;    //The player movement reference
    #endregion

    #region SerializeField
    [Header("Sprites")]
    [SerializeField] private Sprite spt_Idle;
    [SerializeField] private Sprite spt_Jump;
    [SerializeField] private Sprite spt_Slide;
    [SerializeField] private Sprite spt_Crouch;
    [SerializeField] private Sprite spt_Flag;
    [SerializeField] private Sprite spt_Firing;
    [SerializeField] private AnimatedSprite as_Run;
    #endregion

    #region Properties
    public SpriteRenderer SpriteRenderer { get; private set; }
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponentInParent<PlayerMovement>();
    }


    private void OnEnable()
    {
        SpriteRenderer.enabled = true;
    }


    private void OnDisable()
    {
        SpriteRenderer.enabled = false;
        as_Run.enabled = false;
    }


    //Updates sprite renderer based on player movement
    private void LateUpdate()
    {
        as_Run.enabled = movement.Running;

        if (movement.HoldingFlag)
            SpriteRenderer.sprite = spt_Flag;
        else if (spt_Crouch != null && movement.Crouching)
            SpriteRenderer.sprite = spt_Crouch;
        else if (spt_Firing != null && movement.Firing)
            SpriteRenderer.sprite = spt_Firing;
        else if (movement.Jumping)
            SpriteRenderer.sprite = spt_Jump;
        else if (movement.Sliding)
            SpriteRenderer.sprite = spt_Slide;
        else if (!movement.Running)
            SpriteRenderer.sprite = spt_Idle;
    }
    #endregion
}
