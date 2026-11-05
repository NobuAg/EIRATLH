
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOver : MonoBehaviour
{
    [Header("Menú Game Over")]
    public GameObject menuGameOverUI;
    public GameObject primerBoton;

    [Header("Animación")]
    public CanvasGroup canvasGroupGameOver;
    public float velocidadAnimacion = 0.15f;

    // Indica que el jugador ha muerto.
    public static bool JuegoTerminado = false;

    // Mostrar el menú después de morir.
    public void MostrarGameOver()
    {
        JuegoTerminado = true;

        menuGameOverUI.SetActive(true);

        Time.timeScale = 0f;

        // Seleccionar el primer botón.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(primerBoton);
        }

        StopAllCoroutines();
        StartCoroutine(AnimarAparicion());
    }

    IEnumerator AnimarAparicion()
    {
        float tiempoPasado = 0f;

        canvasGroupGameOver.alpha = 0f;
        canvasGroupGameOver.interactable = true;
        canvasGroupGameOver.blocksRaycasts = true;

        while (tiempoPasado < velocidadAnimacion)
        {
            tiempoPasado += Time.unscaledDeltaTime;

            float porcentaje = tiempoPasado / velocidadAnimacion;

            canvasGroupGameOver.alpha =
                Mathf.Lerp(0f, 1f, porcentaje);

            yield return null;
        }

        canvasGroupGameOver.alpha = 1f;
    }

    // BOTÓN REINTENTAR
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        JuegoTerminado = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // BOTÓN MENÚ PRINCIPAL
    public void VolverMenu()
    {
        Time.timeScale = 1f;
        JuegoTerminado = false;

        SceneManager.LoadScene("Menu");
    }
}
