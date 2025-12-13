using UnityEngine;
using Unity.Netcode;
using TMPro;

public class CurrencySystemMP : NetworkBehaviour
{
    public static CurrencySystemMP Instance;

    [Header("Modo de Jogo")]
    public bool modoPvE = false; // A tal "caixinha" para selecionar se é PvE

    [Header("Configurações Iniciais")]
    public int startingMoney = 150;

    [Header("Renda Passiva (Moedas por Tempo)")]
    public bool ativarRendaPassiva = true;
    public int moedasPorRonda = 100;     // Quantidade de moedas a dar
    public float tempoParaRenda = 30f;   // Intervalo de tempo em segundos
    private float temporizadorRenda = 0f;

    // Referências aos textos de UI de cada jogador
    public TextMeshProUGUI moneyTextA;
    public TextMeshProUGUI moneyTextB;

    private NetworkVariable<int> moneyJogadorA = new NetworkVariable<int>(); // Host (ID 0)
    private NetworkVariable<int> moneyJogadorB = new NetworkVariable<int>(); // Client (ID 1+)

    public int MoneyJogadorB => moneyJogadorB.Value;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            moneyJogadorA.Value = startingMoney;
            moneyJogadorB.Value = startingMoney;
        }

        // Todos os clientes atualizam o UI quando o dinheiro muda
        moneyJogadorA.OnValueChanged += (prev, next) => UpdateUI();
        moneyJogadorB.OnValueChanged += (prev, next) => UpdateUI();

        // Atualiza a UI assim que nasce para garantir que os nomes (Host/Bot) aparecem logo
        UpdateUI();
    }

    void Update()
    {
        // A lógica de dar dinheiro acontece APENAS no servidor
        if (!IsServer) return;

        // Se a renda passiva estiver desligada, não faz nada
        if (!ativarRendaPassiva) return;

        // Precisamos verificar se o GameManager existe e se o jogo já começou
        if (GameManagerMP.Instance == null || !GameManagerMP.Instance.jogoIniciado.Value) return;

        // Incrementa o temporizador
        temporizadorRenda += Time.deltaTime;

        if (temporizadorRenda >= tempoParaRenda)
        {
            temporizadorRenda = 0f; // Reseta o tempo
            DarRendaPassiva();
        }
    }

    private void DarRendaPassiva()
    {
        // Adiciona dinheiro ao Jogador A 
        moneyJogadorA.Value += moedasPorRonda;

        // Adiciona dinheiro ao Jogador B 
        moneyJogadorB.Value += moedasPorRonda;

        Debug.Log($"Renda Passiva: +{moedasPorRonda} moedas para ambos os jogadores.");
    }

    public void AddMoney(ulong clientId, int amount)
    {
        if (!IsServer) return;

        if (clientId == 0) // Jogador A (Host)
        {
            moneyJogadorA.Value += amount;
        }
        else // Jogador B
        {
            moneyJogadorB.Value += amount;
        }
    }

    public bool SpendMoney(ulong clientId, int amount)
    {
        if (!IsServer) return false;

        if (clientId == 0) // Jogador A
        {
            if (moneyJogadorA.Value >= amount)
            {
                moneyJogadorA.Value -= amount;
                return true;
            }
        }
        else // Jogador B
        {
            if (moneyJogadorB.Value >= amount)
            {
                moneyJogadorB.Value -= amount;
                return true;
            }
        }
        return false; // Não tem dinheiro
    }

    public int GetMoney(ulong clientId)
    {
        if (clientId == 0) // Jogador A
        {
            return moneyJogadorA.Value;
        }
        else // Jogador B
        {
            return moneyJogadorB.Value;
        }
    }

    // --- UI ATUALIZADA ---
    void UpdateUI()
    {
        // Define os nomes baseados na checkbox "modoPvE"
        string nomeA = modoPvE ? "Player" : "Host";
        string nomeB = modoPvE ? "Bot" : "Client";

        if (moneyTextA != null)
            moneyTextA.text = $"{nomeA}: {moneyJogadorA.Value}";

        if (moneyTextB != null)
            moneyTextB.text = $"{nomeB}: {moneyJogadorB.Value}";
    }
}