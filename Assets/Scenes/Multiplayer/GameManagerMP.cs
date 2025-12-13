using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagerMP : NetworkBehaviour
{
    public static GameManagerMP Instance;

    [Header("Modo de Jogo")]
    [Tooltip("Se for 'true', o jogo inicia no modo PvE contra a IA (clientId 1)")]
    public bool modoPvE = false;

    [Header("Referências de Jogo")]
    public BaseHealthMP baseJogadorA;
    public BaseHealthMP baseJogadorB;

    [Header("Controlo de Estado")]
    public NetworkVariable<bool> jogoIniciado = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> jogoAcabou = new NetworkVariable<bool>(false);

    // --- NOVA VARIÁVEL DE CONTAGEM ---
    private int votosParaRestart = 0;
    private bool euJaVotei = false; // Controlo local para não clicar 2 vezes

    [Header("UI Fim de Jogo")]
    public GameObject fimDeJogoPanel;
    public TextMeshProUGUI textoVencedor;
    public Button restartButton;
    // REFERÊNCIA NOVA: O texto que está DENTRO do botão
    public TextMeshProUGUI textoBotaoRestart;

    [Header("UI de Espera")]
    public GameObject painelEsperandoJogador;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            baseJogadorA.donoDaBaseClientId = NetworkManager.Singleton.LocalClientId;

            if (modoPvE)
            {
                baseJogadorB.donoDaBaseClientId = 1;
                jogoIniciado.Value = true;
            }
            else
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            }
        }

        if (IsClient)
        {
            jogoIniciado.OnValueChanged += HandleJogoIniciado;
            HandleJogoIniciado(false, jogoIniciado.Value);
        }

        // Configuração do UI para todos (Server e Client)
        if (fimDeJogoPanel != null) fimDeJogoPanel.SetActive(false);

        if (restartButton != null)
        {
            // Removemos listeners antigos e adicionamos o novo sistema de votação
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartButtonPressed);
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer || modoPvE) return;

        if (NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            baseJogadorB.donoDaBaseClientId = clientId;
            jogoIniciado.Value = true;
        }
    }

    private void HandleJogoIniciado(bool anterior, bool novo)
    {
        if (novo == true)
        {
            if (painelEsperandoJogador != null)
                painelEsperandoJogador.SetActive(false);
        }
        else
        {
            if (painelEsperandoJogador != null)
                painelEsperandoJogador.SetActive(true);
        }
    }

    public void BaseDestruida(ulong clientIdDoJogadorQuePerdeu)
    {
        if (!IsServer || jogoAcabou.Value) return;

        jogoAcabou.Value = true;

        ulong vencedorClientId = (clientIdDoJogadorQuePerdeu == baseJogadorA.donoDaBaseClientId)
            ? baseJogadorB.donoDaBaseClientId
            : baseJogadorA.donoDaBaseClientId;

        string nomeVencedor;

        if (modoPvE && vencedorClientId == 1) nomeVencedor = "IA";
        else if (modoPvE && vencedorClientId == 0) nomeVencedor = "Jogador A (Host)";
        else nomeVencedor = (vencedorClientId == 0) ? "Jogador A (Host)" : $"Jogador B (Client {vencedorClientId})";

        // Reinicia o texto do botão para o estado original antes de mostrar
        ResetarTextoBotaoClientRpc();

        NotificarClientesDoVencedorClientRpc(nomeVencedor);
    }

    [ClientRpc]
    private void NotificarClientesDoVencedorClientRpc(string nomeVencedor)
    {
        Time.timeScale = 0f;

        if (fimDeJogoPanel != null)
        {
            fimDeJogoPanel.SetActive(true);
            if (textoVencedor != null)
                textoVencedor.text = $"O {nomeVencedor} VENCEU!";
        }
    }

    // --- SISTEMA DE RESTART COM VOTAÇÃO ---

    public void OnRestartButtonPressed()
    {
        // Se eu já cliquei, não faz nada
        if (euJaVotei) return;

        euJaVotei = true;

        // Bloqueia o botão visualmente (opcional)
        restartButton.interactable = false;

        // Avisa o servidor que eu quero reiniciar
        SolicitarRestartServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SolicitarRestartServerRpc(ServerRpcParams rpcParams = default)
    {
        // Incrementa votos no servidor
        votosParaRestart++;

        int totalJogadores = NetworkManager.Singleton.ConnectedClients.Count;

        // Se for PvE (1 jogador) ou se todos os jogadores (PvP) já votaram
        if (votosParaRestart >= totalJogadores)
        {
            ExecutarRestart();
        }
        else
        {
            // Atualiza o texto para todos mostrarem "Aguardando 1/2..."
            AtualizarTextoBotaoClientRpc(votosParaRestart, totalJogadores);
        }
    }

    [ClientRpc]
    private void AtualizarTextoBotaoClientRpc(int votosAtuais, int totalNecessario)
    {
        if (textoBotaoRestart != null)
        {
            textoBotaoRestart.text = $"Aguardando ({votosAtuais}/{totalNecessario})";
        }
    }

    [ClientRpc]
    private void ResetarTextoBotaoClientRpc()
    {
        // Garante que o texto volta ao normal quando o painel abre
        if (textoBotaoRestart != null)
        {
            textoBotaoRestart.text = "Reiniciar Jogo";
        }
        if (restartButton != null) restartButton.interactable = true;
        euJaVotei = false;
    }

    private void ExecutarRestart()
    {
        Time.timeScale = 1f;

        // Carrega a cena. Como o NetworkManager persiste, os clientes reconectam automaticamente à nova cena
        NetworkManager.Singleton.SceneManager.LoadScene(
            SceneManager.GetActiveScene().name,
            LoadSceneMode.Single);
    }
}