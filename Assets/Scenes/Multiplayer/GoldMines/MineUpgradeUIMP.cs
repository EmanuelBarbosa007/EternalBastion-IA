using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections; // Necessário para IEnumerator

public class MineUpgradeUIMP : MonoBehaviour
{
    public static MineUpgradeUIMP Instance;

    public GameObject panel;
    public Button upgradeButton;
    public Button sellButton;
    public Button closeButton;

    [Header("Configurações UI")]
    public float inputDelay = 0.3f; // Tempo de segurança

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI statsText;

    private GoldMineMP currentMine;
    private DebrisSpotMP currentSpot;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        panel.SetActive(false);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        sellButton.onClick.AddListener(OnSellClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    void Update()
    {
        if (!panel.activeInHierarchy || currentMine == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.gameObject == currentMine.gameObject)
                    return;
            }
            ClosePanel();
        }
    }

    public void OpenPanel(GoldMineMP mine, DebrisSpotMP spot)
    {
        currentMine = mine;
        currentSpot = spot;

        panel.SetActive(true);

        // Atualiza textos
        UpdateUI_Texts();

        // Inicia segurança dos botões
        StartCoroutine(EnableButtonsRoutine());
    }

    // --- CORROTINA DE SEGURANÇA ---
    IEnumerator EnableButtonsRoutine()
    {
        // 1. Bloqueia tudo
        if (upgradeButton != null) upgradeButton.interactable = false;
        if (sellButton != null) sellButton.interactable = false;
        if (closeButton != null) closeButton.interactable = false;

        // 2. Espera
        yield return new WaitForSeconds(inputDelay);

        // 3. Reativa Vender e Fechar
        if (sellButton != null) sellButton.interactable = true;
        if (closeButton != null) closeButton.interactable = true;

        // 4. Lógica Inteligente do botão Upgrade
        if (currentMine != null && upgradeButton != null)
        {
            int currentLevel = currentMine.level.Value;

            // Se já for nível máximo, mantém desligado
            if (currentLevel >= currentMine.maxLevel)
            {
                upgradeButton.interactable = false;
            }
            else
            {
                // Se não for máx, verifica se tem dinheiro
                int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
                bool hasMoney = false;

                if (PlayerNetwork.LocalInstance != null)
                    hasMoney = CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) >= upgradeCost;

                upgradeButton.interactable = hasMoney;
            }
        }
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentMine = null;
        currentSpot = null;
    }

    // Separei os textos para serem atualizados instantaneamente
    public void UpdateUI_Texts()
    {
        if (currentMine == null) return;

        int currentLevel = currentMine.level.Value;
        var currentStats = currentMine.levelStats[currentLevel - 1];

        if (statsText != null)
            statsText.text = $"Gera: {currentStats.coinsPerInterval} moedas\na cada {currentStats.generationInterval} seg.";

        if (sellValueText != null)
            sellValueText.text = $"Vender\n+{currentMine.GetSellValue()}";

        if (upgradeCostText != null)
        {
            if (currentLevel >= currentMine.maxLevel)
            {
                upgradeCostText.text = "NÍVEL MÁX.";
            }
            else
            {
                int upgradeCost = currentMine.levelStats[currentLevel].upgradeCost;
                upgradeCostText.text = $"Melhorar\n{upgradeCost}";
            }
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentMine == null) return;
        PlayerNetwork.LocalInstance.RequestUpgradeMineServerRpc(currentMine.NetworkObjectId);
        ClosePanel();
    }

    private void OnSellClicked()
    {
        if (currentMine == null) return;
        PlayerNetwork.LocalInstance.RequestSellMineServerRpc(currentMine.NetworkObjectId, currentSpot.NetworkObjectId);
        ClosePanel();
    }
}