using System.Collections;
using UnityEngine;


public class BlockCoin : MonoBehaviour
{
    #region Variables & Properties
    [Header("Coin Parameters")]
    [SerializeField] private float animationVerticalOffset;
    [SerializeField] private float animationDuration;

    [SerializeField] private AudioClip ac_Coin;

    [SerializeField] private int awardedScore;
    #endregion


    #region Mono
    private void Start()
    {
        AudioSource.PlayClipAtPoint(ac_Coin, transform.position);
        ServiceLocator.Instance.Get<GameManager>().AddCoin();
        ServiceLocator.Instance.Get<GameManager>().AddScore(awardedScore);
        StartCoroutine(Animate());
    }
    #endregion


    #region Methods
    /// <summary>
    /// Animates the coin once it comes out of the block
    /// </summary>
    /// <returns></returns>
    private IEnumerator Animate()
    {
        Vector3 restingPosition = transform.localPosition;
        Vector3 animatedPosition = restingPosition + Vector3.up * animationVerticalOffset;

        yield return Move(restingPosition, animatedPosition);
        yield return Move(animatedPosition, restingPosition);

        Destroy(gameObject);
    }


    /// <summary>
    /// Moves the coin once it comes out of the block
    /// </summary>
    /// <param name="from">The starting position</param>
    /// <param name="to">Te ending position</param>
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
    #endregion
}
