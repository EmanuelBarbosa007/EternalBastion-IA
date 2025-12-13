using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections; // Necessário para usar Corrotinas

public class TowerPlacementUIMP : MonoBehaviour
{
    public static TowerPlacementUIMP Instance;

    public GameObject panel;
    public Button closeButton;

    [Header("Configurações de UI")]
    [Tooltip("Tempo em segundos que os botões ficam bloqueados ao abrir o painel (evita cliques acidentais)")]
    public float inputDelay = 0.3f; // 0.3 segundos é geralmente o ideal

    [Header("Torre Normal")]
    public Button buildNormalTowerButton;
    public int normalTowerPrefabId;
    public int normalTowerCost;

    [Header("Torre de Fogo")]
    public Button buildFireTowerButton;
    public int fireTowerPrefabId;
    public int fireTowerCost;

    [Header("Torre Perfurante (Piercing)")]
    public Button buildPiercingTowerButton;
    public int piercingTowerPrefabId;
    public int piercingTowerCost;

    private TowerSpotMP currentSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Configuração dos Listeners (igual ao teu código original)
        if (buildNormalTowerButton != null)
        {
            buildNormalTowerButton.onClick.AddListener(() => {
                BuildTower(normalTowerPrefabId, normalTowerCost);
            });
        }

        if (buildFireTowerButton != null)
        {
            buildFireTowerButton.onClick.AddListener(() => {
                BuildTower(fireTowerPrefabId, fireTowerCost);
            });
        }

        if (buildPiercingTowerButton != null)
        {
            buildPiercingTowerButton.onClick.AddListener(() => {
                BuildTower(piercingTowerPrefabId, piercingTowerCost);
            });
        }
    }

    // --- FUNÇÃO OPENPANEL MODIFICADA ---
    public void OpenPanel(TowerSpotMP spot)
    {
        currentSpot = spot;
        panel.SetActive(true);

        // Inicia a rotina de segurança para evitar cliques acidentais
        StartCoroutine(EnableButtonsRoutine());
    }

    // Esta rotina desativa os botões temporariamente e reativa após o delay
    IEnumerator EnableButtonsRoutine()
    {
        // 1. Desativa a interação imediatamente
        SetButtonsInteractable(false);

        // 2. Espera o tempo de segurança (ex: 0.3 segundos)
        yield return new WaitForSeconds(inputDelay);

        // 3. Reativa a interação
        SetButtonsInteractable(true);
    }

    // Função auxiliar para ligar/desligar todos os botões de uma vez
    void SetButtonsInteractable(bool state)
    {
        if (buildNormalTowerButton != null) buildNormalTowerButton.interactable = state;
        if (buildFireTowerButton != null) buildFireTowerButton.interactable = state;
        if (buildPiercingTowerButton != null) buildPiercingTowerButton.interactable = state;

        // O botão de fechar pode ficar sempre ativo, ou podes incluir aqui se quiseres
        if (closeButton != null) closeButton.interactable = state;
    }

    public void ClosePanel()
    {
        currentSpot = null;
        panel.SetActive(false);
    }

    public void BuildTower(int towerPrefabId, int cost)
    {
        if (currentSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        Vector3 spawnPos = currentSpot.transform.position + new Vector3(0f, 2f, 0f);

        PlayerNetwork.LocalInstance.RequestBuildTowerServerRpc(
            towerPrefabId,
            cost,
            spawnPos,
            currentSpot.NetworkObjectId
        );

        ClosePanel();
    }
}