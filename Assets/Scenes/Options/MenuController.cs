using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Painéis Principais")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject gameModesMenu;

    [Header("Painéis de História")]
    public GameObject singleplayerStoryPanel; // NOVO: Painel da história Singleplayer
    public GameObject multiplayerStoryPanel;  // NOVO: Painel da história Multiplayer

    // --- Navegação: Menu Principal <-> Modos de Jogo ---

    public void OpenGameModes()
    {
        mainMenu.SetActive(false);
        gameModesMenu.SetActive(true);
    }

    public void CloseGameModes()
    {
        gameModesMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // --- Navegação: Modos de Jogo <-> Histórias (NOVO) ---

    // Atribuir ao botão "História Singleplayer"
    public void OpenSingleplayerStory()
    {
        gameModesMenu.SetActive(false);
        singleplayerStoryPanel.SetActive(true);
    }

    // Atribuir ao botão "História Multiplayer"
    public void OpenMultiplayerStory()
    {
        gameModesMenu.SetActive(false);
        multiplayerStoryPanel.SetActive(true);
    }

    // Atribuir ao botão "Voltar" DENTRO dos painéis de história
    // Esta função serve para ambos os painéis, pois fecha os dois e abre o menu de modos
    public void BackToGameModes()
    {
        singleplayerStoryPanel.SetActive(false);
        multiplayerStoryPanel.SetActive(false);
        gameModesMenu.SetActive(true);
    }

    // --- Navegação: Opções ---

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackToMain()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    // --- Carregamento de Cenas ---

    public void StartStandardGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void EasyGame()
    {
        SceneManager.LoadScene("EasyMode");
    }

    public void HardGame()
    {
        SceneManager.LoadScene("HardMode");
    }


    public void MultiplayerGame()
    {
        SceneManager.LoadScene("MultiplayerScene");
    }

    public void PveGame()
    {
        SceneManager.LoadScene("PvEScene");
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}