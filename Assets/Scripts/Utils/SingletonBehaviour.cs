using UnityEngine;

[DisallowMultipleComponent]
public abstract class SingletonBehaviour<T> : MonoBehaviour
    where T : SingletonBehaviour<T>
{
    public static T Instance { get; private set; } = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = (T) this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    protected virtual void Init() { }
}
