using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider volumeSlider;
    // Referência para o ícone do botão de mute (opcional, para mudar visualmente)
    public Image muteImage;
    public Sprite mutedSprite;
    public Sprite unmutedSprite;

    private bool isMuted = false;
    private float previousVolume = 0f;

    [Header("Sensibilidade")]
    public Slider sensitivitySlider;
    public static float mouseSensitivity = 1.0f; // Variável estática para acesso fácil

    [Header("Video")]
    // Se for um Toggle (caixa de seleção)
    public Toggle fullscreenToggle;

    void Start()
    {
        // --- Carregar Definições Salvas ---

        // 1. Carregar Sensibilidade
        mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        if (sensitivitySlider != null)
            sensitivitySlider.value = mouseSensitivity;

        // 2. Carregar Volume
        // O slider vai de 0.0001 a 1. Logaritmo de 1 é 0dB.
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        if (volumeSlider != null)
            volumeSlider.value = savedVolume;

        // Aplica o volume inicial
        SetVolume(savedVolume);

        // 3. Carregar Fullscreen
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
    }

    // --- Funções ligadas à UI ---

    public void SetVolume(float sliderValue)
    {
        // Unity AudioMixer funciona em Decibéis (-80 a 0).
        // Usamos Log10 para converter o slider linear (0-1) para logarítmico.
        float volumeInDecibels = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20;

        mainMixer.SetFloat("MasterVolume", volumeInDecibels);

        // Guardar preferência
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Guarda o volume atual antes de mutar
            previousVolume = volumeSlider.value;
            volumeSlider.value = volumeSlider.minValue; // Põe o slider no mínimo

            // Muta o som (-80db)
            mainMixer.SetFloat("MasterVolume", -80f);

            // Troca sprite (opcional)
            if (muteImage != null && mutedSprite != null) muteImage.sprite = mutedSprite;
        }
        else
        {
            // Restaura o volume
            volumeSlider.value = previousVolume;
            SetVolume(previousVolume);

            // Troca sprite (opcional)
            if (muteImage != null && unmutedSprite != null) muteImage.sprite = unmutedSprite;
        }
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        PlayerPrefs.Save();
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    // Se usares um Toggle em vez de botão simples
    public void SetFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}