using UnityEngine;
using UnityEngine.InputSystem;

public class Eira : MonoBehaviour
{
    private float maxVida = 100;
    private float Vida;
    private float WalkSpeed = 4f;
    private float JumpSpeed = 6f;
    public Rigidbody2D rb2D;
    private float inputX;
    private bool estaEnElSuelo = true;

    public SpriteRenderer SpriteRen;

    void Start()
    {
        Vida = maxVida;
        rb2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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

        if ((Keyboard.current.xKey.wasPressedThisFrame) && !Pause.BloquearInputFrame)
        {
            if (estaEnElSuelo) 
            { 
                Saltar(); 
            }
        }
    }

    void FixedUpdate()
    {
        rb2D.linearVelocity = new Vector2(inputX * WalkSpeed, rb2D.linearVelocity.y);
    }

    private void Saltar()
    {
        rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, JumpSpeed);
        estaEnElSuelo = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        estaEnElSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        estaEnElSuelo = false;
    }

    public void RecibirDano(float cantidad)
    {
        Vida -= cantidad;
        Debug.Log("Vida de Eira: " + Vida);
        if (Vida <= 0f) Debug.Log("Eira ha muerto");
    }
}
