using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class TowerSpotMP : NetworkBehaviour
{
    // NOVO: Sincroniza o estado de ocupado do Server para todos os Clientes
    public NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);

    // NOVO: Sincroniza qual torre (pelo seu ID de rede) está neste spot
    public NetworkVariable<ulong> currentTowerNetworkId = new NetworkVariable<ulong>(0);

    // NOVO: Defina isto no Inspector para cada spot
    // (ex: Spots do lado esquerdo são JogadorA, do lado direito são JogadorB)
    public PlayerID donoDoSpot;

    // Guarda a torre em cache para o UI não ter de a procurar
    private TowerMP cachedTower;

    /// <summary>
    /// Esta é a função principal. É chamada no CLIENTE que clica.
    /// </summary>
    private void OnMouseDown()
    {

        Debug.Log("OnMouseDown FOI ATIVADO no spot: " + gameObject.name);
        

        // 1. Impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        // 2. NOVO: Verifica se o jogador local (quem está a jogar neste PC)
        // é o dono deste spot.
        if (PlayerNetwork.LocalInstance == null)
        {
            Debug.LogError("Não consigo encontrar o PlayerNetwork.LocalInstance!");
            return;
        }

        // Descobre se este jogador é o JogadorA ou JogadorB
        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        if (donoDoSpot != localPlayerId)
        {
            Debug.Log("Este spot não é seu!");
            return; // Sai da função
        }

        // 3. NOVO: Lógica de abrir painel,
        // mas agora chama os painéis da versão MP

        if (isOccupied.Value)
        {
            // --- SPOT OCUPADO: ABRE O PAINEL DE UPGRADE ---

            // Procura a torre que está neste spot
            if (cachedTower == null && currentTowerNetworkId.Value != 0)
            {
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(currentTowerNetworkId.Value, out NetworkObject towerNetworkObject))
                {
                    cachedTower = towerNetworkObject.GetComponent<TowerMP>();
                }
            }

            // (Aviso: Você precisará criar o TowerUpgradeUIMP.cs)
            if (cachedTower != null && TowerUpgradeUIMP.Instance != null)
            {
                // Fecha o painel de construção se estiver aberto
                if (TowerPlacementUIMP.Instance != null)
                    TowerPlacementUIMP.Instance.ClosePanel();

                // <<< CORRIGIDO! Esta linha estava comentada. >>>
                TowerUpgradeUIMP.Instance.OpenPanel(cachedTower, this);
            }
        }
        else
        {
            // --- SPOT VAZIO: ABRE O PAINEL DE CONSTRUÇÃO ---

            // (Aviso: Você precisará criar o TowerPlacementUIMP.cs)
            if (TowerPlacementUIMP.Instance != null)
            {
                // Fecha o painel de upgrade se estiver aberto
                if (TowerUpgradeUIMP.Instance != null)
                    TowerUpgradeUIMP.Instance.ClosePanel();

                // Passa 'this' (este próprio script) para o painel de UI
                // O UI precisa de saber o NetworkObjectId deste spot
                // para o enviar no ServerRpc de construção

                // <<< CORRIGIDO! Esta linha estava comentada. >>>
                TowerPlacementUIMP.Instance.OpenPanel(this);
            }
        }
    }

    /// <summary>
    /// Esta função é chamada pelo SERVIDOR (no PlayerNetwork.cs)
    /// depois de uma torre ser construída, para atualizar o estado.
    /// </summary>
    public void SetOcupado(TowerMP torre)
    {
        if (!IsServer) return;

        isOccupied.Value = true;
        currentTowerNetworkId.Value = torre.NetworkObjectId;
    }

    /// <summary>
    /// Esta função é chamada pelo SERVIDOR (num ServerRpc)
    /// depois de uma torre ser vendida.
    /// </summary>
    public void SetVazio()
    {
        if (!IsServer) return;

        isOccupied.Value = false;
        currentTowerNetworkId.Value = 0;
        cachedTower = null;
    }
}