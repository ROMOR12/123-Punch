using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;

    private Vector3 velocity;       
    private float gravity = 30f;    
    private float disappearTimer;
    private Color textColor;

    private const float DISAPPEAR_TIMER_MAX = 2f;

    public void Setup(int damageAmount, bool isCritical)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = damageAmount.ToString();

        if (isCritical)
        {
            textMesh.fontSize = 7;
            textMesh.color = Color.red;
        }
        else
        {
            textMesh.fontSize = 5;
            textMesh.color = new Color(1f, 0.8f, 0f);
        }
        textColor = textMesh.color;
        disappearTimer = DISAPPEAR_TIMER_MAX;

        // --- LA MAGIA DE LA GRAVEDAD ---
        // 1. Velocidad Horizontal (X): Aleatoria izquierda o derecha (-3 a 3)
        // 2. Velocidad Vertical (Y): Salto fuerte hacia arriba (12)
        velocity = new Vector3(Random.Range(-3f, 3f), 12f, 0);
    }

    void Update()
    {
        // 1. Aplicar movimiento
        transform.position += velocity * Time.deltaTime;

        // 2. Aplicar Gravedad
        velocity.y -= gravity * Time.deltaTime;

        // 3. Temporizador de desaparición
        disappearTimer -= Time.deltaTime;

        // 4. Efecto de Escala y Desvanecido
        if (disappearTimer > DISAPPEAR_TIMER_MAX * 0.5f)
        {
            // Primera mitad: Crece un poco
            float increaseScaleAmount = 1f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            // Segunda mitad: Se encoge y desaparece
            float decreaseScaleAmount = 1f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;

            // Alpha
            float disappearSpeed = 5f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
        }

        // Auto-destrucción
        if (textColor.a < 0)
        {
            Destroy(gameObject);
        }
    }
}
