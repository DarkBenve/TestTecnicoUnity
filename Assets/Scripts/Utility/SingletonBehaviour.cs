using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
