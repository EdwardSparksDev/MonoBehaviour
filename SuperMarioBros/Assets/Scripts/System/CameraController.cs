using UnityEngine;


public class CameraController : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private Transform playerTransform;

    public Vector3 Offset = Vector3.zero;
    #endregion

    #region SerializeField
    [Header("Camera Parameters")]
    [SerializeField] private float defaultHeight;
    [SerializeField] private float defaultUndergroundHeight;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }


    private void LateUpdate()
    {
        Vector3 cameraPosition = transform.position;
        cameraPosition.x = Mathf.Max(cameraPosition.x, playerTransform.position.x);
        transform.position = cameraPosition + Offset;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Sets the camera underground or default based on <underground> parameter
    /// </summary>
    /// <param name="underground">Should the camera be set underground</param>
    public void SetUnderground(bool underground)
    {
        Vector3 cameraPosition = transform.position;

        if (underground)
        {
            cameraPosition.y = defaultUndergroundHeight;
            ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.UNDERGROUND, true);
        }
        else
        {
            cameraPosition.y = defaultHeight;
            ServiceLocator.Instance.Get<AudioManager>().PlayBackgroundMusic(BackgroundMusicType.MAIN, true);
        }

        transform.position = cameraPosition;
    }
    #endregion
}
