using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Necessário para IEnumerator

public class MinePlacementUIMP : MonoBehaviour
{
    public static MinePlacementUIMP Instance;

    public GameObject panel;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI costText;

    [Header("Configurações UI")]
    public float inputDelay = 0.3f; // Tempo de segurança

    private DebrisSpotMP currentDebrisSpot;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    void Start()
    {
        panel.SetActive(false);
        yesButton.onClick.AddListener(OnYesClicked);
        noButton.onClick.AddListener(OnNoClicked);
    }

    public void OpenPanel(DebrisSpotMP spot)
    {
        currentDebrisSpot = spot;
        costText.text = "Construir Mina?\nCusto: " + spot.buildCost;

        panel.SetActive(true);

        // Inicia a rotina de segurança
        StartCoroutine(EnableButtonsRoutine(spot.buildCost));
    }

    IEnumerator EnableButtonsRoutine(int cost)
    {
        // 1. Bloqueia botões
        if (yesButton != null) yesButton.interactable = false;
        if (noButton != null) noButton.interactable = false;

        // 2. Espera o tempo de segurança
        yield return new WaitForSeconds(inputDelay);

        // 3. Reativa botão "Não"
        if (noButton != null) noButton.interactable = true;

        // 4. Lógica do botão "Sim" (Verifica Dinheiro)
        bool hasMoney = false;
        if (PlayerNetwork.LocalInstance != null)
        {
            hasMoney = CurrencySystemMP.Instance.GetMoney(PlayerNetwork.LocalInstance.OwnerClientId) >= cost;
        }

        if (yesButton != null) yesButton.interactable = hasMoney;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentDebrisSpot = null;
    }

    private void OnYesClicked()
    {
        if (currentDebrisSpot == null) return;
        if (PlayerNetwork.LocalInstance == null) return;

        PlayerNetwork.LocalInstance.RequestBuildMineServerRpc(
            currentDebrisSpot.goldMinePrefabId,
            currentDebrisSpot.buildCost,
            currentDebrisSpot.transform.position,
            currentDebrisSpot.NetworkObjectId
        );

        ClosePanel();
    }

    private void OnNoClicked()
    {
        ClosePanel();
    }
}