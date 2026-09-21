using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vaelith : MonoBehaviour
{
    [SerializeField] private float VidaBoss = 1000;
    private float MaxVidaBoss = 1000;
    public Rigidbody2D rb2D;
    public enum BossPhase { Fase1, Fase2, Fase3 };
    public BossPhase faseActual;
    [System.Serializable]
    public struct AtaqueConfig
    {
        public string nombreAtaque;
        public int pesoBase;             // Mayor peso = más probabilidad de salir
        public float distanciaMinima;    // Distancia mínima para poder usarlo
        public float distanciaMaxima;    // Distancia máxima para poder usarlo
        public BossPhase faseMinima;     // A partir de qué fase se puede usar
        public float cooldown;           // Tiempo de espera entre usos de este ataque específico
        [HideInInspector] public float ultimoTiempoUso; // Control interno de cooldown
    }
    public List<AtaqueConfig> listaAtaques;
    public Transform jugador; // Arrastra al jugador aquí en el Inspector
    public float TiempoEntreAtaques = 2f;
    private bool estaAtacando = false;

    private void Start()
    {
        VidaBoss = MaxVidaBoss;
        rb2D = GetComponent<Rigidbody2D>();

        // Inicializar los tiempos de cooldown
        for (int i = 0; i < listaAtaques.Count; i++)
        {
            var ataque = listaAtaques[i];
            ataque.ultimoTiempoUso = -999f;
            listaAtaques[i] = ataque;
        }

        StartCoroutine(RutinaDeAtaques());
    }
    IEnumerator RutinaDeAtaques()
    {
        while (true)
        {
            if (!estaAtacando && jugador != null)
            {
                estaAtacando = true;
                SeleccionarYEjecutarAtaque();
                yield return new WaitForSeconds(TiempoEntreAtaques);
                estaAtacando = false;
            }
            yield return null;
        }
    }
    void SeleccionarYEjecutarAtaque()
    {
        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);
        List<int> indicesAtaquesValidos = new List<int>();
        int sumaPesosTotal = 0;

        // 1. Filtrar qué ataques cumplen las condiciones actuales
        for (int i = 0; i < listaAtaques.Count; i++)
        {
            AtaqueConfig ataque = listaAtaques[i];

            bool cumpleDistancia = distanciaAlJugador >= ataque.distanciaMinima && distanciaAlJugador <= ataque.distanciaMaxima;
            bool cumpleFase = faseActual >= ataque.faseMinima;
            bool cumpleCooldown = Time.time >= ataque.ultimoTiempoUso + ataque.cooldown;

            if (cumpleDistancia && cumpleFase && cumpleCooldown)
            {
                indicesAtaquesValidos.Add(i);
                sumaPesosTotal += ataque.pesoBase;
            }
        }

        // Si ningún ataque cumple las condiciones, salir o hacer un ataque por defecto
        if (indicesAtaquesValidos.Count == 0)
        {
            Debug.LogWarning("El jefe no encontró ningún ataque válido para las condiciones actuales.");
            return;
        }

        // 2. Selección aleatoria basada en pesos (Ruleta de selección)
        int valorAleatorio = Random.Range(0, sumaPesosTotal);
        int indiceAtaqueElegido = indicesAtaquesValidos[0];
        int sumaActual = 0;

        foreach (int index in indicesAtaquesValidos)
        {
            sumaActual += listaAtaques[index].pesoBase;
            if (valorAleatorio < sumaActual)
            {
                indiceAtaqueElegido = index;
                break;
            }
        }

        // 3. Registrar el uso y ejecutar el ataque elegido
        AtaqueConfig ataqueSeleccionado = listaAtaques[indiceAtaqueElegido];
        ataqueSeleccionado.ultimoTiempoUso = Time.time;
        listaAtaques[indiceAtaqueElegido] = ataqueSeleccionado; // Guardar cambio de cooldown

        LanzarHabilidad(ataqueSeleccionado.nombreAtaque);
    }

    void LanzarHabilidad(string nombreAtaque)
    {
        Debug.Log($"[JEFE] Ejecutando: {nombreAtaque}");

        // Aquí rediriges a tus métodos reales según el nombre configurado
        switch (nombreAtaque)
        {
            case "Iceball":
                // Código o animación de ataque cuerpo a cuerpo
                break;
            case "Veyrglass attack":
                // Código para instanciar un proyectil
                break;
            case "ShakeIce":
                // Código para saltar sobre el jugador
                break;
        }
    }
    public void RecibirDaño(float cantidad)
    {
        VidaBoss -= cantidad;
        if (VidaBoss <= 0)
        {
            Morir();
        }
    }
    void Morir() { Debug.Log("El jefe ha sido derrotado."); Destroy(gameObject); }
}


