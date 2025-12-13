using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{
    public static PlayerNetwork LocalInstance { get; private set; }

    // --- Variáveis de Nível das Tropas (Geridas pelo Server) ---
    public NetworkVariable<int> NivelTropaNormal = new NetworkVariable<int>(1);
    public NetworkVariable<int> NivelTropaTanque = new NetworkVariable<int>(1);
    public NetworkVariable<int> NivelCavalo = new NetworkVariable<int>(1);

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        // O Servidor inicializa os valores para garantir
        if (IsServer)
        {
            NivelTropaNormal.Value = 1;
            NivelTropaTanque.Value = 1;
            NivelCavalo.Value = 1;
        }
    }

    [ServerRpc]
    public void RequestSpawnTroopServerRpc(int tropaPrefabID, int custo)
    {
        // Descobrir o nível desta tropa
        int nivel = 1;
        if (tropaPrefabID == 0) // ID Tropa Normal
            nivel = NivelTropaNormal.Value;
        else if (tropaPrefabID == 1) // ID Tropa Tanque
            nivel = NivelTropaTanque.Value;
        else if (tropaPrefabID == 8) // ID Cavalo
            nivel = NivelCavalo.Value;

        // Chama o cérebro central (isto está correto)
        GameServerLogic.Instance.TrySpawnTroop(OwnerClientId, tropaPrefabID, custo, nivel);
    }

    // --- NOVO: RPC de Upgrade de Tropa (Lógica Simplificada) ---
    [ServerRpc]
    public void RequestUpgradeTroopServerRpc(int tropaPrefabID, int custoUpgrade)
    {
        // Esta função JÁ ESTÁ a correr no Servidor e no NetworkObject deste jogador.
        // Não precisamos de ir ao GameServerLogic.

        // 1. Tentar gastar o dinheiro
        if (CurrencySystemMP.Instance.SpendMoney(OwnerClientId, custoUpgrade))
        {
            // 2. Aplicar o Upgrade DIRETAMENTE nesta instância
            if (tropaPrefabID == 0) // Tropa Normal
            {
                if (NivelTropaNormal.Value < 2)
                    NivelTropaNormal.Value = 2;
            }
            else if (tropaPrefabID == 1) // Tropa Tanque
            {
                if (NivelTropaTanque.Value < 2)
                    NivelTropaTanque.Value = 2;
            }
            else if (tropaPrefabID == 8) // Cavalo
            {
                if (NivelCavalo.Value < 2)
                    NivelCavalo.Value = 2;
            }
        }
        else
        {
            // Log de aviso (opcional)
            Debug.LogWarning($"[Server] Client {OwnerClientId} tentou fazer upgrade (Tropa {tropaPrefabID}) mas não tinha {custoUpgrade} moedas.");
        }
    }


    // --- Resto dos RPCs (Estão corretos como estavam) ---

    [ServerRpc]
    public void RequestBuildTowerServerRpc(int torrePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        GameServerLogic.Instance.TryBuildTower(OwnerClientId, torrePrefabID, custo, posicao, spotNetworkId);
    }

    [ServerRpc]
    public void RequestUpgradeTowerServerRpc(ulong towerNetworkId)
    {
        GameServerLogic.Instance.TryUpgradeTower(OwnerClientId, towerNetworkId);
    }

    [ServerRpc]
    public void RequestSellTowerServerRpc(ulong towerNetworkId, ulong spotNetworkId)
    {
        GameServerLogic.Instance.TrySellTower(OwnerClientId, towerNetworkId, spotNetworkId);
    }

    [ServerRpc]
    public void RequestBuildMineServerRpc(int minePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        GameServerLogic.Instance.TryBuildMine(OwnerClientId, minePrefabID, custo, posicao, spotNetworkId);
    }

    [ServerRpc]
    public void RequestUpgradeMineServerRpc(ulong mineNetworkId)
    {
        GameServerLogic.Instance.TryUpgradeMine(OwnerClientId, mineNetworkId);
    }

    [ServerRpc]
    public void RequestSellMineServerRpc(ulong mineNetworkId, ulong spotNetworkId)
    {
        GameServerLogic.Instance.TrySellMine(OwnerClientId, mineNetworkId, spotNetworkId);
    }
}