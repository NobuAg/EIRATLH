using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Flecha : MonoBehaviour
{
    [SerializeField] private float velocidad = 12f;
    [SerializeField] private float tiempoDeVida = 3f;

    private Rigidbody2D rb2D;
    private Vector2 direccion;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        if (rb2D != null)
        {
            rb2D.gravityScale = 0f;
            rb2D.freezeRotation = true;
        }
    }

    public void Inicializar(bool miraDerecha)
    {
        direccion = miraDerecha ? Vector2.right : Vector2.left;

        if (rb2D != null)
        {
            rb2D.gravityScale = 0f;
            rb2D.linearVelocity = direccion * velocidad;
        }

        Vector3 escala = transform.localScale;
        escala.x = Mathf.Abs(escala.x) *
                   (miraDerecha ? 1f : -1f);

        transform.localScale = escala;

        Destroy(gameObject, tiempoDeVida);
    }
}

