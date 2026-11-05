using UnityEngine;
using UnityEngine.SceneManagement;

public class PasarNivel : MonoBehaviour
{
    [Header("Configuración del nivel")]
    public string nombreEscenaDestino;

    private bool nivelCambiado = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprobar si quien toca el objeto es Eira.
        Eira jugador = other.GetComponentInParent<Eira>();

        if (jugador != null && !nivelCambiado)
        {
            nivelCambiado = true;

            CambiarNivel();
        }
    }

    private void CambiarNivel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Lvl2");
    }
}
