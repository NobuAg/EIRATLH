
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalJuego : MonoBehaviour
{
    [Header("Panel de juego completo")]
    public GameObject panelJuegoCompleto;

    private bool juegoCompletado = false;

    private void Start()
    {
        if (panelJuegoCompleto != null)
            panelJuegoCompleto.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (juegoCompletado)
            return;

        Eira jugador = other.GetComponentInParent<Eira>();

        if (jugador != null)
        {
            CompletarJuego();
        }
    }

    private void CompletarJuego()
    {
        juegoCompletado = true;

        panelJuegoCompleto.SetActive(true);

        Time.timeScale = 0f;
    }

    public void VolverMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}
