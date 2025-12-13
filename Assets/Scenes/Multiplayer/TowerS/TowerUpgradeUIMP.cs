using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.EventSystems;
using System.Collections; // Necessário para a Corrotina

public class TowerUpgradeUIMP : MonoBehaviour
{
    public static TowerUpgradeUIMP Instance;

    public GameObject uiPanel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Configurações UI")]
    [Tooltip("Tempo de espera para evitar cliques acidentais ao abrir")]
    public float inputDelay = 0.3f; // O atraso de segurança

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI towerNameText;

    private TowerMP currentTower;
    private TowerSpotMP currentSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeTower);

        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Update()
    {
        if (!uiPanel.activeInHierarchy || currentTower == null)
            return;

        // Lógica para fechar ao clicar fora (mantida igual)
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == currentTower.gameObject)
                    return;
            }

            ClosePanel();
        }
    }

    public void OpenPanel(TowerMP tower, TowerSpotMP spot)
    {
        currentTower = tower;
        currentSpot = spot;
        uiPanel.SetActive(true);

        // Atualiza os textos primeiro
        UpdateUI_Texts();

        // Inicia a rotina que gere os botões com segurança
        StartCoroutine(EnableButtonsRoutine());
    }

    // --- NOVA CORROTINA DE SEGURANÇA ---
    IEnumerator EnableButtonsRoutine()
    {
        // 1. Bloqueia todos os botões imediatamente
        if (upgradeButton != null) upgradeButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        // 2. Espera o tempo de segurança
        yield return new WaitForSeconds(inputDelay);

        // 3. Reativa os botões simples
        if (sellButton != null) sellButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;

        // 4. Reativa o botão de Upgrade COM LÓGICA INTELIGENTE
        // Só ativa se a torre ainda não estiver no nível máximo
        if (currentTower != null && upgradeButton != null)
        {
            // Se o nível for menor que 3 (assumindo que 3 é o máximo baseado no teu código)
            if (currentTower.level.Value < 3)
            {
                upgradeButton.interactable = true;
            }
            else
            {
                // Se já for nível máximo, mantém desativado
                upgradeButton.interactable = false;
            }
        }
    }

    public void ClosePanel()
    {
        uiPanel.SetActive(false);
        currentTower = null;
        currentSpot = null;
    }

    // Separei a atualização de Texto da atualização de Botões para facilitar
    void UpdateUI_Texts()
    {
        if (currentTower == null) return;

        if (towerNameText != null)
            towerNameText.text = $"{currentTower.towerName} (Level {currentTower.level.Value})";

        int sellAmount = currentTower.totalInvested / 2;
        if (sellValueText != null)
            sellValueText.text = $"Vender\n{sellAmount} Moedas";

        if (upgradeCostText != null)
        {
            if (currentTower.level.Value == 1)
                upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel2} Moedas";
            else if (currentTower.level.Value == 2)
                upgradeCostText.text = $"Melhorar\n{currentTower.upgradeCostLevel3} Moedas";
            else
                upgradeCostText.text = "NÍVEL MÁXIMO";
        }
    }

    private void UpgradeTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        PlayerNetwork.LocalInstance.RequestUpgradeTowerServerRpc(
            currentTower.NetworkObjectId
        );

        ClosePanel();
    }

    private void SellTower()
    {
        if (currentTower == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        PlayerNetwork.LocalInstance.RequestSellTowerServerRpc(
            currentTower.NetworkObjectId,
            currentSpot.NetworkObjectId
        );

        ClosePanel();
    }
}