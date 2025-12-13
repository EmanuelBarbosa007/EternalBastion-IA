using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TowerPlacementUI : MonoBehaviour
{
    public static TowerPlacementUI Instance;

    [Header("UI")]
    public GameObject panel;
    public Button basicTowerButton;
    public Button fireTowerButton;
    public Button piercingTowerButton;
    public Button closeButton;

    [Header("Prefabs das torres")]
    public GameObject basicTowerPrefab;
    public GameObject fireTowerPrefab;
    public GameObject piercingTowerPrefab;


    private TowerSpot currentSpot;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        // A função BuildTower vai buscar o custo ao prefab
        basicTowerButton.onClick.AddListener(() => BuildTower(basicTowerPrefab));
        fireTowerButton.onClick.AddListener(() => BuildTower(fireTowerPrefab));
        piercingTowerButton.onClick.AddListener(() => BuildTower(piercingTowerPrefab));
        closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPanel(TowerSpot spot)
    {
        // Se o painel já estiver aberto, ignora selecionar outro slot
        if (panel.activeSelf)
            return;

        currentSpot = spot;
        StartCoroutine(OpenPanelNextFrame());
    }

    private IEnumerator OpenPanelNextFrame()
    {
        yield return null; // espera 1 frame
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentSpot = null;
    }

    void BuildTower(GameObject towerPrefab)
    {
        if (currentSpot == null || towerPrefab == null) return;

        // Vai buscar o script 'Tower' ao prefab para saber o custo
        Tower towerComponent = towerPrefab.GetComponent<Tower>();
        if (towerComponent == null)
        {
            Debug.LogError("O prefab da torre não tem o script 'Tower'!");
            ClosePanel();
            return;
        }

        // 2. Obtém o custo de Nível 1 definido no prefab
        int cost = towerComponent.costLevel1;

        // 3. Verifica se tem dinheiro
        if (CurrencySystem.Money >= cost)
        {
            CurrencySystem.SpendMoney(cost);

            // Posição de spawn 
            Vector3 spawnPos = currentSpot.transform.position + new Vector3(0f, 2f, 0f);


            GameObject newTowerGO = Instantiate(towerPrefab, spawnPos, Quaternion.identity);


            Tower newTower = newTowerGO.GetComponent<Tower>();

            // Liga o spot à torre
            currentSpot.isOccupied = true;
            currentSpot.currentTower = newTower;

            // Liga a torre ao spot e define o investimento inicial
            newTower.myTowerSpot = currentSpot;
            newTower.totalInvested = cost;
        }
        else
        {
            Debug.Log("Moedas insuficientes para comprar esta torre!");
        }

        ClosePanel();
    }
}