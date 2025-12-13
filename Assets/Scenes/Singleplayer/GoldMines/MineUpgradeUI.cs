using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; 

public class MineUpgradeUI : MonoBehaviour
{
    public static MineUpgradeUI Instance;

    public GameObject panel;
    public Button upgradeButton;
    public Button sellButton;

    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI sellValueText;
    public TextMeshProUGUI statsText;

    private GoldMine currentMine;

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
    }


    void Update()
    {
        if (panel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; // Se foi, não faz nada
            }

            // Verifica se o clique foi na própria mina que está selecionada
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Usamos 100f como distância de raycast, igual ao seu script de torre
            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Se acertámos na mina que abriu este painel, não o fecha
                if (currentMine != null && hit.collider.gameObject == currentMine.gameObject)
                {
                    return;
                }
            }

            // Se clicou fora de tudo  fecha o painel
            ClosePanel();
        }
    }


    public void OpenPanel(GoldMine mine)
    {
        currentMine = mine;
        UpdateUI();
        panel.SetActive(true);

        if (TowerUpgradeUI.Instance != null)
        {
            TowerUpgradeUI.Instance.ClosePanel();
        }
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentMine = null;
    }

    // Método para ser chamado pelo CurrencySystem
    public void OnMoneyChanged()
    {
        if (panel.activeSelf)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (currentMine == null) return;

        statsText.text = $"Gera: {currentMine.GetCurrentProduction()} moedas\na cada {currentMine.GetCurrentInterval()} seg.";
        sellValueText.text = $"Vender\n+{currentMine.GetSellValue()}";

        if (currentMine.currentLevel >= currentMine.maxLevel)
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "NÍVEL MÁX.";
        }
        else
        {
            int upgradeCost = currentMine.GetNextUpgradeCost();
            upgradeCostText.text = $"Melhorar\n{upgradeCost}";
            upgradeButton.interactable = (CurrencySystem.Money >= upgradeCost);
        }
    }

    private void OnUpgradeClicked()
    {
        if (currentMine != null)
        {
            currentMine.Upgrade();
        }
    }

    private void OnSellClicked()
    {
        if (currentMine != null)
        {
            currentMine.Sell();
        }
        ClosePanel();
    }


    public bool IsPanelActive()
    {
        return panel.activeSelf;
    }
}