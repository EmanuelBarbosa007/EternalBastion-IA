using UnityEngine;
using UnityEngine.UI;

public class ConsumableManager : MonoBehaviour
{
    [Header("Configurações da Bomba")]
    public GameObject bombPrefab;
    public int bombCost = 15;
    public Button bombButton;

    [Header("Configurações do Caminho")]
    public LayerMask pathLayer;
    public string roadTag = "Path";
    public Color highlightColor = Color.yellow;

    private bool isPlacingBomb = false;
    private GameObject[] roadSegments;
    private Color[] originalColors;

    // Nome exato da referência no Shader Graph
    private string colorProperty = "_BaseColor";

    void Start()
    {
        roadSegments = GameObject.FindGameObjectsWithTag(roadTag);

        if (roadSegments.Length > 0)
        {
            originalColors = new Color[roadSegments.Length];
            for (int i = 0; i < roadSegments.Length; i++)
            {
                Renderer rend = roadSegments[i].GetComponent<Renderer>();
                if (rend != null)
                {
                    // CORREÇÃO: Usa GetColor com o nome da propriedade do Shader
                    if (rend.material.HasProperty(colorProperty))
                    {
                        originalColors[i] = rend.material.GetColor(colorProperty);
                    }
                    else
                    {
                        // Se não encontrar, assume branco por segurança
                        originalColors[i] = Color.white;
                    }
                }
            }
        }

        if (bombButton != null)
        {
            bombButton.onClick.AddListener(ToggleBombPlacement);
        }
    }

    void Update()
    {
        if (!isPlacingBomb) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceBomb();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ToggleBombPlacement();
        }
    }

    public void ToggleBombPlacement()
    {
        isPlacingBomb = !isPlacingBomb;
        HighlightRoads(isPlacingBomb);
    }

    void TryPlaceBomb()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, pathLayer))
        {
            // Nota: Certifique-se que o CurrencySystem existe no seu projeto
            // Se der erro aqui, comente a linha if(CurrencySystem...) para testar
            if (CurrencySystem.SpendMoney(bombCost))
            {
                Instantiate(bombPrefab, hit.point, Quaternion.identity);
                ToggleBombPlacement();
            }
            else
            {
                Debug.Log("Dinheiro insuficiente!");
            }
        }
        else
        {
            Debug.Log("Local inválido! Clica no caminho.");
        }
    }

    void HighlightRoads(bool highlight)
    {
        if (roadSegments == null) return;

        for (int i = 0; i < roadSegments.Length; i++)
        {
            Renderer rend = roadSegments[i].GetComponent<Renderer>();
            if (rend != null)
            {
                // CORREÇÃO: Usa SetColor para alterar a propriedade específica
                Color targetColor = highlight ? highlightColor : originalColors[i];
                rend.material.SetColor(colorProperty, targetColor);
            }
        }
    }
}