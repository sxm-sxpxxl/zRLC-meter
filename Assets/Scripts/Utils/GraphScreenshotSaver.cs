using UnityEditor;
using UnityEngine;

/// <summary>
/// Получает изображение из камеры по запросу и сохраняет как картинку в формате PNG.
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Camera))]
public sealed class GraphScreenshotSaver : MonoBehaviour
{
    private Camera _graphCamera;
    private bool takeScreenshotOnNextFrame;
    
    private void Awake()
    {
        _graphCamera = GetComponent<Camera>();
    }

    private void OnPostRender()
    {
        if (takeScreenshotOnNextFrame == false)
        {
            return;
        }

        takeScreenshotOnNextFrame = false;
        
        RenderTexture renderTexture = _graphCamera.targetTexture;
        Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        renderResult.ReadPixels(rect, 0, 0);

        string path = EditorUtility.SaveFilePanel("Save graph as PNG image", string.Empty, "impedance-graph", "png");
        if (path.Length == 0)
        {
            return;
        }
        
        byte[] byteArray = renderResult.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, byteArray);
        
        Debug.Log($"<color=yellow>Graph chart image was saved on path</color>: {path}");

        RenderTexture.ReleaseTemporary(renderTexture);
        _graphCamera.targetTexture = null;
    }

    /// <summary>
    /// Сделать скриншот чарта на следующем кадре камеры.
    /// </summary>
    public void TakeScreenshot()
    {
        _graphCamera.targetTexture = RenderTexture.GetTemporary(_graphCamera.pixelWidth, _graphCamera.pixelHeight, 16);
        takeScreenshotOnNextFrame = true;
    }
}
