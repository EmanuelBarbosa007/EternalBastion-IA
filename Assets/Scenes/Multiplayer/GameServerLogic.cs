using UnityEngine;
using Unity.Netcode;

public class GameServerLogic : NetworkBehaviour
{
    public static GameServerLogic Instance;

    [Header("Jogador A (Host)")]
    public Transform spawnPointA;
    public WaypointPathMP caminhoA;
    public BaseHealthMP baseA;

    [Header("Jogador B (IA)")]
    public Transform spawnPointB;
    public WaypointPathMP caminhoB;
    public BaseHealthMP baseB;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public bool TrySpawnTroop(ulong clientId, int tropaPrefabID, int custo, int nivel)
    {
        if (!IsServer) return false;
        if (CurrencySystemMP.Instance.SpendMoney(clientId, custo))
        {
            Transform spawnPoint = (clientId == 0) ? spawnPointA : spawnPointB;
            WaypointPathMP caminho = (clientId == 0) ? caminhoA : caminhoB;
            BaseHealthMP baseInimiga = (clientId == 0) ? baseB : baseA;

            GameObject prefabTropa = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[tropaPrefabID].Prefab;
            GameObject go = Instantiate(prefabTropa, spawnPoint.position, spawnPoint.rotation);

            go.GetComponent<NetworkObject>().Spawn();

            EnemyMP tropa = go.GetComponent<EnemyMP>();
            tropa.Setup(clientId, caminho.points, baseInimiga, nivel);

            return true;
        }
        return false;
    }

    public bool TryBuildTower(ulong clientId, int torrePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        if (!IsServer) return false;

        if (CurrencySystemMP.Instance.SpendMoney(clientId, custo))
        {
            GameObject prefabTorre = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[torrePrefabID].Prefab;
            GameObject go = Instantiate(prefabTorre, posicao, Quaternion.identity);

            TowerMP torre = go.GetComponent<TowerMP>();
            torre.donoDaTorreClientId = clientId;
            torre.totalInvested = custo;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
            {
                TowerSpotMP spot = spotNetworkObject.GetComponent<TowerSpotMP>();
                spot.SetOcupado(torre);
                torre.myTowerSpot = spot;
            }

            go.GetComponent<NetworkObject>().Spawn();
            return true;
        }
        return false;
    }

    public bool TryUpgradeTower(ulong clientId, ulong towerNetworkId)
    {
        if (!IsServer) return false;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
        {
            Debug.LogError($"GameServerLogic: Não foi possível encontrar a torre {towerNetworkId} para upgrade.");
            return false;
        }
        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null)
        {
            Debug.LogError($"GameServerLogic: O objeto {towerNetworkId} não tem um componente TowerMP.");
            return false;
        }

        torre.TryUpgrade(clientId);
        return true;
    }

    public bool TrySellTower(ulong clientId, ulong towerNetworkId, ulong spotNetworkId)
    {
        if (!IsServer) return false;
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(towerNetworkId, out NetworkObject towerNetworkObject))
            return false;

        TowerMP torre = towerNetworkObject.GetComponent<TowerMP>();
        if (torre == null) return false;

        if (torre.donoDaTorreClientId != clientId)
            return false;

        int sellAmount = torre.totalInvested / 2;
        CurrencySystemMP.Instance.AddMoney(clientId, sellAmount);

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
        {
            spotNetworkObject.GetComponent<TowerSpotMP>().SetVazio();
        }

        towerNetworkObject.Despawn();
        return true;
    }



    public bool TryBuildMine(ulong clientId, int minePrefabID, int custo, Vector3 posicao, ulong spotNetworkId)
    {
        if (!IsServer) return false;

        // 1. Encontra o spot PRIMEIRO
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
        {
            Debug.LogError($"[Server] TryBuildMine: Não foi possível encontrar o DebrisSpotMP com ID {spotNetworkId}");
            return false;
        }
        DebrisSpotMP spot = spotNetworkObject.GetComponent<DebrisSpotMP>();
        if (spot == null) return false;

        // 2. Tenta gastar o dinheiro
        if (CurrencySystemMP.Instance.SpendMoney(clientId, custo))
        {
            // 3. Encontra o prefab
            GameObject prefabMina = NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs[minePrefabID].Prefab;

            // 4. Calcula posição
            Vector3 spawnPos = spot.transform.position + (Vector3.up * spot.spawnHeightOffset);
            Quaternion spawnRot = spot.transform.rotation * Quaternion.Euler(0, spot.spawnYRotation, 0);

            // 5. Instancia
            GameObject go = Instantiate(prefabMina, spawnPos, spawnRot);
            GoldMineMP mine = go.GetComponent<GoldMineMP>();
            NetworkObject mineNetObj = go.GetComponent<NetworkObject>();

            // --- MUDANÇA 1: Spawn PRIMEIRO ---
            // O objeto tem de nascer na rede antes de definirmos as NetworkVariables
            mineNetObj.Spawn();

            // --- MUDANÇA 2: Usar o SetupMine ---
            // Passamos os dados para as NetworkVariables através deste método
            mine.SetupMine(clientId, custo, spotNetworkObject);

            // 7. Atualiza o spot
            spot.SetOcupado(mine);

            return true;
        }
        return false;
    }

    public bool TryUpgradeMine(ulong clientId, ulong mineNetworkId)
    {
        if (!IsServer) return false;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mineNetworkId, out NetworkObject mineNetworkObject))
        {
            Debug.LogError($"GameServerLogic: Não foi possível encontrar a mina {mineNetworkId} para upgrade.");
            return false;
        }

        GoldMineMP mine = mineNetworkObject.GetComponent<GoldMineMP>();
        if (mine == null) return false;

        // A própria mina trata da lógica 
        mine.TryUpgrade(clientId);
        return true;
    }

    public bool TrySellMine(ulong clientId, ulong mineNetworkId, ulong spotNetworkId)
    {
        if (!IsServer) return false;

        // 1. Encontra a mina
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(mineNetworkId, out NetworkObject mineNetworkObject))
            return false;

        GoldMineMP mine = mineNetworkObject.GetComponent<GoldMineMP>();
        if (mine == null) return false;

        // --- MUDANÇA 3: Usar .Value ---
        // Como donoDaMinaClientId agora é uma NetworkVariable, temos de por .Value
        if (mine.donoDaMinaClientId.Value != clientId)
            return false;

        // 3. Encontra o spot
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spotNetworkId, out NetworkObject spotNetworkObject))
            return false;

        DebrisSpotMP spot = spotNetworkObject.GetComponent<DebrisSpotMP>();
        if (spot == null) return false;

        // 4. Paga ao jogador (GetSellValue também já usa .Value lá dentro)
        int sellAmount = mine.GetSellValue();
        CurrencySystemMP.Instance.AddMoney(clientId, sellAmount);

        // 5. Liberta o spot
        spot.SetVazio();

        // 6. Destrói
        mineNetworkObject.Despawn();
        return true;
    }
}