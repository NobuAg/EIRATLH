
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Referencia al jugador")]
    public Eira jugador;

    [Header("Altura de muerte")]
    public float alturaMuerte = -15f;

    private bool muerteActivada = false;

    private void Update()
    {
        if (jugador == null || muerteActivada)
            return;

        // Comprueba si Eira cayó por debajo del límite.
        if (jugador.transform.position.y <= alturaMuerte)
        {
            muerteActivada = true;
            jugador.MorirPorCaida();
        }
    }
}
