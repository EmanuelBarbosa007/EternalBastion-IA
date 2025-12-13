using UnityEngine;
using UnityEngine.UI;

public class TargetIndicator : MonoBehaviour
{
    [Header("Configurações")]
    public Transform target; // O alvo (Base ou Portal)
    public float edgeBuffer = 50f; // Margem da borda da tela
    public bool rotateTowardsTarget = true; // Se a seta deve rodar

    private Camera mainCamera;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        mainCamera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        // Tenta pegar o CanvasGroup para controlar opacidade (opcional)
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject); // Se o alvo for destruído, remove a seta
            return;
        }

        UpdateIndicatorPosition();
    }

    void UpdateIndicatorPosition()
    {
        // Converter posição do mundo para posição do ecrã
        Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

        // Verificar se o alvo está atrás da câmara
        bool isBehind = screenPos.z < 0;

        // Se estiver atrás, invertemos a posição para que a seta aponte para o lado oposto corretamente
        if (isBehind)
        {
            screenPos *= -1;
        }

        // Converter para coordenadas locais do Canvas pode ser complexo, 
        // então vamos trabalhar com limites do ecrã (Screen Width/Height).

        // Calcular o centro do ecrã
        Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;

        // Limites da tela com a margem (buffer)
        Vector3 bounds = screenCenter * 2;
        bounds.x -= edgeBuffer;
        bounds.y -= edgeBuffer;

        // Verificar se está dentro da visão
        bool onScreen = screenPos.x > edgeBuffer && screenPos.x < bounds.x &&
                        screenPos.y > edgeBuffer && screenPos.y < bounds.y && !isBehind;

        if (onScreen)
        {
            // --- ALVO VISÍVEL ---
            // A seta fica exatamente em cima do alvo
            rectTransform.position = screenPos;

            // Opcional: Podes rodar a seta para baixo (apontar para o objeto) ou deixá-la fixa
            if (rotateTowardsTarget)
                rectTransform.rotation = Quaternion.Euler(0, 0, 0); // Ajusta se a tua sprite apontar para cima/baixo
        }
        else
        {
            // --- ALVO FORA DE VISÃO ---

            // Calcular direção do centro até à posição do alvo (mesmo que projetada fora do ecrã)
            Vector3 dir = (screenPos - screenCenter).normalized;

            // Calcular a inclinação da linha (declive)
            // y = mx -> m = y/x
            Vector3 clampedPos = screenCenter;

            // Matemática para "prender" a seta nas bordas (Clamping)
            float divX = (Screen.width - 2 * edgeBuffer) / 2f;
            float divY = (Screen.height - 2 * edgeBuffer) / 2f;

            // Evitar divisão por zero
            if (dir.x != 0)
            {
                // Calcular onde o vetor interseta a borda vertical ou horizontal
                float slope = dir.y / dir.x;

                // Tentar intersetar com as bordas verticais (esquerda/direita)
                if (Mathf.Abs(slope) < divY / divX)
                {
                    // Toca nas laterais
                    if (dir.x > 0) clampedPos.x = Screen.width - edgeBuffer;
                    else clampedPos.x = edgeBuffer;

                    clampedPos.y = screenCenter.y + slope * (clampedPos.x - screenCenter.x);
                }
                else
                {
                    // Toca no topo ou fundo
                    if (dir.y > 0) clampedPos.y = Screen.height - edgeBuffer;
                    else clampedPos.y = edgeBuffer;

                    clampedPos.x = screenCenter.x + (clampedPos.y - screenCenter.y) / slope;
                }
            }
            else
            {
                // Caso vertical perfeito
                clampedPos.x = screenCenter.x;
                clampedPos.y = dir.y > 0 ? Screen.height - edgeBuffer : edgeBuffer;
            }

            rectTransform.position = clampedPos;

            // Rodar a seta para apontar para fora do ecrã (na direção do alvo)
            if (rotateTowardsTarget)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                // O "- 90" depende da orientação original da tua sprite. 
                // Se a seta aponta para CIMA na imagem original, usa -90.
                rectTransform.rotation = Quaternion.Euler(0, 0, angle -1);
            }
        }
    }
}