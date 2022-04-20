using UnityEditor;
using UnityEngine;

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
