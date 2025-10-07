using UnityEngine;


public class AnimatedSprite : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private SpriteRenderer spriteRenderer;      //The sprite renderer to be animated

    private float framerate;    //The animated sprite framerate
    private int frame;          //The current animated frame
    #endregion

    #region SerializeField
    [Header("Animation")]
    [SerializeField] private float framesPerSecond;
    [SerializeField] private Sprite[] sprites;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        framerate = 1f / framesPerSecond;
    }


    private void OnEnable()
    {
        InvokeRepeating(nameof(Animate), framerate, framerate);
    }


    private void OnDisable()
    {
        CancelInvoke();
    }
    #endregion


    #region Methods
    /// <summary>
    /// Animates the sprite renderer by running in a cycle through all the available sprites in the animation
    /// </summary>
    private void Animate()
    {
        if (++frame >= sprites.Length)
            frame = 0;

        if (frame >= 0 && frame < sprites.Length)
            spriteRenderer.sprite = sprites[frame];
    }
    #endregion
}
