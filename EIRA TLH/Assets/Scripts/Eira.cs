using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Eira : MonoBehaviour
{
    [Header("Detección del suelo")]
    public Transform groundCheck;
    public LayerMask capaSuelo;
    public float radioSuelo = 0.15f;

    [Header("Movimiento")]
    public float WalkSpeed = 8f;
    public float JumpSpeed = 6f;
    public Rigidbody2D rb2D;
    public SpriteRenderer SpriteRen;
    public float alturaInicioRaycast = 0.25f;
    public float distanciaRaycastSuelo = 0.6f;
    private float inputX;
    private bool estaEnElSuelo = true;
    private Animator animator;

    [Header("Vida y daño")]
    [SerializeField] private float maxVida = 100f;
    private float Vida;
    [SerializeField] private Slider barraVida;
    private bool recibiendoDano;
    [Header("Invulnerabilidad")]
    public float duracionInvulnerabilidad = 2f;
    private bool invulnerable;
    private float temporizadorInvulnerabilidad;
    [Header("Efecto visual de daño")]
    [SerializeField] private Color colorDanio = Color.red;
    [SerializeField] private float duracionColorDanio = 0.2f;
    private float temporizadorColorDanio;
    private Color colorOriginal;
    [Header("Sistema de muerte")]
    public bool estaMuerta;
    public ParticleSystem efectoMuerte;
    public GameObject panelGameOver;
    public float tiempoAntesGameOver = 1.5f;
    public GameOver menuGameOver;
    [Header("Ataque con arco")]
    [SerializeField] private float tiempoSinDisparar = 3f;
    [SerializeField] private float duracionAnimacionDisparo = 0.5f;
    [SerializeField] private GameObject prefabFlecha;
    [SerializeField] private Transform controladorDisparo;
    private bool arcoPreparado;
    private bool arcoListoParaDisparar;
    private bool disparando;
    private bool flechaCreadaEsteDisparo;
    private float temporizadorArco;
    private float temporizadorDisparo;
    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip sonidoPasos;
    public AudioClip sonidoAtaque;
    public AudioClip sonidoSalto;
    public AudioClip sonidoDaño;
    [Header("Configuración de pasos")]
    public float intervaloPasos = 0.35f;
    private float temporizadorPasos;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();

        if (SpriteRen == null)
            SpriteRen = GetComponent<SpriteRenderer>();

        if (SpriteRen != null)
            colorOriginal = SpriteRen.color;

        animator = GetComponent<Animator>();
        Vida = maxVida;
        if (barraVida != null)
        {
            barraVida.maxValue = maxVida;
            barraVida.value = Vida;
        }

        if (animator != null)
        {
            animator.SetBool("Running", false);
            animator.SetBool("EstaAtacando", false);
            animator.SetBool("AttackIni", false);
            animator.SetBool("AttackStart", false);
            animator.SetBool("ReturnToIdle", false);
        }
    }
    private void Update()
    {
        ComprobarSuelo();
        ActualizarAnimacionDano();
        ActualizarColorDanio();
        ActualizarInvulnerabilidad();

        if (estaMuerta || Keyboard.current == null)
            return;
        if (recibiendoDano)
        {
            inputX = 0f;
            return;
        }
        ActualizarAnimacionSalto();
        LeerMovimiento();
        ActualizarSonidoPasos();
        ActualizarAnimacionMovimiento();
        ProcesarSalto();
        ProcesarArco();
    }
    private void LeerMovimiento()
    {
        inputX = 0f;

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            inputX = 1f;
            if (SpriteRen != null) SpriteRen.flipX = false;
        }
        else if (Keyboard.current.leftArrowKey.isPressed)
        {
            inputX = -1f;
            if (SpriteRen != null) SpriteRen.flipX = true;
        }

        if (arcoPreparado || disparando)
            inputX = 0f;
    }
    private void ActualizarSonidoPasos()
    {
        bool caminando = Mathf.Abs(inputX) > 0.01f
                         && estaEnElSuelo
                         && !arcoPreparado
                         && !disparando;

        if (caminando)
        {
            temporizadorPasos += Time.deltaTime;

            if (temporizadorPasos >= intervaloPasos)
            {
                if (audioSource != null && sonidoPasos != null && !audioSource.isPlaying)
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
            DetenerSonidoPasos();
        }
    }
    private void DetenerSonidoPasos()
    {
        if (audioSource != null && audioSource.clip == sonidoPasos)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
    private void ActualizarAnimacionMovimiento()
    {
        if (animator != null)
            animator.SetBool("Running", inputX != 0f);
    }
    private void ProcesarSalto()
    {
        if (Keyboard.current.xKey.wasPressedThisFrame && !Pause.BloquearInputFrame
            && estaEnElSuelo && !arcoPreparado && !disparando)
        {
            Saltar();
        }
    }
    private void ProcesarArco()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame && !Pause.BloquearInputFrame)
        {
            if (!arcoPreparado && !disparando)
            {
                if (inputX == 0f)
                    PrepararArco();
            }
            else if (arcoPreparado && arcoListoParaDisparar && !disparando)
            {
                DispararUnaFlecha();
            }
        }

        if (arcoPreparado && !disparando)
        {
            temporizadorArco += Time.deltaTime;
            if (temporizadorArco >= tiempoSinDisparar)
                GuardarArco();
        }

        if (disparando)
        {
            temporizadorDisparo += Time.deltaTime;
            if (temporizadorDisparo >= duracionAnimacionDisparo)
                FinalizarDisparo();
        }
    }
    private void ActualizarColorDanio()
    {
        if (temporizadorColorDanio <= 0f || SpriteRen == null)
            return;

        temporizadorColorDanio -= Time.deltaTime;

        if (temporizadorColorDanio <= 0f)
        {
            SpriteRen.color = colorOriginal;
            temporizadorColorDanio = 0f;
        }
    }
    private void ActualizarInvulnerabilidad()
    {
        if (!invulnerable)
            return;

        temporizadorInvulnerabilidad -= Time.deltaTime;
        if (temporizadorInvulnerabilidad <= 0f)
        {
            invulnerable = false;
            temporizadorInvulnerabilidad = 0f;
            Debug.Log("Eira ya puede recibir daño nuevamente.");
        }
    }
    private void ComprobarSuelo()
    {
        if (groundCheck == null)
            return;

        bool tocandoSuelo = Physics2D.OverlapCircle(
            groundCheck.position, radioSuelo, capaSuelo);

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
            rb2D.linearVelocity = new Vector2(0f, rb2D.linearVelocity.y);
            return;
        }

        float velocidadX = (!arcoPreparado && !disparando) ? inputX * WalkSpeed : 0f;

        if (Mathf.Abs(velocidadX) < 0.01f)
        {
            rb2D.linearVelocity = new Vector2(0f, rb2D.linearVelocity.y);
            return;
        }

        if (estaEnElSuelo && groundCheck != null)
        {
            Vector2 origenRaycast = (Vector2)groundCheck.position
                                    + Vector2.up * alturaInicioRaycast;

            RaycastHit2D impacto = Physics2D.Raycast(
                origenRaycast, Vector2.down, distanciaRaycastSuelo, capaSuelo);

            if (impacto.collider != null && impacto.normal.y > 0.2f)
            {
                Vector2 direccionPendiente = new Vector2(
                    impacto.normal.y, -impacto.normal.x).normalized;

                if (velocidadX < 0f)
                    direccionPendiente = -direccionPendiente;

                rb2D.linearVelocity = direccionPendiente * Mathf.Abs(velocidadX);
                return;
            }
        }

        rb2D.linearVelocity = new Vector2(velocidadX, rb2D.linearVelocity.y);
    }
    private void PrepararArco()
    {
        arcoPreparado = true;
        arcoListoParaDisparar = false;
        disparando = false;
        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        if (animator == null) return;
        animator.SetBool("Running", false);
        animator.SetBool("ReturnToIdle", false);
        animator.SetBool("AttackStart", false);
        animator.SetBool("AttackIni", true);
        animator.SetBool("EstaAtacando", true);
    }
    // Animation Event de EiraAttackIntro.
    public void TerminarPreparacionArco()
    {
        if (arcoPreparado && !disparando)
            arcoListoParaDisparar = true;
    }

    private void DispararUnaFlecha()
    {
        if (!arcoPreparado || !arcoListoParaDisparar || disparando)
            return;

        if (prefabFlecha == null || controladorDisparo == null)
        {
            Debug.LogWarning("Asigna el prefab de la flecha y ControladorDisparo en Eira.");
            return;
        }

        disparando = true;
        arcoListoParaDisparar = false;
        flechaCreadaEsteDisparo = false;
        temporizadorDisparo = 0f;
        temporizadorArco = 0f;

        if (animator != null)
        {
            animator.SetBool("ReturnToIdle", false);
            animator.SetBool("AttackIni", false);
            animator.SetBool("AttackStart", true);
            animator.SetBool("EstaAtacando", true);
        }
    }
    // Animation Event de EiraAttackFinish.
    public void LanzarFlecha()
    {
        if (!disparando || flechaCreadaEsteDisparo
            || prefabFlecha == null || controladorDisparo == null)
            return;

        if (audioSource != null && sonidoAtaque != null)
            audioSource.PlayOneShot(sonidoAtaque);

        bool miraDerecha = SpriteRen == null || !SpriteRen.flipX;
        Vector3 posicionLocal = controladorDisparo.localPosition;
        posicionLocal.x = Mathf.Abs(posicionLocal.x) * (miraDerecha ? 1f : -1f);
        Vector3 posicionDisparo = transform.TransformPoint(posicionLocal);

        GameObject nuevaFlecha = Instantiate(
            prefabFlecha, posicionDisparo, Quaternion.identity);

        Flecha flecha = nuevaFlecha.GetComponent<Flecha>();
        if (flecha != null)
            flecha.Inicializar(miraDerecha);

        flechaCreadaEsteDisparo = true;
    }
    public void FinalizarDisparo()
    {
        disparando = false;
        temporizadorDisparo = 0f;
        temporizadorArco = 0f;
        arcoPreparado = true;
        arcoListoParaDisparar = true;

        if (animator != null)
        {
            animator.SetBool("AttackStart", false);
            animator.SetBool("AttackIni", false);
            animator.SetBool("EstaAtacando", true);
            animator.SetBool("ReturnToIdle", false);
        }
    }
    private void GuardarArco()
    {
        arcoPreparado = false;
        arcoListoParaDisparar = false;
        disparando = false;
        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        if (animator != null)
        {
            animator.SetBool("AttackIni", false);
            animator.SetBool("AttackStart", false);
            animator.SetBool("EstaAtacando", false);
            animator.SetBool("ReturnToIdle", true);
        }

        Debug.Log("Eira terminó el ataque y puede volver a moverse.");
    }
    private void Saltar()
    {
        if (!estaEnElSuelo || rb2D == null)
            return;

        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, 0f);
        rb2D.AddForce(Vector2.up * JumpSpeed, ForceMode2D.Impulse);

        if (audioSource != null && sonidoSalto != null)
            audioSource.PlayOneShot(sonidoSalto);

        estaEnElSuelo = false;

        if (animator != null)
        {
            animator.SetBool("EnElAire", true);
            animator.SetBool("Cayendo", false);
        }
    }
    private void ActualizarAnimacionSalto()
    {
        if (animator == null || rb2D == null)
            return;

        if (estaEnElSuelo)
        {
            animator.SetBool("EnElAire", false);
            animator.SetBool("Cayendo", false);
            return;
        }

        animator.SetBool("Cayendo", rb2D.linearVelocity.y < -0.1f);
    }

    public void RecibirDano(float cantidad)
    {
        if (invulnerable || estaMuerta)
            return;

        Vida -= cantidad;
        Vida = Mathf.Clamp(Vida, 0f, maxVida);

        if (barraVida != null)
            barraVida.value = Vida;

        if (Vida <= 0f)
        {
            Morir();
            return;
        }

        Debug.Log("Eira recibió daño. Vida: " + Vida);

        invulnerable = true;
        temporizadorInvulnerabilidad = duracionInvulnerabilidad;

        if (SpriteRen != null)
        {
            SpriteRen.color = colorDanio;
            temporizadorColorDanio = duracionColorDanio;
        }

        recibiendoDano = true;
        inputX = 0f;

        if (rb2D != null)
            rb2D.linearVelocity = new Vector2(0f, rb2D.linearVelocity.y);

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
            if (sonidoDaño != null)
                audioSource.PlayOneShot(sonidoDaño);
        }

        arcoPreparado = false;
        arcoListoParaDisparar = false;
        disparando = false;
        temporizadorArco = 0f;
        temporizadorDisparo = 0f;

        if (animator != null)
        {
            animator.SetBool("EstaAtacando", false);
            animator.SetBool("AttackIni", false);
            animator.SetBool("AttackStart", false);
            animator.SetBool("ReturnToIdle", false);
            animator.SetBool("Running", false);
            animator.SetTrigger("Daño");
        }
    }

    public void FinalizarAnimacionDano()
    {
        recibiendoDano = false;
        inputX = 0f;
        Debug.Log("Eira terminó la animación de daño.");
    }

    private void ActualizarAnimacionDano()
    {
        if (!recibiendoDano || animator == null)
            return;

        AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);
        if (estado.IsName("EiraHurt") && estado.normalizedTime >= 1f)
        {
            recibiendoDano = false;
            inputX = 0f;
            Debug.Log("Animación de daño finalizada.");
        }
    }

    public void Morir()
    {
        if (estaMuerta)
            return;

        estaMuerta = true;
        inputX = 0f;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.simulated = false;
        }

        if (audioSource != null)
            audioSource.Stop();

        if (efectoMuerte != null)
        {
            ParticleSystem efecto = Instantiate(
                efectoMuerte, transform.position, Quaternion.identity);
            efecto.Play();
        }

        if (SpriteRen != null)
            SpriteRen.enabled = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        StartCoroutine(MostrarGameOver());
    }

    private IEnumerator MostrarGameOver()
    {
        yield return new WaitForSeconds(tiempoAntesGameOver);

        if (menuGameOver != null)
            menuGameOver.MostrarGameOver();
        else
            Debug.LogWarning("No asignaste el menú GameOver en Eira.");
    }

    public void MorirPorCaida()
    {
        Morir();
    }
}
