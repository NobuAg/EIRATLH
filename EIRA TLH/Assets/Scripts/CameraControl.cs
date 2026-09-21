using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform objetivo;

    [Tooltip("Sube este valor en el Inspector (ej. entre 5 y 15) para que siga más rápido al jugador.")]
    public float CameraSpeed = 10f;
    public Vector3 Desplazamiento;

    // Regresamos a LateUpdate para que la cámara renderice justo después 
    // de que la interpolación del Rigidbody2D mueva visualmente al personaje.
    private void LateUpdate()
    {
        if (objetivo == null) return;

        Vector3 posicionDeseada = objetivo.position + Desplazamiento;

        // Al estar en LateUpdate, DEBEMOS usar Time.deltaTime.
        // Además, usamos Mathf.Clamp01 para asegurar que el cálculo matemático 
        // nunca supere el 100% de la distancia en pantallas de bajos FPS, evitando tirones.
        float suavizado = Mathf.Clamp01(CameraSpeed * Time.deltaTime);
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);

        transform.position = posicionSuavizada;
    }
}
