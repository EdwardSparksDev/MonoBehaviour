using UnityEngine;


public class DeathBarrier : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private AudioSource source;     //The barrier audio source
    #endregion

    #region SerializeField
    [Header("Barrier Parameters")]
    [SerializeField] private LayerMask delayedDestroyMask;
    [SerializeField] private float levelResetDelay;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((delayedDestroyMask.value & (1 << other.gameObject.layer)) > 0)
        {
            source.Play();
            ServiceLocator.Instance.Get<AudioManager>().StopBackgroundMusic();
            other.gameObject.SetActive(false);
            ServiceLocator.Instance.Get<GameManager>().ResetLevel(levelResetDelay);
        }
        else
            Destroy(other.gameObject);
    }
    #endregion
}
