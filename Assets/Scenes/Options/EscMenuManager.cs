using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class EscMenuManager : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject pauseMenuPanel; // painel principal de pausa
    [SerializeField] private GameObject optionsPanel;   // painel de opções

    [Header("Cena do Menu")]
    [SerializeField] private string mainMenuSceneName = "MenuScene";


    [Header("Referências Externas")]
    [SerializeField] private GameSpeedManager gameSpeedManager; // Referência ao script de velocidade

    private bool isPaused = false;
    public static bool IsGamePaused { get; private set; }

    void Start()
    {
        // Garante que o jogo começa a correr e com os menus fechados
        ResumeGame();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Se o menu de opções estiver aberto, ESC fecha
                if (optionsPanel.activeSelf)
                {
                    CloseOptions();
                }
                else
                {
                    // Se só o menu de pausa estiver aberto, fecha tudo e continua o jogo
                    ResumeGame();
                }
            }
            else
            {
                // Se o jogo não estava pausado, pausa-o
                PauseGame();
            }
        }
    }


    public void ResumeGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (optionsPanel != null)
            optionsPanel.SetActive(false); // Garante que as opções também fecham

        isPaused = false;
        IsGamePaused = false;


        // Diz ao GameSpeedManager para aplicar a velocidade correta (1x ou 2x)
        if (gameSpeedManager != null)
        {
            gameSpeedManager.ApplyCurrentSpeed();
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    public void OpenOptions()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false); // Esconde o menu de pausa

        if (optionsPanel != null)
            optionsPanel.SetActive(true);  // Mostra o de opções
    }


    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false); // Esconde as opções

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);  // Mostra o de pausa
    }

    public void GoToMainMenu()
    {
        // Despausar o jogo antes de sair da cena
        Time.timeScale = 1f;
        isPaused = false;
        IsGamePaused = false;

        if (NetworkManager.Singleton != null)
        {
            // Guarda a referência do GameObject antes de fazer shutdown
            GameObject networkManagerGo = NetworkManager.Singleton.gameObject;

            NetworkManager.Singleton.Shutdown();


            Destroy(networkManagerGo);
            Debug.Log("NetworkManager foi desligado e destruído.");
        }

        // Carrega a cena do menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Esta função é chamada pela primeira vez que se prime ESC.
    private void PauseGame()
    {
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        Time.timeScale = 0f; // PAUSA O JOGO
        isPaused = true;
        IsGamePaused = true;
    }
}