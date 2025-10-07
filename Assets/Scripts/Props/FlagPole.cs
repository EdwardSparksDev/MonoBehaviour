using System.Collections;
using UnityEngine;


public class FlagPole : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private AudioSource source;     //The flag audio source

    private int scoreIndex = 0;     //The selected awarded score index
    #endregion

    #region SerializeField
    [Header("Flag Parameters and References")]
    [SerializeField] private LayerMask triggerMask;

    [SerializeField] private Transform flag;
    [SerializeField] private Transform poleBottom;

    [SerializeField] private float speed;
    [SerializeField] private float acceptableDistance;
    [SerializeField] private int[] awardedScores;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((triggerMask.value & (1 << other.gameObject.layer)) > 0)
        {
            source.Play();
            CalculateScore(other.transform.position.y);

            StartCoroutine(MoveTo(flag, poleBottom.position));
            StartCoroutine(LevelCompleteSequence(other.transform));
        }
    }
    #endregion


    #region Methods
    /// <summary>
    /// The sequence played once the flag is reached
    /// </summary>
    /// <param name="playerTransform">Reference to the player's transform</param>
    /// <returns></returns>
    private IEnumerator LevelCompleteSequence(Transform playerTransform)
    {
        ServiceLocator.Instance.Get<GameManager>().StopTimer();

        PlayerMovement playerMovement = playerTransform.GetComponent<PlayerMovement>();
        SpriteRenderer playerSpriteRenderer = playerTransform.GetComponent<PlayerState>().ActiveRenderer.SpriteRenderer;
        playerMovement.enabled = false;
        playerMovement.HoldingFlag = true;

        ServiceLocator.Instance.Get<AudioManager>().StopBackgroundMusic();

        yield return MoveTo(playerTransform, poleBottom.position);
        ServiceLocator.Instance.Get<ScoreSpawner>().SpawnScore(playerTransform.position, awardedScores[scoreIndex]);

        playerSpriteRenderer.flipX = true;
        playerTransform.position += Vector3.right;
        Camera.main.GetComponent<CameraController>().Offset = Vector3.left;
        yield return new WaitForSeconds(.3f);

        playerSpriteRenderer.flipX = false;
        playerMovement.HoldingFlag = false;
        ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.STAGE_CLEAR, false);

        yield return MoveTo(playerTransform, playerTransform.position + Vector3.right + Vector3.down);
        yield return MoveTo(playerTransform, ServiceLocator.Instance.Get<FireworksMortar>().transform.position);

        playerTransform.gameObject.SetActive(false);

        StartCoroutine(ServiceLocator.Instance.Get<GameManager>().CalculateFinalScore());
    }


    /// <summary>
    /// Moves an object to a set destination
    /// </summary>
    /// <param name="subject">The object to be moved</param>
    /// <param name="destination">The destination to be move to</param>
    /// <returns></returns>
    private IEnumerator MoveTo(Transform subject, Vector3 destination)
    {
        while ((subject.position - destination).sqrMagnitude > Mathf.Pow(acceptableDistance, 2))
        {
            subject.position = Vector3.MoveTowards(subject.position, destination, speed * Time.deltaTime);
            yield return null;
        }

        subject.position = destination;
    }


    /// <summary>
    /// Calculates the final flag score based on grabbed point
    /// </summary>
    /// <param name="y"></param>
    private void CalculateScore(float y)
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        float height = transform.position.y + .5f;
        float heightIncrement = col.size.y / awardedScores.Length;

        while (y > height + heightIncrement)
        {
            height += heightIncrement;
            if (scoreIndex < awardedScores.Length - 1)
                scoreIndex++;
        }
    }
    #endregion
}
