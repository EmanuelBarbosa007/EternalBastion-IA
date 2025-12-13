using UnityEngine;
using Unity.Netcode;
using System.Collections;
using UnityEngine.EventSystems;

// A tua struct de stats mantém-se igual
[System.Serializable]
public class MineLevelStats
{
    [Tooltip("Custo para fazer upgrade PARA este nível")]
    public int upgradeCost;
    public int coinsPerInterval;
    public float generationInterval = 10f;
    public GameObject visualPrefab;
}

[RequireComponent(typeof(NetworkObject))]
public class GoldMineMP : NetworkBehaviour
{
    [Header("Atributos da Mina")]
    public NetworkVariable<int> level = new NetworkVariable<int>(1);
    public int maxLevel = 3;
    public MineLevelStats[] levelStats;

    [Header("Referências")]
    public Transform visualContainer;

    // --- CORREÇÃO: Usar NetworkVariables para sincronizar dados ---
    // Variáveis normais não passam para o cliente, por isso usamos estas:
    public NetworkVariable<ulong> donoDaMinaClientId = new NetworkVariable<ulong>();
    public NetworkVariable<int> totalInvested = new NetworkVariable<int>();

    // Para guardar referências a objetos (como o Spot), usamos NetworkObjectReference
    public NetworkVariable<NetworkObjectReference> debrisSpotRef = new NetworkVariable<NetworkObjectReference>();

    public override void OnNetworkSpawn()
    {
        // Regista a callback
        level.OnValueChanged += OnLevelChanged;
        OnLevelChanged(0, level.Value);

        if (IsServer)
        {
            StartCoroutine(GenerateCoinsRoutine());
        }
    }

    public override void OnNetworkDespawn()
    {
        level.OnValueChanged -= OnLevelChanged;
    }

    /// <summary>
    /// NOVO MÉTODO: Configura a mina logo após o Spawn no servidor
    /// </summary>
    public void SetupMine(ulong _donoId, int _custo, NetworkObject _spotNetObject)
    {
        if (!IsServer) return;

        donoDaMinaClientId.Value = _donoId;
        totalInvested.Value = _custo;
        debrisSpotRef.Value = _spotNetObject; // Cria a referência de rede
    }

    private void OnLevelChanged(int previousLevel, int newLevel)
    {
        ApplyLevelVisuals(newLevel);
    }

    void ApplyLevelVisuals(int newLevel)
    {
        if (visualContainer == null) return;
        if (newLevel <= 0 || newLevel > levelStats.Length) return;

        MineLevelStats stats = levelStats[newLevel - 1];
        if (stats.visualPrefab == null) return;

        foreach (Transform child in visualContainer)
        {
            Destroy(child.gameObject);
        }

        Instantiate(stats.visualPrefab, visualContainer.position, visualContainer.rotation, visualContainer);
    }

    IEnumerator GenerateCoinsRoutine()
    {
        if (!IsServer) yield break;

        while (true)
        {
            MineLevelStats stats = levelStats[level.Value - 1];
            yield return new WaitForSeconds(stats.generationInterval);

            // Usa .Value para aceder à variável de rede
            CurrencySystemMP.Instance.AddMoney(donoDaMinaClientId.Value, stats.coinsPerInterval);
        }
    }

    private void OnMouseDown()
    {
        // 1. Impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (PlayerNetwork.LocalInstance == null) return;

        // --- CORREÇÃO: Recuperar o DebrisSpot a partir da referência de rede ---
        DebrisSpotMP myDebrisSpot = null;
        if (debrisSpotRef.Value.TryGet(out NetworkObject spotObject))
        {
            myDebrisSpot = spotObject.GetComponent<DebrisSpotMP>();
        }

        if (myDebrisSpot == null)
        {
            Debug.LogError("Cliente: Mina não conseguiu encontrar o DebrisSpot associado!");
            return;
        }

        // 2. Verifica se o jogador local é o dono do SPOT (segurança adicional)
        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (myDebrisSpot.donoDoSpot != localPlayerId)
        {
            Debug.Log("Esta mina (spot) não é sua!");
            return;
        }

        // 3. Abre o painel de Upgrade
        if (MineUpgradeUIMP.Instance != null)
        {
            if (TowerPlacementUIMP.Instance != null) TowerPlacementUIMP.Instance.ClosePanel();
            if (TowerUpgradeUIMP.Instance != null) TowerUpgradeUIMP.Instance.ClosePanel();

            MineUpgradeUIMP.Instance.OpenPanel(this, myDebrisSpot);
        }
    }

    public void TryUpgrade(ulong playerClientId)
    {
        if (!IsServer) return;

        // Usa .Value
        if (playerClientId != donoDaMinaClientId.Value) return;

        if (level.Value >= maxLevel) return;

        int upgradeCost = levelStats[level.Value].upgradeCost;

        if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCost))
        {
            totalInvested.Value += upgradeCost; // Usa .Value
            level.Value++;
        }
    }

    public int GetSellValue()
    {
        // Usa .Value
        return Mathf.RoundToInt(totalInvested.Value * 0.7f);
    }
}