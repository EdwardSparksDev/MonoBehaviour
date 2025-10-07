using System.Collections;
using UnityEngine;


public class BlockItem : MonoBehaviour
{
    #region Variables & Properties
    [Header("Item Parameters")]
    [SerializeField] private float spawnDelay;
    [SerializeField] private float animationTime;
    [SerializeField] private float upwardsShift;

    [SerializeField] private AudioClip ac_Spawn;
    #endregion


    #region Mono
    private void Start()
    {
        StartCoroutine(Animate());
    }
    #endregion


    #region Methods
    /// <summary>
    /// Animates the item coming out of the block
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        //Disables all physics
        rb.isKinematic = true;
        if (collider != null)
            collider.enabled = false;
        trigger.enabled = false;
        spriteRenderer.enabled = false;

        yield return new WaitForSeconds(spawnDelay);

        spriteRenderer.enabled = true;

        if (ac_Spawn != null)
            AudioSource.PlayClipAtPoint(ac_Spawn, transform.position);

        float elapsed = 0f;

        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = transform.localPosition + Vector3.up * upwardsShift;

        while (elapsed < animationTime)
        {
            float t = elapsed / animationTime;

            transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = endPosition;

        //Enables physics
        if (collider != null)
        {
            rb.isKinematic = false;
            collider.enabled = true;
        }
        trigger.enabled = true;
    }
    #endregion
}
