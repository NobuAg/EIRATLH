using UnityEngine;

public class BackgroundPositionOnly : MonoBehaviour
{
    public Transform cameraTransform; // Arrastra tu Main Camera aquí desde el Inspector
    private Vector3 initialOffset;

    void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        // Guarda la distancia inicial para mantener la profundidad (Z)
        initialOffset = transform.position - cameraTransform.position;
    }

    void LateUpdate()
    {
        // Solo actualiza la posición, ignorando por completo la rotación de la cámara
        Vector3 targetPosition = cameraTransform.position + initialOffset;
        transform.position = targetPosition;
    }
}
