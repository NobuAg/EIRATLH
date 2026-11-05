using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using UnityEngine.InputSystem;

public class Eira : MonoBehaviour
{
    [Header("Detección del suelo")]
    public Transform groundCheck;
    public LayerMask capaSuelo;
    public float radioSuelo = 0.15f;
    // VIDA
    private float maxVida = 100f;
    private float Vida;

    // MOVIMIENTO
    [Header("Movimiento")]
    public float WalkSpeed = 8f;
    public float JumpSpeed = 6f;
    public Rigidbody2D rb2D;
    public SpriteRenderer SpriteRen;
    private float inputX;
    private bool estaEnElSuelo = true;
    private Animator animator;
    [Header("Movimiento en rampas")]
    public float alturaInicioRaycast = 0.25f;
    public float distanciaRaycastSuelo = 0.6f;

    private bool recibiendoDano = false;
    [Header("Sistema de muerte")]
    public bool estaMuerta = false;
    public ParticleSystem efectoMuerte;
    public GameObject panelGameOver;
    public float tiempoAntesGameOver = 1.5f;
    [Header("Sistema Game Over")]
    public GameOver menuGameOver;
    [Header("Invulnerabilidad")]
    public float duracionInvulnerabilidad = 2f;

    private bool invulnerable = false;
    private float temporizadorInvulnerabilidad = 0f;

    [Header("Efecto de invulnerabilidad")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public float intervaloParpadeo = 0.5f;

    private float temporizadorParpadeo = 0f;

    // ATAQUE CON ARCO

    [Header("Ataque con arco")]
    [SerializeField] private float tiempoSinDisparar = 3f;
    [SerializeField] private float duracionAnimacionDisparo = 0.5f;
    private bool arcoPreparado = false;
    private bool arcoListoParaDisparar = false;
    private bool disparando = false;
    private bool flechaCreadaEsteDisparo = false;
    private float temporizadorArco = 0f;
    private float temporizadorDisparo = 0f;

    [Header("Sonidos")]
    public AudioSource audioSource;

    public AudioClip sonidoPasos;
    public AudioClip sonidoAtaque;
    public AudioClip sonidoSalto;
    public AudioClip sonidoDaño;

    [Header("Configuración de pasos")]
    public float intervaloPasos = 0.35f;

    private float temporizadorPasos = 0f;

    // DISPARO DE FLECHAS

    [Header("Disparo de flechas")]
    [SerializeField] private GameObject prefabFlecha;
    [SerializeField] private Transform controladorDisparo;

    // INICIALIZACIÓN
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        Vida = maxVida;

        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();

        if (SpriteRen == null)
            SpriteRen = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();

        // Inicializar parámetros del Animator.
        animator.SetBool("Running", false);
        animator.SetBool("EstaAtacando", false);
        animator.SetBool("AttackIni", false);
        animator.SetBool("AttackStart", false);
        animator.SetBool("ReturnToIdle", false);
    }

    // UPDATE
    private void Update()
    {
        ComprobarSuelo();

        ActualizarAnimacionDano();
        // Esto controla Muerte
        if (estaMuerta)
            return;
        // Controlar duración de invulnerabilidad.
        // CONTROLAR DURACIÓN DE INVULNERABILIDAD
        if (invulnerable)
        {
            temporizadorInvulnerabilidad -= Time.deltaTime;

            if (temporizadorInvulnerabilidad <= 0f)
            {
                invulnerable = false;

                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;

                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }

                Debug.Log("Eira ya puede recibir daño nuevamente.");
            }
        }
        if (Keyboard.current == null)
            return;
        if (recibiendoDano)
        {
            inputX = 0f;
            return;
        }

        ActualizarAnimacionSalto();

        // MOVIMIENTO HORIZONTAL
        inputX = 0f;
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            inputX = 1f;

            SpriteRen.flipX = false;
        }
        else if (Keyboard.current.leftArrowKey.isPressed)
        {
            inputX = -1f;

            SpriteRen.flipX = true;
        }
        if (Mathf.Abs(inputX) > 0.01f && estaEnElSuelo && !arcoPreparado && !disparando)
        {
            temporizadorPasos += Time.deltaTime;

            if (temporizadorPasos >= intervaloPasos)
            {
                if (sonidoPasos != null && !audioSource.isPlaying)
                {
                    audioSource.clip = sonidoPasos;
                    audioSource.loop = true;
                    audioSource.Play();
                }

                temporizadorPasos = 0f;
            }
        }
        else
        {
            temporizadorPasos = 0f;

            // Detener los pasos inmediatamente.
            if (audioSource.clip == sonidoPasos && audioSource.isPlaying)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = null;
            }
        }
        // BLOQUEAR MOVIMIENTO DURANTE EL ARCO
        if (arcoPreparado || disparando)
        {
            inputX = 0f;
        }
        // ANIMACIÓN RUNNING
        animator.SetBool("Running", inputX != 0f);

        // SALTO
        if (Keyboard.current.xKey.wasPressedThisFrame
            && !Pause.BloquearInputFrame)
        {
            if (estaEnElSuelo && !arcoPreparado && !disparando)
            {
                Saltar();
            }
        }
        // CONTROL DEL ARCO
        if (Keyboard.current.zKey.wasPressedThisFrame
            && !Pause.BloquearInputFrame)
        {
            // Primera pulsación: preparar arco.
            if (!arcoPreparado && !disparando)
            {
                // Eira debe estar quieta para preparar el arco.
                if (inputX == 0f)
                {
                    PrepararArco();
                }
            }
            // Segunda pulsación: disparar flecha.
            else if (arcoPreparado
                     && arcoListoParaDisparar
                     && !disparando)
            {
                DispararUnaFlecha();
            }
        }
        // TEMPORIZADOR PARA GUARDAR EL ARCO
        if (arcoPreparado && !disparando)
        {
            temporizadorArco += Time.deltaTime;

            if (temporizadorArco >= tiempoSinDisparar)
            {
                GuardarArco();
            }
        }
        // DURACIÓN DE LA ANIMACIÓN DE DISPARO
        if (disparando)
        {
            temporizadorDisparo += Time.deltaTime;

            if (temporizadorDisparo >= duracionAnimacionDisparo)
            {
                FinalizarDisparo();
            }
        }
    }
    // FÍSICA Y MOVIMIENTO
    private void ComprobarSuelo()
    {
        if (groundCheck == null)
            return;

        bool tocandoSuelo = Physics2D.OverlapCircle(
            groundCheck.position,
            radioSuelo,
            capaSuelo
        );

        // Evitar detectar el suelo durante el ascenso del salto.
        if (rb2D != null && rb2D.linearVelocity.y > 0.1f)
        {
            estaEnElSuelo = false;
            return;
        }

        estaEnElSuelo = tocandoSuelo;
    }
    private void FixedUpdate()
    {
        if (estaMuerta || rb2D == null)
            return;

        if (recibiendoDano)
        {
            rb2D.linearVelocity = new Vector2(
                0f,
                rb2D.linearVelocity.y
            );
            return;
        }

        float velocidadX = 0f;

        if (!arcoPreparado && !disparando)
        {
            velocidadX = inputX * WalkSpeed;
        }

        // Si Eira está quieta, detener solamente el movimiento horizontal.
        if (Mathf.Abs(velocidadX) < 0.01f)
        {
            rb2D.linearVelocity = new Vector2(
                0f,
                rb2D.linearVelocity.y
            );
            return;
        }

        // Adaptar el movimiento a la pendiente únicamente en el suelo.
        if (estaEnElSuelo && groundCheck != null)
        {
            Vector2 origenRaycast =
                (Vector2)groundCheck.position +
                Vector2.up * alturaInicioRaycast;

            RaycastHit2D impacto = Physics2D.Raycast(
                origenRaycast,
                Vector2.down,
                distanciaRaycastSuelo,
                capaSuelo
            );

            if (impacto.collider != null && impacto.normal.y > 0.2f)
            {
                Vector2 direccionPendiente = new Vector2(
                    impacto.normal.y,
                    -impacto.normal.x
                ).normalized;

                if (velocidadX < 0f)
                    direccionPendiente = -direccionPendiente;

                rb2D.linearVelocity =
                    direccionPendiente * Mathf.Abs(velocidadX);

                return;
            }
        }

        // Movimiento horizontal normal, tanto en el aire como en terreno plano.
        rb2D.linearVelocity = new Vector2(
            velocidadX,
            rb2D.linearVelocity.y
        );
    }
    // PREPARAR ARCO
    private void PrepararArco()
    {
        arcoPreparado = true;
        arcoListoParaDisparar = false;
        disparando = false;
        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        animator.SetBool("Running", false);
        animator.SetBool("ReturnToIdle", false);
        animator.SetBool("AttackStart", false);
        animator.SetBool("AttackIni", true);
        animator.SetBool("EstaAtacando", true);
    }
    // FINALIZAR PREPARACIÓN DEL ARCO
    // ANIMATION EVENT: EiraAttackIntro
    public void TerminarPreparacionArco()
    {
        if (arcoPreparado && !disparando)
        {
            arcoListoParaDisparar = true;
        }
    }
    // DISPARAR UNA FLECHA
    private void DispararUnaFlecha()
    {
        if (!arcoPreparado || !arcoListoParaDisparar || disparando)
            return;
        if (prefabFlecha == null || controladorDisparo == null)
        {
            Debug.LogWarning(
                "Asigna el prefab de la flecha y ControladorDisparo en Eira."
            );

            return;
        }
        // Comenzar secuencia de disparo.
        disparando = true;
        arcoListoParaDisparar = false;
        flechaCreadaEsteDisparo = false;
        temporizadorDisparo = 0f;
        temporizadorArco = 0f;


        // Cambiar parámetros del Animator.
        animator.SetBool("ReturnToIdle", false);
        animator.SetBool("AttackIni", false);
        animator.SetBool("AttackStart", true);
        animator.SetBool("EstaAtacando", true);
    }
    // LANZAR FLECHA ANIMATION EVENT: EiraAttackFinish
    public void LanzarFlecha()
    {
        if (!disparando || flechaCreadaEsteDisparo)
            return;

        if (prefabFlecha == null || controladorDisparo == null)
            return;
        if (sonidoAtaque != null)
            audioSource.PlayOneShot(sonidoAtaque);

        // Determinar dirección.
        bool miraDerecha = !SpriteRen.flipX;

        // Calcular posición de salida.
        Vector3 posicionLocal = controladorDisparo.localPosition;

        posicionLocal.x = Mathf.Abs(posicionLocal.x)
                          * (miraDerecha ? 1f : -1f);

        Vector3 posicionDisparo =
            transform.TransformPoint(posicionLocal);


        // Crear flecha.
        GameObject nuevaFlecha = Instantiate(
            prefabFlecha,
            posicionDisparo,
            Quaternion.identity
        );

        // Inicializar dirección del proyectil.
        Flecha flecha = nuevaFlecha.GetComponent<Flecha>();

        if (flecha != null)
        {
            flecha.Inicializar(miraDerecha);
        }
        // Evitar crear más de una flecha por disparo.
        flechaCreadaEsteDisparo = true;
    }
    // FINALIZAR DISPARO
    public void FinalizarDisparo()
    {
        disparando = false;
        temporizadorDisparo = 0f;
        temporizadorArco = 0f;

        // Mantener arco preparado.
        arcoPreparado = true;
        arcoListoParaDisparar = true;

        // Volver al estado de arco preparado.
        animator.SetBool("AttackStart", false);
        animator.SetBool("AttackIni", false);
        animator.SetBool("EstaAtacando", true);
        animator.SetBool("ReturnToIdle", false);
    }
    // GUARDAR ARCO
    private void GuardarArco()
    {
        arcoPreparado = false;
        arcoListoParaDisparar = false;
        disparando = false;

        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        // Restablecer parámetros del Animator.
        animator.SetBool("AttackIni", false);
        animator.SetBool("AttackStart", false);
        animator.SetBool("EstaAtacando", false);
        animator.SetBool("ReturnToIdle", true);

        Debug.Log("Eira terminó el ataque y puede volver a moverse.");
    }
    // SALTO
    private void Saltar()
    {
        if (!estaEnElSuelo)
            return;

        // Eliminar la velocidad vertical anterior.
        rb2D.linearVelocity = new Vector2(
            rb2D.linearVelocity.x,
            0f
        );

        // Aplicar el impulso del salto.
        rb2D.AddForce(
            Vector2.up * JumpSpeed,
            ForceMode2D.Impulse
        );

        if (sonidoSalto != null)
            audioSource.PlayOneShot(sonidoSalto);

        estaEnElSuelo = false;

        animator.SetBool("EnElAire", true);
        animator.SetBool("Cayendo", false);
    }
    private void ActualizarAnimacionSalto()
    {
        if (estaEnElSuelo)
        {
            animator.SetBool("EnElAire", false);
            animator.SetBool("Cayendo", false);
            return;
        }

        if (rb2D.linearVelocity.y < -0.1f)
        {
            animator.SetBool("Cayendo", true);
        }
        else
        {
            animator.SetBool("Cayendo", false);
        }
    }
    // RECIBIR DAÑO
    public void RecibirDano(float cantidad)
    {
        // Ignorar golpes durante la invulnerabilidad.
        if (invulnerable)
            return;

        // Descontar vida.
        Vida -= cantidad;
        if (Vida <= 0)
        {
            Vida = 0;
            Morir();
            return;
        }

        Debug.Log("Eira recibió daño. Vida: " + Vida);

        // Activar invulnerabilidad.
        invulnerable = true;
        temporizadorInvulnerabilidad = duracionInvulnerabilidad;

        temporizadorParpadeo = 0f;

        temporizadorParpadeo = 0f;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        // Activar bloqueo de controles.
        recibiendoDano = true;

        inputX = 0f;

        rb2D.linearVelocity = new Vector2(0f, rb2D.linearVelocity.y);

        // Detener sonido de pasos.
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        // Reproducir sonido de daño.
        if (audioSource != null && sonidoDaño != null)
            audioSource.PlayOneShot(sonidoDaño);

        // Cancelar acciones del arco.
        arcoPreparado = false;
        arcoListoParaDisparar = false;
        disparando = false;

        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        // Restablecer parámetros del Animator.
        animator.SetBool("EstaAtacando", false);
        animator.SetBool("AttackIni", false);
        animator.SetBool("AttackStart", false);
        animator.SetBool("ReturnToIdle", false);
        animator.SetBool("Running", false);

        // Reproducir daño.
        animator.SetTrigger("Daño");
    }
    public void FinalizarAnimacionDano()
    {
        recibiendoDano = false;

        inputX = 0f;

        Debug.Log("Eira terminó la animación de daño.");
    }
    void ActualizarAnimacionDano()
    {
        if (!recibiendoDano)
            return;

        AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);

        if (estado.IsName("EiraHurt") && estado.normalizedTime >= 1f)
        {
            recibiendoDano = false;
            inputX = 0f;

            Debug.Log("Animación de daño finalizada.");
        }
    }
    void ActualizarParpadeo()
    {
        if (!invulnerable || spriteRenderer == null)
            return;

        temporizadorParpadeo -= Time.deltaTime;

        if (temporizadorParpadeo <= 0f)
        {
            Color color = spriteRenderer.color;

            color.a = color.a > 0.5f ? 0.2f : 1f;

            spriteRenderer.color = color;

            temporizadorParpadeo = intervaloParpadeo;
        }
    }
    public void Morir()
    {
        // Evitar que la muerte se ejecute varias veces.
        if (estaMuerta)
            return;

        estaMuerta = true;

        // Detener movimiento.
        inputX = 0f;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.simulated = false;

        // Detener sonidos.
        if (audioSource != null)
            audioSource.Stop();

        // Generar efecto visual.
        // GENERAR EFECTO DE MUERTE
        if (efectoMuerte != null)
        {
            ParticleSystem efecto = Instantiate(
                efectoMuerte,
                transform.position,
                Quaternion.identity
            );

            efecto.Play();
        }
        // Ocultar a Eira.
        if (SpriteRen != null)
            SpriteRen.enabled = false;

        // Desactivar colisiones.
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        StartCoroutine(MostrarGameOver());
    }
        private IEnumerator MostrarGameOver()
        {
            yield return new WaitForSeconds(tiempoAntesGameOver);

            if (menuGameOver != null)
            {
                menuGameOver.MostrarGameOver();
            }
        }




    // Muerte por caída al vacío.
    public void MorirPorCaida()
    {
        Morir();
    }
}