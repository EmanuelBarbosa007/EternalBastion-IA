using UnityEngine;
using System.Collections.Generic;

public class IndicatorManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject arrowPrefab; // O prefab da UI (Imagem da seta)
    public Transform canvasTransform; // O transform do teu Canvas

    [Header("Target Tags")]
    public string baseTag = "Base";
    public string portalTag = "Portal"; // Ou a tag que usares para os spawners

    void Start()
    {
        CreateIndicators();
    }

    void CreateIndicators()
    {
        // 1. Encontrar a Base
        GameObject baseObj = GameObject.FindGameObjectWithTag(baseTag);
        if (baseObj != null)
        {
            SpawnArrow(baseObj.transform, Color.red); // Cor azul para a base (exemplo)
        }
        else
        {
            Debug.LogWarning("Nenhuma Base encontrada com a tag: " + baseTag);
        }

        // 2. Encontrar todos os Portais
        GameObject[] portals = GameObject.FindGameObjectsWithTag(portalTag);
        foreach (GameObject portal in portals)
        {
            SpawnArrow(portal.transform, Color.blue); // Cor vermelha para portais (exemplo)
        }
    }

    void SpawnArrow(Transform targetTransform, Color color)
    {
        // Instancia a seta no Canvas
        GameObject newArrow = Instantiate(arrowPrefab, canvasTransform);

        // Configura o script do indicador
        TargetIndicator indicator = newArrow.GetComponent<TargetIndicator>();
        if (indicator != null)
        {
            indicator.target = targetTransform;
        }

        // Muda a cor da seta (opcional, se a imagem for branca)
        UnityEngine.UI.Image img = newArrow.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = color;
        }
    }
}