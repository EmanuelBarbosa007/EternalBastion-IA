using UnityEngine;
using TMPro;

public class GameSpeedManager : MonoBehaviour
{
    [Header("Speed Settings")]
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;
    public float superFastSpeed = 4f; // Nova variável para o x4

    [Header("UI")]
    public TextMeshProUGUI speedButtonText;

    // Mudamos de bool para int para controlar 3 estados (0, 1, 2)
    // 0 = x1, 1 = x2, 2 = x4
    private int currentSpeedIndex = 0;

    void Start()
    {
        // Garante que o jogo começa na velocidade normal (índice 0)
        currentSpeedIndex = 0;
        ApplyCurrentSpeed();
    }

    public void ToggleSpeed()
    {
        // Não faz nada se o jogo estiver pausado
        if (EscMenuManager.IsGamePaused)
        {
            return;
        }

        // Aumenta o índice. Se passar de 2, volta para 0.
        // Ciclo: x1 -> x2 -> x4 -> x1 ...
        currentSpeedIndex++;
        if (currentSpeedIndex > 2)
        {
            currentSpeedIndex = 0;
        }

        ApplyCurrentSpeed();
    }

    public void ApplyCurrentSpeed()
    {
        // Se o jogo estiver pausado, Time.timeScale deve ser 0
        if (EscMenuManager.IsGamePaused)
        {
            Time.timeScale = 0f;
            return;
        }

        // Verifica qual é o índice atual e aplica a velocidade correspondente
        switch (currentSpeedIndex)
        {
            case 0: // Velocidade Normal (x1)
                Time.timeScale = normalSpeed;
                if (speedButtonText != null) speedButtonText.text = "x1";
                break;

            case 1: // Rápido (x2)
                Time.timeScale = fastSpeed;
                if (speedButtonText != null) speedButtonText.text = "x2";
                break;

            case 2: // Muito Rápido (x4)
                Time.timeScale = superFastSpeed;
                if (speedButtonText != null) speedButtonText.text = "x4";
                break;
        }
    }
}