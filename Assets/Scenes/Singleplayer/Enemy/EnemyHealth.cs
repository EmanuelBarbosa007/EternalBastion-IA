using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;
    public int reward = 20;

    [Header("UI")]
    public Slider healthBar;

    [Header("Audio Settings")] //áudio
    public AudioClip deathSound;
    [Range(0f, 1f)] public float soundVolume = 1f; // Controlo de volume (0 a 1)

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {

        // Toca o som na posição onde o inimigo morreu
        if (deathSound != null)
        {
            // PlayClipAtPoint cria um objeto temporário, assim o som não corta quando o inimigo é destruído
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }


        CurrencySystem.AddMoney(reward);

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        TrojanHorseBoss boss = GetComponent<TrojanHorseBoss>();

        if (boss != null)
        {
            boss.StartDeathSequence();
            return;
        }

        Destroy(gameObject);
    }
}