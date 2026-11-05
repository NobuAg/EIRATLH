using UnityEngine;

public class FijarAspecto : MonoBehaviour
{
    // Define aquí el aspecto que deseas (ej. 16:9 -> x=16, y=9)
    public float targetAspectX = 16f;
    public float targetAspectY = 9f;

    void Start()
    {
        float targetAspect = targetAspectX / targetAspectY;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera camera = GetComponent<Camera>();

        // Si la pantalla es más alta que el aspecto objetivo (Pillarbox - barras a los lados)
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            camera.rect = rect;
        }
        // Si la pantalla es más ancha (Letterbox - barras arriba y abajo)
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            camera.rect = rect;
        }
    }
}
