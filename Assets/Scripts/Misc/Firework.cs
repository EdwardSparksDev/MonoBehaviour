using System.Collections;
using UnityEngine;


public class Firework : MonoBehaviour
{
    #region Variables & Properties
    [Header("Autodestruction")]
    [SerializeField] private float hideTimer;
    [SerializeField] private float autodestructionTimer;
    #endregion


    #region Mono
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(hideTimer);
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(autodestructionTimer - hideTimer);
        Destroy(gameObject);
    }
    #endregion
}
