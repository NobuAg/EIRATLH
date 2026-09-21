using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections; // Necesario para usar Corrutinas

public class Pause : MonoBehaviour
{
    public GameObject menuPausaUI;
    public GameObject primerBoton;
    private bool estaPausado = false;
    public CanvasGroup canvasGroupPausa; // Arrastra el CanvasGroup del panel aquí
    public float velocidadAnimacion = 0.15f; // Tiempo que tarda en abrirse

    // Esta variable global avisa al personaje si debe ignorar la tecla X en este frame
    public static bool BloquearInputFrame = false;

    void Update()
    {
        // 1. Abrir o cerrar el menú solo con la tecla P
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (estaPausado)
            {
                ReanudarMenu();
            }
            else
            {
                PausarMenu();
            }
        }

        // 2. Si está pausado y presionas X, ejecuta la opción seleccionada
        if (estaPausado && Input.GetKeyDown(KeyCode.X))
        {
            ActivarBotonSeleccionado();
        }
    }

    void ActivarBotonSeleccionado()
    {
        GameObject objetoSeleccionado = EventSystem.current.currentSelectedGameObject;

        if (objetoSeleccionado != null)
        {
            Button boton = objetoSeleccionado.GetComponent<Button>();

            if (boton != null)
            {
                // Si el botón que vas a pulsar es "Continuar", usamos la corrutina para evitar el salto
                if (objetoSeleccionado.name == "Continuar")
                {
                    StartCoroutine(ReanudarConEspera());
                }
                else
                {
                    boton.onClick.Invoke(); // Para el botón Salir u otros
                }
            }
        }
    }

    // Cerramos el menú normalmente si se presiona la P otra vez
    public void ReanudarMenu()
    {
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        estaPausado = false;
    }

    // Corrutina que bloquea el salto del personaje al presionar X sobre "Continuar"
    IEnumerator ReanudarConEspera()
    {
        BloquearInputFrame = true; // Activa el escudo anti-salto

        menuPausaUI.SetActive(false);
        Time.timeScale = 1f;
        estaPausado = false;

        // Espera a que termine por completo el frame actual
        yield return new WaitForEndOfFrame();

        BloquearInputFrame = false; // Desactiva el escudo
    }

    void PausarMenu()
    {
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f;
        estaPausado = true;

        // Detiene cualquier animación previa e inicia la de aparición
        StopAllCoroutines();
        StartCoroutine(AnimarAparicion());

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(primerBoton);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }

    IEnumerator AnimarAparicion()
    {
        float tiempoPasado = 0f;

        // Configuración inicial: Completamente invisible
        canvasGroupPausa.alpha = 0f;

        while (tiempoPasado < velocidadAnimacion)
        {
            // Avanza el tiempo real ignorando la pausa del juego
            tiempoPasado += Time.unscaledDeltaTime;
            float porcentaje = tiempoPasado / velocidadAnimacion;

            // Cambia la opacidad de forma lineal y suave de 0 a 1
            canvasGroupPausa.alpha = Mathf.Lerp(0f, 1f, porcentaje);

            yield return null; // Espera al siguiente frame
        }

        // Asegura que al terminar quede 100% visible
        canvasGroupPausa.alpha = 1f;
    }

}
