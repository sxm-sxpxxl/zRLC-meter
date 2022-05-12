using UnityEngine;

/// <summary>
/// Компонент, обеспечивающий поведение Singleton'a,
/// т.е. объекта существующего в единственном экземпляре на протяжение всего цикла жизни программы
/// и предоставляющий централизованную точку доступа всем заинтересованным.
/// Используется в связке с InputDeviceListener и OutputDeviceGenerator, чтобы не прокидывать лишние зависимости
/// в обращающихся к ним компонентах.
/// В общем такой подход не рекомендуется, но в силу простоты организации нашей программы, его использование выглядит вполне удачным.
/// </summary>
/// <typeparam name="T"></typeparam>
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
