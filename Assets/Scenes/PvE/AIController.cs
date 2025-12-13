using UnityEngine;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    private ulong aiClientId = 1;

    [Header("Dados do Jogo (Ligar no Inspector)")]
    public TroopSenderUI troopData;
    public TowerPlacementUIMP towerData;

    [Header("Defesas da IA (Ligar no Inspector)")]
    [SerializeField]
    private TowerSpotMP[] aiTowerSpots;

    private BTNode tree;
    private float decisionCooldown = 1.0f;
    private float nextDecisionTime;

    private bool isAiReady = false; 

    void Start()
    {
        if (GameServerLogic.Instance == null)
        {
            Debug.LogError("AIController: GameServerLogic.Instance é nulo! O GameObject _GameServerLogic está na cena?");
            Destroy(this.gameObject);
        }
    }

    void Update()
    {
        //LÓGICA DE INICIAÇÃO
        if (!isAiReady)
        {
            // Se a IA não estiver pronta, verifica se o servidor já está a postos
            if (GameServerLogic.Instance != null && GameServerLogic.Instance.IsServer)
            {
                //  o servidor está pronto
                Debug.Log("AIController: Servidor detetado. A construir Árvore de Comportamento.");
                SetupBehaviorTree();
                nextDecisionTime = Time.time + decisionCooldown;
                isAiReady = true; // Marca a IA como pronta
            }
            else
            {
                return;
            }
        }


        // A IA só corre se estiver pronta e no servidor
        // (A verificação 'IsServer' já está em 'isAiReady')
        if (tree == null || Time.time < nextDecisionTime)
        {
            return;
        }

        nextDecisionTime = Time.time + decisionCooldown;

        // Avalia a árvore de comportamento
        tree.Evaluate();
    }


    //  CONSTRUÇÃO DA ÁRVORE DE COMPORTAMENTO 
    void SetupBehaviorTree()
    {
        tree = new BTSelector(this, new List<BTNode>
        {
            new BTSequence(this, new List<BTNode>
            {
                new BTCheck(this, Check_ShouldBuildTower),
                new BTAction(this, Action_BuildBestTower)
            }),
            new BTSequence(this, new List<BTNode>
            {
                new BTCheck(this, () => Check_HasMoney(80)),
                new BTAction(this, Action_SendTropaTanque),
                new BTAction(this, Action_SendTropaCavalo)
            }),
            new BTSequence(this, new List<BTNode>
            {
                new BTCheck(this, () => Check_HasMoney(45)),
                new BTAction(this, Action_SendTropaCavalo),
                new BTAction(this, Action_SendTropaNormal)
            }),
            new BTSequence(this, new List<BTNode>
            {
                new BTCheck(this, () => Check_HasMoneyForTroop(TroopType.Normal)),
                new BTAction(this, Action_SendTropaNormal)
            })
        });
    }

    // FUNÇÕES DE VERIFICAÇÃO
    private int GetAIMoney()
    {
        return CurrencySystemMP.Instance.MoneyJogadorB;
    }
    private bool Check_HasMoney(int amount)
    {
        return GetAIMoney() >= amount;
    }
    private bool Check_ShouldBuildTower()
    {
        int towerCount = 0;
        foreach (var spot in aiTowerSpots)
        {
            if (spot.isOccupied.Value) towerCount++;
        }
        if (towerCount >= 3)
            return false;
        if (FindFreeTowerSpot() == null)
            return false;
        return GetAIMoney() >= towerData.normalTowerCost;
    }
    private TowerSpotMP FindFreeTowerSpot()
    {
        foreach (var spot in aiTowerSpots)
        {
            if (!spot.isOccupied.Value)
            {
                return spot;
            }
        }
        return null;
    }
    private bool Check_HasMoneyForTroop(TroopType type)
    {
        if (type == TroopType.Normal) return Check_HasMoney(troopData.custoTropaNormal);
        if (type == TroopType.Tanque) return Check_HasMoney(troopData.custoTropaTanque);
        if (type == TroopType.Cavalo) return Check_HasMoney(troopData.custoCavalo);
        return false;
    }

    //FUNÇÕES DE AÇÃO
    private NodeState Action_BuildBestTower()
    {
        TowerSpotMP spot = FindFreeTowerSpot();
        if (spot == null) return NodeState.FAILURE;
        Vector3 pos = spot.transform.position + new Vector3(0f, 2f, 0f);
        if (Check_HasMoney(towerData.piercingTowerCost))
        {
            return TryBuild(towerData.piercingTowerPrefabId, towerData.piercingTowerCost, pos, spot.NetworkObjectId);
        }
        else if (Check_HasMoney(towerData.fireTowerCost))
        {
            return TryBuild(towerData.fireTowerPrefabId, towerData.fireTowerCost, pos, spot.NetworkObjectId);
        }
        else if (Check_HasMoney(towerData.normalTowerCost))
        {
            return TryBuild(towerData.normalTowerPrefabId, towerData.normalTowerCost, pos, spot.NetworkObjectId);
        }
        return NodeState.FAILURE;
    }
    private NodeState Action_SendTropaNormal()
    {
        return TrySend(troopData.prefabIdTropaNormal, troopData.custoTropaNormal);
    }
    private NodeState Action_SendTropaTanque()
    {
        return TrySend(troopData.prefabIdTropaTanque, troopData.custoTropaTanque);
    }
    private NodeState Action_SendTropaCavalo()
    {
        return TrySend(troopData.prefabIdCavalo, troopData.custoCavalo);
    }


    private NodeState TrySend(int prefabId, int cost)
    {
        bool success = GameServerLogic.Instance.TrySpawnTroop(aiClientId, prefabId, cost, 1);
        return success ? NodeState.SUCCESS : NodeState.FAILURE;
    }
    private NodeState TryBuild(int prefabId, int cost, Vector3 pos, ulong spotId)
    {
        bool success = GameServerLogic.Instance.TryBuildTower(aiClientId, prefabId, cost, pos, spotId);
        return success ? NodeState.SUCCESS : NodeState.FAILURE;
    }
    private enum TroopType { Normal, Tanque, Cavalo }
}