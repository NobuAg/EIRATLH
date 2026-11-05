
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Referencia al jugador")]
    public Transform Eira;

    [Header("Movimiento")]
    public float velocidad = 2f;
    public float distanciaDeteccion = 5f;

    [Header("Vida del enemigo")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    [Header("Daño")]
    public float dañoFlecha = 25f;
    public float dañoContacto = 10f;
    public float intervaloDaño = 0.25f;

    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;

    private Eira jugadorEnContacto;
    private float temporizadorContacto = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        vidaActual = vidaMaxima;

        if (animator != null)
        {
            animator.SetBool("PlayerCerca", false);
            animator.SetBool("Running", false);
        }
    }

    void Update()
    {
        if (Eira == null)
            return;

        float distancia = Vector2.Distance(transform.position, Eira.position);

        if (distancia <= distanciaDeteccion)
        {
            Vector2 direccion = (Eira.position - transform.position).normalized;

            movement = new Vector2(direccion.x, 0f);

            if (animator != null)
            {
                animator.SetBool("PlayerCerca", true);
                animator.SetBool("Running", Mathf.Abs(movement.x) > 0.01f);
            }

            if (spriteRenderer != null && movement.x != 0)
            {
                spriteRenderer.flipX = movement.x < 0;
            }
        }
        else
        {
            movement = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("PlayerCerca", false);
                animator.SetBool("Running", false);
            }
        }

        // DAÑO REPETIDO POR CONTACTO
        if (jugadorEnContacto != null)
        {
            temporizadorContacto -= Time.deltaTime;

            if (temporizadorContacto <= 0f)
            {
                jugadorEnContacto.RecibirDano(dañoContacto);
                temporizadorContacto = intervaloDaño;
            }
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (Eira == null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(
            movement.x * velocidad,
            rb.linearVelocity.y
        );
    }

    // RECIBIR DAÑO
    public void RecibirDano(float cantidad)
    {
        vidaActual -= cantidad;

        Debug.Log("Vida del enemigo: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    // MORIR
    void Morir()
    {
        Debug.Log("El enemigo ha muerto.");
        Destroy(gameObject);
    }

    // COLISIONES FÍSICAS
    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcesarImpacto(collision.collider);
        ProcesarContactoJugador(collision.collider);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Eira jugador = collision.collider.GetComponentInParent<Eira>();

        if (jugador != null && jugador == jugadorEnContacto)
        {
            jugadorEnContacto = null;
            temporizadorContacto = 0f;
        }
    }

    // COLISIONES CON TRIGGER
    void OnTriggerEnter2D(Collider2D other)
    {
        ProcesarImpacto(other);
        ProcesarContactoJugador(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Eira jugador = other.GetComponentInParent<Eira>();

        if (jugador != null && jugador == jugadorEnContacto)
        {
            jugadorEnContacto = null;
            temporizadorContacto = 0f;
        }
    }

    // DAÑO RECIBIDO POR FLECHAS
    void ProcesarImpacto(Collider2D other)
    {
        Flecha flecha = other.GetComponentInParent<Flecha>();

        if (flecha != null)
        {
            RecibirDano(dañoFlecha);
            Destroy(flecha.gameObject);
        }
    }

    // DAÑO AL JUGADOR POR CONTACTO
    void ProcesarContactoJugador(Collider2D other)
    {
        Eira jugador = other.GetComponentInParent<Eira>();

        if (jugador != null)
        {
            jugadorEnContacto = jugador;
            temporizadorContacto = 0f;
        }
    }
}
