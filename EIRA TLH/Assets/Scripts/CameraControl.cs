using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform objetivo;

    [Tooltip("Tiempo aproximado en segundos que tarda la cámara en alcanzar al jugador. Valores más bajos = seguimiento más rápido (ej: 0.1f o 0.15f).")]
    public float SmoothTime = 0.1f;

    public Vector3 Desplazamiento;

    // SmoothDamp necesita una variable privada para almacenar la velocidad interna de la cámara.
    // No la modifiques desde fuera.
    private Vector3 velocidadActual = Vector3.zero;

    private void LateUpdate()
    {
        if (objetivo == null) return;

        // 1. Calculamos la posición exacta a la que queremos llegar
        Vector3 posicionDeseada = objetivo.position + Desplazamiento;

        // 2. Usamos SmoothDamp para un movimiento matemáticamente perfecto e independiente de los FPS.
        // Reemplaza por completo al Lerp y automáticamente maneja el deltaTime internamente.
        Vector3 posicionSuavizada = Vector3.SmoothDamp(
            transform.position,
            posicionDeseada,
            ref velocidadActual,
            SmoothTime
        );

        // 3. Aplicamos la posición corregida
        transform.position = posicionSuavizada;
    }
}
