using UnityEngine;
using System.Collections;

public class GoldMine : MonoBehaviour
{
    [Header("Atributos da Mina")]
    public int currentLevel = 1;
    public int maxLevel = 3;
    public MineLevelStats[] levelStats;

    [Header("Referências")]
    public GameObject debrisSpotPrefab;

    [Tooltip("Um objeto 'filho' vazio que servirá de 'contentor' para os modelos.")] // <-- NOVO
    public Transform visualContainer; // <-- NOVO (Arraste o 'filho' para aqui)

    private MineUpgradeUI upgradeUI;

    // Stats Atuais
    private int coinsPerInterval;
    private float generationInterval;
    private int totalCostSpent;

    void Start()
    {
        upgradeUI = FindAnyObjectByType<MineUpgradeUI>();

        // Aplica os stats E o visual do Nível 1
        ApplyLevelStats(); // <-- Esta função agora também trata do visual

        StartCoroutine(GenerateCoinsRoutine());
    }

    public void InitializeCost(int initialCost)
    {
        totalCostSpent = initialCost;
    }

    void OnMouseDown()
    {
        if (TowerUpgradeUI.Instance != null && TowerUpgradeUI.Instance.IsPanelActive())
        {
            return;
        }

        if (upgradeUI != null)
        {
            upgradeUI.OpenPanel(this);
        }
    }

    void ApplyLevelStats()
    {
        if (currentLevel > 0 && currentLevel <= levelStats.Length)
        {
            MineLevelStats stats = levelStats[currentLevel - 1]; // Obtém os stats do nível atual

            // 1. Atualiza os stats de economia
            coinsPerInterval = stats.coinsPerInterval;
            generationInterval = stats.generationInterval;

            // 2. --- LÓGICA PARA ATUALIZAR O VISUAL ---
            if (visualContainer != null && stats.visualPrefab != null) // <-- NOVO
            {
                // Destrói qualquer modelo que já lá esteja (ex: Nível 1)
                foreach (Transform child in visualContainer) // <-- NOVO
                {
                    Destroy(child.gameObject); // <-- NOVO
                }

                // Instancia o novo modelo (ex: Nível 2)
                // como filho do 'visualContainer'
                Instantiate(stats.visualPrefab, visualContainer.position, visualContainer.rotation, visualContainer); // <-- NOVO
            }
            else if (visualContainer == null)
            {
                Debug.LogError("O 'Visual Container' não está atribuído no prefab da Mina!", this);
            }
            // --- FIM DA LÓGICA DO VISUAL ---
        }
    }

    IEnumerator GenerateCoinsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(generationInterval);
            CurrencySystem.AddMoney(coinsPerInterval);
        }
    }

    public void Upgrade()
    {
        if (currentLevel >= maxLevel) return;

        int upgradeCost = GetNextUpgradeCost();

        if (CurrencySystem.SpendMoney(upgradeCost))
        {
            currentLevel++;
            totalCostSpent += upgradeCost;

            // Esta função agora também vai mudar o visual!
            ApplyLevelStats();

            upgradeUI.UpdateUI();
            Debug.Log("Mina melhorada para nível " + currentLevel);
        }
        else
        {
            Debug.Log("Não tem moedas suficientes para o upgrade.");
        }
    }

    public void Sell()
    {
        int sellAmount = Mathf.RoundToInt(totalCostSpent * 0.7f);
        CurrencySystem.AddMoney(sellAmount);

        if (debrisSpotPrefab != null)
        {
            Instantiate(debrisSpotPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    // --- Métodos Públicos para a UI ---

    public int GetNextUpgradeCost()
    {
        if (currentLevel >= maxLevel) return 0;
        return levelStats[currentLevel].upgradeCost;
    }

    public int GetCurrentProduction()
    {
        return coinsPerInterval;
    }

    public float GetCurrentInterval()
    {
        return generationInterval;
    }

    public int GetSellValue()
    {
        return Mathf.RoundToInt(totalCostSpent * 0.7f);
    }
}
