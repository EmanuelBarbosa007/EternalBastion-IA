using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinePlacementUI : MonoBehaviour
{
    public GameObject panel;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI costText;

    private DebrisSpot currentDebrisSpot;

    void Start()
    {
        panel.SetActive(false);

        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    public void OpenPanel(DebrisSpot spot)
    {
        currentDebrisSpot = spot;
        costText.text = "Construir Mina?\nCusto: " + spot.buildCost;

        // Verifica o botão usando a variável estática
        yesButton.interactable = (CurrencySystem.Money >= spot.buildCost);

        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentDebrisSpot = null;
    }

    private void OnYesClicked()
    {
        // O CurrencySystem.SpendMoney() já faz a verificação e gasta o dinheiro
        if (currentDebrisSpot != null && CurrencySystem.SpendMoney(currentDebrisSpot.buildCost))
        {
            currentDebrisSpot.BuildMine();
        }
        ClosePanel();
    }

    private void OnNoClicked()
    {
        ClosePanel();
    }
}