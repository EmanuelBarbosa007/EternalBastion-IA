using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para mudar de cena
using UnityEngine.UI; // Necessário se for mexer em UI, mas aqui usamos CanvasGroup
using System.Collections;

public class SplashManager : MonoBehaviour
{
    [Header("Configurações")]
    public string sceneToLoad = "MenuInicial"; // Nome exato da cena do menu
    public float fadeInDuration = 1.0f;
    public float displayDuration = 2.0f;
    public float fadeOutDuration = 1.0f;

    [Header("Referências")]
    public CanvasGroup logoCanvasGroup;

    private bool hasSkipped = false;

    void Start()
    {
        // Inicia a sequência de animação
        StartCoroutine(SplashSequence());
    }

    void Update()
    {
        // Se o jogador apertar qualquer tecla e ainda não tiver pulado
        if (Input.anyKeyDown && !hasSkipped)
        {
            SkipSplash();
        }
    }

    IEnumerator SplashSequence()
    {
        // 1. Fade In (Aparecer)
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            logoCanvasGroup.alpha = timer / fadeInDuration;
            yield return null; // Espera o próximo frame
        }
        logoCanvasGroup.alpha = 1f; // Garante que fique totalmente visível

        // 2. Esperar (Mostrar o logo)
        yield return new WaitForSeconds(displayDuration);

        // 3. Fade Out (Desaparecer)
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            // Inverso do fade in (vai de 1 para 0)
            logoCanvasGroup.alpha = 1f - (timer / fadeOutDuration);
            yield return null;
        }
        logoCanvasGroup.alpha = 0f;

        // 4. Carregar a próxima cena
        LoadNextScene();
    }

    void SkipSplash()
    {
        hasSkipped = true;
        StopAllCoroutines(); // Para a animação onde estiver
        LoadNextScene();
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}