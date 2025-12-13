using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHealthMP : NetworkBehaviour
{
    [Header("Stats Base (Nível 1)")]
    public int baseHealth = 10;
    public int moneyOnDeath = 5;

    [Header("Multiplicadores Nível 2")]
    public float healthMultiplierLvl2 = 1.5f; // Bónus de 50%

    [Header("UI")]
    public Slider healthBar;

    [Header("Audio Settings")] // --- NOVO ---
    public AudioClip deathSound; // Som específico para este tipo de tropa
    [Range(0f, 1f)] public float soundVolume = 1f;

    private NetworkVariable<int> currentMaxHealth = new NetworkVariable<int>();
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public void SetNivelServer(int nivel)
    {
        if (!IsServer) return;

        int calculatedMaxHealth;
        if (nivel >= 2)
        {
            calculatedMaxHealth = (int)(baseHealth * healthMultiplierLvl2);
        }
        else
        {
            calculatedMaxHealth = baseHealth;
        }

        currentMaxHealth.Value = calculatedMaxHealth;
        currentHealth.Value = calculatedMaxHealth;
    }

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += OnHealthChanged;
        currentMaxHealth.OnValueChanged += (prev, next) => OnHealthChanged(0, currentHealth.Value);
        OnHealthChanged(0, currentHealth.Value);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = currentMaxHealth.Value;
            healthBar.value = newValue;
        }
    }

    // Chamado pela BulletMP (que só corre no Server)
    public void TakeDamage(int amount, ulong killerClientId)
    {
        if (!IsServer) return;
        if (currentHealth.Value <= 0) return;

        currentHealth.Value -= amount;

        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, currentMaxHealth.Value);

        if (currentHealth.Value <= 0)
        {
            // 1. Manda todos os clientes tocarem o som ANTES de destruir o objeto
            PlayDeathSoundClientRpc();

            // 2. Dá o dinheiro ao jogador
            CurrencySystemMP.Instance.AddMoney(killerClientId, moneyOnDeath);

            // 3. Destrói o inimigo na rede
            NetworkObject.Despawn();
        }
    }


    // O [ClientRpc] é chamado no Server, mas executado em TODOS os clientes
    [ClientRpc]
    private void PlayDeathSoundClientRpc()
    {
        if (deathSound != null)
        {
            // Toca o som no local onde o inimigo está, criando um objeto temporário
            // Isto funciona mesmo se o NetworkObject for destruído logo a seguir
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);
        }
    }
}