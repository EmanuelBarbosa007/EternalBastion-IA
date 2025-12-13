using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TowerUpgradeUI : MonoBehaviour
{
    public static TowerUpgradeUI Instance;

    public GameObject uiPanel;
    public TextMeshProUGUI towerNameText;
    public Button upgradeButton;
    public Button sellButton;


    public TextMeshProUGUI upgradeButtonText;
    public TextMeshProUGUI sellButtonText;

    private Tower selectedTower;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        uiPanel.SetActive(false); // Começa escondido
    }

    void Update()
    {
        // fechar o painel se clicar fora
        if (uiPanel.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }


            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (selectedTower.myTowerSpot != null && hit.collider.gameObject == selectedTower.myTowerSpot.gameObject)
                {
                    return; // Clicou na mesma torre, deixa o painel aberto
                }
            }

            // Fecha o painel
            ClosePanel();
        }
    }


    //Abre o painel para uma torre específica
    public void OpenPanel(Tower tower)
    {
        selectedTower = tower;
        uiPanel.SetActive(true);
        UpdateUI();
    }



    // Fecha o painel
    public void ClosePanel()
    {
        selectedTower = null;
        uiPanel.SetActive(false);
    }


    // Atualiza toda a informação no painel (textos, botões)
    void UpdateUI()
    {
        if (selectedTower == null) return;

        // "Piercing Tower (Level 1)"
        towerNameText.text = $"{selectedTower.towerName} (Level {selectedTower.level})";

        // Texto do botão Vender
        int sellAmount = selectedTower.totalInvested / 2;
        sellButtonText.text = $"Vender\n({sellAmount} Moedas)";

        // Lógica do botão Melhorar
        if (selectedTower.level == 1)
        {
            upgradeButtonText.text = $"Melhorar\n({selectedTower.upgradeCostLevel2} Moedas)";
            // Ativa/desativa o botão se tiver dinheiro
            upgradeButton.interactable = CurrencySystem.Money >= selectedTower.upgradeCostLevel2;
        }
        else if (selectedTower.level == 2)
        {
            upgradeButtonText.text = $"Melhorar\n({selectedTower.upgradeCostLevel3} Moedas)";
            upgradeButton.interactable = CurrencySystem.Money >= selectedTower.upgradeCostLevel3;
        }
        else // Nível 3 (Máximo)
        {
            upgradeButtonText.text = "NÍVEL MÁX.";
            upgradeButton.interactable = false;
        }
    }


    // Chamado quando o dinheiro do jogador muda 
    public void OnMoneyChanged()
    {
        if (uiPanel.activeInHierarchy && selectedTower != null)
        {
            UpdateUI();
        }
    }



    public void OnUpgradePressed()
    {
        if (selectedTower != null)
        {
            selectedTower.UpgradeTower();
            // Atualiza o UI para refletir o novo nível (ou se falhou por falta de dinheiro)
            UpdateUI();
        }
    }

    public void OnSellPressed()
    {
        if (selectedTower != null)
        {
            selectedTower.SellTower();
        }
        ClosePanel(); // Torre desapareceu, fecha o painel
    }

    public bool IsPanelActive()
    {
        // Verifica se o painel de UI desta torre está ativo
        return uiPanel.activeSelf;
    }
}