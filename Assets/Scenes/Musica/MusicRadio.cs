using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MusicRadio : MonoBehaviour
{
    public static MusicRadio instance;

    [Header("Configurações de Áudio")]
    public AudioSource audioSource;
    public AudioClip[] musicList;
    private int currentTrackIndex = 0;

    [Header("UI References")]
    public GameObject painelControlos;
    public TextMeshProUGUI songNameText;
    // O Slider foi removido daqui

    [Header("Animação do Botão")]
    public RectTransform botaoRect;
    public float posYFechado = -460f;
    public float posYAberto = -310f;

    private bool isPanelOpen = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        painelControlos.SetActive(false);

        // Removemos a configuração do volumeSlider aqui

        if (musicList.Length > 0)
        {
            PlayTrack(0);
        }
    }

    void Update()
    {
        // Só passa para a próxima se a música não estiver a tocar E se não estiver em PAUSA (tempo no fim)
        // Nota: Quando fazemos Pause(), o isPlaying fica false, mas o time mantém-se.
        // Por isso verificamos se o time é 0 ou se chegou ao fim do clip.
        if (!audioSource.isPlaying && audioSource.time == 0)
        {
            // Proteção extra: só avança se o clip tiver realmente terminado e não apenas pausado no inicio
            if (audioSource.clip != null && audioSource.time >= audioSource.clip.length)
            {
                NextTrack();
            }
            // Na maioria dos casos simples, o código antigo funcionava, mas este é mais seguro para o Pause.
            else if (audioSource.time == 0 && !isPausedManual)
            {
                // Se estava a tocar e acabou (e não foi pausa manual no inicio)
                NextTrack();
            }
        }
    }

    // Variável para controlar se o jogador pausou manualmente
    private bool isPausedManual = false;

    // --- NOVA FUNÇÃO DE PAUSA ---
    public void TogglePause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPausedManual = true;
        }
        else
        {
            audioSource.UnPause();
            isPausedManual = false;
        }
    }

    public void TogglePanel()
    {
        isPanelOpen = !isPanelOpen;
        painelControlos.SetActive(isPanelOpen);

        Vector2 novaPosicao = botaoRect.anchoredPosition;

        if (isPanelOpen)
        {
            novaPosicao.y = posYAberto;
        }
        else
        {
            novaPosicao.y = posYFechado;
        }

        botaoRect.anchoredPosition = novaPosicao;
    }

    public void NextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= musicList.Length) currentTrackIndex = 0;
        PlayTrack(currentTrackIndex);
    }

    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = musicList.Length - 1;
        PlayTrack(currentTrackIndex);
    }

    private void PlayTrack(int index)
    {
        if (musicList.Length == 0) return;

        audioSource.clip = musicList[index];
        audioSource.Play();
        isPausedManual = false; // Reset da pausa ao mudar de música

        if (songNameText != null)
            songNameText.text = musicList[index].name;
    }
}