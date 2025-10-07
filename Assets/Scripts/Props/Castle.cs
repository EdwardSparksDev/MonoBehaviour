using System.Collections;
using UnityEngine;


public class Castle : MonoBehaviour, IGameService
{
    #region Variables & Properties
    [Header("Flag Parameters")]
    [SerializeField] private Transform flag;
    [SerializeField] private float raisingDistance;
    [SerializeField] private float raisingSpeed;
    #endregion


    #region Mono
    private void Start()
    {
        ServiceLocator.Instance.Register<Castle>(this);
    }
    #endregion


    #region Methods
    /// <summary>
    /// Wrapper method for raising the castle flag
    /// </summary>
    public void RaiseFlag()
    {
        StartCoroutine(RaiseFlagCR());
    }


    /// <summary>
    /// Raises the castle flag on win
    /// </summary>
    /// <returns></returns>
    private IEnumerator RaiseFlagCR()
    {
        float startingY = flag.position.y;

        while (flag.position.y < startingY + raisingDistance)
        {
            flag.position = new Vector3(flag.position.x, flag.position.y + raisingSpeed * Time.deltaTime, 0f);
            yield return null;
        }
    }
    #endregion
}
