using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NetworkObject))]
public class DebrisSpotMP : NetworkBehaviour
{
    [Header("Configuração de Rede")]
    // Sincroniza o estado de ocupado
    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);
    // Sincroniza qual mina está neste spot (pelo NetworkObjectId)
    public NetworkVariable<ulong> currentMineNetworkId = new NetworkVariable<ulong>(0);
    // Defina isto no Inspector para cada spot
    public PlayerID donoDoSpot;

    [Header("Configuração da Mina")]
    public int goldMinePrefabId;
    public int buildCost = 75;

    [Tooltip("Quão mais alto a mina deve 'spawnar' em relação aos destroços.")]
    public float spawnHeightOffset = 0f;
    [Tooltip("Rotação em Y para a mina ficar virada para a frente.")]
    public float spawnYRotation = 180f;

    [Header("Referências")]
    public GameObject debrisVisuals;

    // REMOVIDO: private GoldMineMP cachedMine; -> Não estava a ser usado

    public override void OnNetworkSpawn()
    {
        // Regista a callback. Quando 'isOccupied' mudar, chama a função
        isOccupied.OnValueChanged += OnOccupiedChanged;
        // Chama a função uma vez no início para acertar o estado visual
        OnOccupiedChanged(false, isOccupied.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Limpa a callback
        isOccupied.OnValueChanged -= OnOccupiedChanged;
    }

    private void OnOccupiedChanged(bool previousValue, bool newValue)
    {
        if (debrisVisuals != null)
        {
            // Mostra os destroços se NÃO estiver ocupado
            debrisVisuals.SetActive(!newValue);
        }
    }

    private void OnMouseDown()
    {
        // 1. Só pode ser clicado se estiver VAZIO (a mina tratará dos cliques se estiver cheio)
        if (isOccupied.Value)
            return;

        // 2. Impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 3. Verifica se o jogador local é o dono
        if (PlayerNetwork.LocalInstance == null)
        {
            Debug.LogError("Não consigo encontrar o PlayerNetwork.LocalInstance!");
            return;
        }

        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (donoDoSpot != localPlayerId)
        {
            Debug.Log("Este spot de destroços não é seu!");
            return;
        }

        // 4. Abre o painel de CONSTRUÇÃO da mina
        if (MinePlacementUIMP.Instance != null)
        {
            // Fecha outros painéis para evitar sobreposição
            if (TowerPlacementUIMP.Instance != null)
                TowerPlacementUIMP.Instance.ClosePanel();
            if (TowerUpgradeUIMP.Instance != null)
                TowerUpgradeUIMP.Instance.ClosePanel();

            MinePlacementUIMP.Instance.OpenPanel(this);
        }
    }

    public void SetOcupado(GoldMineMP mine)
    {
        if (!IsServer) return;
        isOccupied.Value = true;
        currentMineNetworkId.Value = mine.NetworkObjectId;
    }

    public void SetVazio()
    {
        if (!IsServer) return;
        isOccupied.Value = false;
        currentMineNetworkId.Value = 0;
    }
}