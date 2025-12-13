using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TroopSenderUI : MonoBehaviour
{
    [Header("Configuração de Cooldown")]
    [Tooltip("Tempo em segundos que o jogador tem de esperar entre cada clique")]
    public float delayEntreSpawns = 0.5f;
    private float tempoProximoSpawn = 0f; // Controla o tempo interno

    [Header("Custos de Spawn")]
    public int custoTropaNormal = 20;
    public int prefabIdTropaNormal = 0;

    public int custoTropaTanque = 35;
    public int prefabIdTropaTanque = 1;

    public int custoCavalo = 25;
    public int prefabIdCavalo = 8;

    // --- NOVO: Referências aos Botões de Spawn (Para desativar durante o delay) ---
    [Header("Botões de Spawn (Arrasta os botões aqui)")]
    public Button botaoSpawnNormal;
    public Button botaoSpawnTanque;
    public Button botaoSpawnCavalo;

    [Header("Custos de Upgrade")]
    public int custoUpgradeTropaNormal = 50;
    public int custoUpgradeTropaTanque = 75;
    public int custoUpgradeCavalo = 60;

    [Header("UI Upgrade Tropa Normal")]
    public Button botaoUpgradeNormal;
    public TextMeshProUGUI textoNivelNormal;

    [Header("UI Upgrade Tropa Tanque")]
    public Button botaoUpgradeTanque;
    public TextMeshProUGUI textoNivelTanque;

    [Header("UI Upgrade Cavalo")]
    public Button botaoUpgradeCavalo;
    public TextMeshProUGUI textoNivelCavalo;


    void Update()
    {
        if (PlayerNetwork.LocalInstance == null) return;

        // --- LÓGICA DO COOLDOWN (NOVO) ---
        // Verifica se já passou tempo suficiente para poder clicar de novo
        bool podeSpawnar = Time.time >= tempoProximoSpawn;

        // Se tiveres associado os botões no Inspector, ele controla se estão clicáveis ou não
        if (botaoSpawnNormal != null) botaoSpawnNormal.interactable = podeSpawnar;
        if (botaoSpawnTanque != null) botaoSpawnTanque.interactable = podeSpawnar;
        if (botaoSpawnCavalo != null) botaoSpawnCavalo.interactable = podeSpawnar;


        // --- Atualiza UI da Tropa Normal ---
        int nivelNormal = PlayerNetwork.LocalInstance.NivelTropaNormal.Value;
        if (textoNivelNormal != null) textoNivelNormal.text = "Nível: " + nivelNormal;

        if (botaoUpgradeNormal != null) botaoUpgradeNormal.interactable = (nivelNormal < 2);

        // --- Atualiza UI da Tropa Tanque ---
        int nivelTanque = PlayerNetwork.LocalInstance.NivelTropaTanque.Value;
        if (textoNivelTanque != null) textoNivelTanque.text = "Nível: " + nivelTanque;

        if (botaoUpgradeTanque != null) botaoUpgradeTanque.interactable = (nivelTanque < 2);

        // --- Atualiza UI do Cavalo ---
        int nivelCavalo = PlayerNetwork.LocalInstance.NivelCavalo.Value;
        if (textoNivelCavalo != null) textoNivelCavalo.text = "Nível: " + nivelCavalo;

        if (botaoUpgradeCavalo != null) botaoUpgradeCavalo.interactable = (nivelCavalo < 2);
    }


    // --- Funções de Comprar ---

    public void OnClick_ComprarTropaNormal()
    {
        // 1. Verifica se estamos no tempo de espera (Cooldown)
        if (Time.time < tempoProximoSpawn) return;

        if (PlayerNetwork.LocalInstance != null)
        {
            // 2. Define o novo tempo de espera
            tempoProximoSpawn = Time.time + delayEntreSpawns;

            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdTropaNormal,
                custoTropaNormal
            );
        }
    }

    public void OnClick_ComprarTropaTanque()
    {
        if (Time.time < tempoProximoSpawn) return;

        if (PlayerNetwork.LocalInstance != null)
        {
            tempoProximoSpawn = Time.time + delayEntreSpawns;

            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdTropaTanque,
                custoTropaTanque
            );
        }
    }

    public void OnClick_ComprarCavalo()
    {
        if (Time.time < tempoProximoSpawn) return;

        if (PlayerNetwork.LocalInstance != null)
        {
            tempoProximoSpawn = Time.time + delayEntreSpawns;

            PlayerNetwork.LocalInstance.RequestSpawnTroopServerRpc(
                prefabIdCavalo,
                custoCavalo
            );
        }
    }

    // --- Funções de Melhorar (Mantêm-se iguais) ---
    // (Não adicionei delay aqui porque upgrades são raros, mas podes adicionar se quiseres)

    public void OnClick_MelhorarTropaNormal()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdTropaNormal,
                custoUpgradeTropaNormal
            );
        }
    }

    public void OnClick_MelhorarTropaTanque()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdTropaTanque,
                custoUpgradeTropaTanque
            );
        }
    }

    public void OnClick_MelhorarCavalo()
    {
        if (PlayerNetwork.LocalInstance != null)
        {
            PlayerNetwork.LocalInstance.RequestUpgradeTroopServerRpc(
                prefabIdCavalo,
                custoUpgradeCavalo
            );
        }
    }
}