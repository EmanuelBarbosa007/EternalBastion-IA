using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    public AudioClip clickSound; //som aqui
    private AudioSource sfxSource;

    void Awake()
    {
        //  Lógica Singleton (Só pode haver um destes no jogo)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Torna este objeto imortal
        }
        else
        {
            Destroy(gameObject); // Se já existe um, destrói o novo
            return;
        }

        //  Configura o AudioSource 
        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    //  Deteta quando mudas de cena
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Quando a cena carrega, procura os botões
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Button[] botoes = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button btn in botoes)
        {
            // Remove para não duplicar se algo estranho acontecer
            btn.onClick.RemoveListener(PlaySound);
            // Adiciona o som
            btn.onClick.AddListener(PlaySound);
        }
    }

    void PlaySound()
    {
        // permite que o som toque por cima dele mesmo
        sfxSource.PlayOneShot(clickSound);
    }
}