using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class BaseHealthMP : NetworkBehaviour
{
    public int maxHealth = 100;
    public Slider healthBar;

    // Sincroniza a vida da base do Server para os Clientes
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    // O Server define quem é o dono desta base
    [HideInInspector]
    public ulong donoDaBaseClientId;

    // --- Sincronização ---

    public override void OnNetworkSpawn()
    {
        // Quando um cliente entra, o valor atual é sincronizado
        // Ligamos a função de UI a esta mudança
        currentHealth.OnValueChanged += OnHealthChanged;

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
        }

        // Atualiza o UI com o valor inicial
        UpdateHealthBar(currentHealth.Value);
    }

    // Esta função corre em TODOS OS CLIENTES
    private void OnHealthChanged(int previousValue, int newValue)
    {
        UpdateHealthBar(newValue);
    }

    private void UpdateHealthBar(int value)
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = value;
        }
    }

    // --- Lógica de Dano (SÓ O SERVER PODE CHAMAR ISTO) ---

    public void TakeDamage(int amount)
    {
        if (!IsServer)
        {
            // Só o server pode reduzir a vida
            return;
        }

        int newHealth = currentHealth.Value - amount;
        currentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (currentHealth.Value <= 0)
        {
            // Avisa o GameManager (no server) que esta base foi destruída
            if (GameManagerMP.Instance != null)
            {
                GameManagerMP.Instance.BaseDestruida(donoDaBaseClientId);
            }
        }
    }
}