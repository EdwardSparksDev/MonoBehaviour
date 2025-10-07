using System.Collections;
using UnityEngine;


public class Pipe : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private AudioSource source;     //The pipe audio source

    private bool entering;          //Is the player entering the pipe
    #endregion

    #region SerializeField
    [Header("Pipe Parameters")]
    [SerializeField] private Transform connector;

    [SerializeField] private LayerMask enterLayer;
    [SerializeField] private KeyCode[] enterKeys;
    [SerializeField] private Vector3 enterDirection;
    [SerializeField] private Vector3 exitDirection = Vector2.zero;

    [SerializeField] private float scaleReduction;
    [SerializeField] private float teleportDelay;
    [SerializeField] private float animationDuration;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (connector != null && (enterLayer.value & (1 << other.gameObject.layer)) > 0)
            if (!entering && isValidKey())
                StartCoroutine(Enter(other.transform));
    }
    #endregion


    #region Methods
    /// <summary>
    /// Checks if a pressed key is in the pipe entering keys array
    /// </summary>
    /// <returns></returns>
    private bool isValidKey()
    {
        for (int i = 0; i < enterKeys.Length; i++)
        {
            if (Input.GetKey(enterKeys[i]))
                return true;
        }

        return false;
    }


    /// <summary>
    /// Method used for handling the pipe entering sequence
    /// </summary>
    /// <param name="playerTransform">Reference to the player's transform</param>
    /// <returns></returns>
    private IEnumerator Enter(Transform playerTransform)
    {
        entering = true;
        source.Play();
        ServiceLocator.Instance.Get<AudioManager>().StopBackgroundMusic();

        playerTransform.GetComponent<PlayerMovement>().enabled = false;

        Vector3 enteredPosition = transform.position + enterDirection;
        Vector3 enteredScale = Vector3.one * scaleReduction;

        yield return Move(playerTransform, enteredPosition, enteredScale);
        yield return new WaitForSeconds(teleportDelay);

        Camera.main.GetComponent<CameraController>().SetUnderground(connector.position.y < 0f);

        if (exitDirection != Vector3.zero)
        {
            playerTransform.position = connector.position - exitDirection;

            source.Play();
            yield return Move(playerTransform, connector.position + exitDirection, Vector3.one);
        }
        else
        {
            playerTransform.position = connector.position;
            playerTransform.localScale = Vector3.one;
        }

        playerTransform.GetComponent<PlayerMovement>().enabled = true;

        entering = false;
    }


    /// <summary>
    /// Method used for handling the player pipe movement
    /// </summary>
    /// <param name="playerTransform">Reference to the player's transform</param>
    /// <param name="endPosition">The final player's position</param>
    /// <param name="endScale">The final player's scale</param>
    /// <returns></returns>
    private IEnumerator Move(Transform playerTransform, Vector3 endPosition, Vector3 endScale)
    {
        float elapsed = 0f;

        Vector3 startPosition = playerTransform.position;
        Vector3 startScale = playerTransform.localScale;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;

            playerTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            playerTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;

            yield return null;
        }

        playerTransform.position = endPosition;
        playerTransform.localScale = endScale;
    }
    #endregion
}
