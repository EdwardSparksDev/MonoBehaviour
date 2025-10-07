using System.Collections;
using UnityEngine;


public class FireworksMortar : MonoBehaviour, IGameService
{
    #region Variables & Properties
    [Header("Fireworks")]
    [SerializeField] private int awardedScorePerFirework;

    [SerializeField] private GameObject firework;
    [SerializeField] private Vector3 fireworksOffset;
    [SerializeField] private float fireworksRange;
    [SerializeField] private float fireworksDelay;
    [SerializeField] private int[] fireworkTriggers;
    #endregion


    #region Mono
    private void Start()
    {
        ServiceLocator.Instance.Register<FireworksMortar>(this);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Tries to fire fireworks burst depending on last timer digit
    /// </summary>
    /// <param name="lastTimerDigit">The last timer digit when grabbing the flag</param>
    /// <returns></returns>
    public IEnumerator FireFireworksCR(int lastTimerDigit)
    {
        //Check if the last digit is valid
        bool triggerFireworks = false;
        for (int i = 0; i < fireworkTriggers.Length && !triggerFireworks; i++)
        {
            if (fireworkTriggers[i] == lastTimerDigit)
                triggerFireworks = true;
        }

        //Fire <lastTimerDigit> fireworks
        if (triggerFireworks)
        {
            WaitForSeconds delay = new WaitForSeconds(fireworksDelay);

            for (int i = 0; i < lastTimerDigit; i++)
            {
                yield return delay;
                Instantiate(firework, transform.position + fireworksOffset
                    + new Vector3(Random.Range(-fireworksRange, fireworksRange), Random.Range(-fireworksRange, fireworksRange), 0f), Quaternion.identity);
                ServiceLocator.Instance.Get<GameManager>().AddScore(awardedScorePerFirework);
            }
        }
    }
    #endregion
}
