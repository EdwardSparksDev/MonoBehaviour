using System.Collections;
using UnityEngine;


public class DeathAnimation : MonoBehaviour
{
    #region Variables & Properties
    [Header("Animation")]
    [SerializeField] private float animationDuration;
    [SerializeField] private float animationJumpVelocity;
    [SerializeField] private float animationGravity;
    [SerializeField] private Sprite spt_Dead;
    [SerializeField] private SpriteRenderer spriteRenderer;
    #endregion


    #region Mono
    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void OnEnable()
    {
        UpdateSprite();
        DisablePhysics();
        StartCoroutine(Animate());
    }
    #endregion


    #region Methods
    /// <summary>
    /// Updates the sprite renderer with the death sprite
    /// </summary>
    private void UpdateSprite()
    {
        spriteRenderer.enabled = true;
        spriteRenderer.sortingOrder = 100;

        if (spt_Dead != null)
            spriteRenderer.sprite = spt_Dead;
    }


    /// <summary>
    /// Disables all character physics
    /// </summary>
    private void DisablePhysics()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();

        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        GetComponent<Rigidbody2D>().isKinematic = true;

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        EntityMovement entityMovement = GetComponent<EntityMovement>();

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (entityMovement != null)
            entityMovement.enabled = false;
    }


    /// <summary>
    /// Animated the death sequence with a vertical jump
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        float elapsed = 0f;

        Vector3 velocity = Vector3.up * animationJumpVelocity;

        while (elapsed < animationDuration)
        {
            transform.position += velocity * Time.deltaTime;
            velocity.y += animationGravity * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion
}
