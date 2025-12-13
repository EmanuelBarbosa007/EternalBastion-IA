using UnityEngine;
using UnityEngine.UI;

public class BaseHealth : MonoBehaviour
{
    public int maxHealth = 50;
    public int currentHealth;

    public Slider healthBar;

    [Header("Audio Settings")]
    public AudioClip damageSound;   // Som quando leva dano 
    public AudioClip destroySound;  // Som quando base é destruida
    [Range(0f, 1f)] public float soundVolume = 1f;

    private GameManager gameManager;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Atualiza a barra de vida
        if (healthBar != null)
            healthBar.value = currentHealth;

        // Verifica se morreu ou se apenas sofreu dano
        if (currentHealth <= 0)
        {
            // SOM DE DESTRUIÇÃO
            if (destroySound != null)
            {
                AudioSource.PlayClipAtPoint(destroySound, transform.position, soundVolume);
            }

            // Chama o Game Over
            if (gameManager != null)
                gameManager.GameOver();
        }
        else
        {
            //SOM DE DANO NORMAL
            if (damageSound != null)
            {
                AudioSource.PlayClipAtPoint(damageSound, transform.position, soundVolume);
            }
        }
    }
}