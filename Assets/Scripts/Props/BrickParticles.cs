using System.Collections;
using UnityEngine;


public class BrickParticles : MonoBehaviour
{
    #region Variables & Properties

    #region Local
    private Rigidbody rb;               //The particles rigidbody
    private WaitForSeconds flipDelay;   //The delay between each particle sprite flip

    private Vector3 separationVector = Quaternion.Euler(0f, 0f, -45f) * Vector3.up;     //The direction offset each particle should spread to
    #endregion

    #region SerializeField
    [Header("Particles Parameters")]
    [SerializeField] private GameObject[] go_Particles;
    [SerializeField] private float verticalPushForce;
    [SerializeField] private float extraGravity;
    [SerializeField] private float separationSpeed;
    [SerializeField] private float flipTime;
    [SerializeField] private float autodestructionTimer;
    #endregion

    #endregion


    #region Mono
    private void Awake()
    {
        flipDelay = new WaitForSeconds(flipTime);

        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.up * verticalPushForce);
        StartCoroutine(FlipCR());
        StartCoroutine(AutodestructionCR());
    }


    private void Update()
    {
        rb.AddForce(Vector3.up * extraGravity * Time.deltaTime);

        for (int i = 0; i < go_Particles.Length; i++)
            go_Particles[i].transform.localPosition += Quaternion.Euler(0f, 0f, -90f * i) * separationVector * separationSpeed * Time.deltaTime;
    }
    #endregion


    #region Methods
    /// <summary>
    /// Keeps flipping the particles sprites as long as they exist
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlipCR()
    {
        while (true)
        {
            yield return flipDelay;
            for (int i = 0; i < go_Particles.Length; i++)
                go_Particles[i].transform.localRotation *= Quaternion.Euler(0f, 0f, -90f);
        }
    }


    /// <summary>
    /// Destroys the particles after a set amount of time
    /// </summary>
    /// <returns></returns>
    private IEnumerator AutodestructionCR()
    {
        yield return new WaitForSeconds(autodestructionTimer);
        Destroy(gameObject);
    }
    #endregion
}
