using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Обрабатывает закрытие приложения в режиме PlayMode редактора и режиме Runtime итоговой сборки.
/// </summary>
[DisallowMultipleComponent]
public sealed class ApplicationQuitHandler : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
