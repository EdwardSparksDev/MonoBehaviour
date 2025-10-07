using UnityEngine;


public class FloatingText : MonoBehaviour
{
    #region Variables & Properties
    [Header("Animation")]
    [SerializeField] float risingSpeed;
    [SerializeField] float autodestructionTime;
    #endregion


    #region Mono
    private void Update()
    {
        autodestructionTime -= Time.deltaTime;
        if (autodestructionTime <= 0)
            Destroy(gameObject);

        transform.position += Vector3.up * risingSpeed * Time.deltaTime;
    }
    #endregion
}
