using UnityEngine;


public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    #region Variables and Properties
    private static T instance;

    //Returns instance (if it's null, it creates and attaches the Singleton to it)
    public static T Instance { 
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.transform.SetSiblingIndex(1);
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                }
            }
            return instance;
        } 
    }
    #endregion


    #region Mono
    //Initializes singleton on game initialization by getting its reference and deletes copies
    protected virtual void Awake()
    {
        if (instance == null)
            instance = this as T;
        else if (instance != this as T)
            Destroy(gameObject);
    }
    #endregion
}
