using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencySystem : MonoBehaviour
{
    public static int Money { get; private set; }
    private static CurrencySystem _instance; 

    public int startingMoney = 100;
    public TextMeshProUGUI moneyText;


    void Awake()
    {
        // Configura o Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }


    void Start()
    {
        Money = startingMoney;
        UpdateUI();
    }


    public static void AddMoney(int amount)
    {
        Money += amount;


        if (_instance != null)
        {
            _instance.UpdateUI();

            // Notifica o UI de melhoria que o dinheiro mudou
            if (TowerUpgradeUI.Instance != null)
                TowerUpgradeUI.Instance.OnMoneyChanged();

            if (MineUpgradeUI.Instance != null)
                MineUpgradeUI.Instance.OnMoneyChanged();
        }
    }


    public static bool SpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;


            if (_instance != null)
            {
                _instance.UpdateUI();

                // Notifica o UI de melhoria que o dinheiro mudou
                if (TowerUpgradeUI.Instance != null)
                    TowerUpgradeUI.Instance.OnMoneyChanged();

                if (MineUpgradeUI.Instance != null)
                    MineUpgradeUI.Instance.OnMoneyChanged();
            }
            return true;
        }
        return false; // não tem dinheiro suficiente
    }

    void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "Moedas: " + Money;
    }
}