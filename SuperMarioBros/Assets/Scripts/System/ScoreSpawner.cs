using TMPro;
using UnityEngine;


public class ScoreSpawner : MonoBehaviour, IGameService
{
    #region Variables & Properties
    [Header("Spawner References")]
    [SerializeField] private GameObject scoreText;
    #endregion


    #region Mono
    private void Start()
    {
        ServiceLocator.Instance.Register<ScoreSpawner>(this);
    }
    #endregion


    #region Methods
    public void SpawnScore(Vector3 position, int value)
    {
        Instantiate(scoreText, position + Vector3.up * .5f, Quaternion.identity).GetComponentInChildren<TMP_Text>().text = value.ToString();
        ServiceLocator.Instance.Get<GameManager>().AddScore(value);
    }
    #endregion
}
