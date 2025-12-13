using UnityEngine;

public class DebrisSpot : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject goldMinePrefab; // O prefab da Mina de Ouro (Nível 1)
    public int buildCost = 75; // Custo para construir

    [Header("Ajustes de Posição")]
    [Tooltip("Quão mais alto a mina deve 'spawnar' em relação aos destroços.")]
    public float spawnHeightOffset = 1f;
    [Tooltip("Rotação em Y para a mina ficar virada para a frente.")]
    public float spawnYRotation = 180f;

    [Header("Referências da UI")]
    public MinePlacementUI placementUI; // Referência para o painel de construção da mina

    void Start()
    {
        if (placementUI == null)
        {
            // Usa FindAnyObjectByType para evitar o aviso de obsoleto
            placementUI = FindAnyObjectByType<MinePlacementUI>();
        }
    }

    void OnMouseDown()
    {
        // Impede a abertura se outros painéis de UI estiverem ativos
        if ((TowerUpgradeUI.Instance != null && TowerUpgradeUI.Instance.IsPanelActive()) ||
            (MineUpgradeUI.Instance != null && MineUpgradeUI.Instance.IsPanelActive()))
        {
            return;
        }

        if (placementUI != null)
        {
            placementUI.OpenPanel(this);
        }
    }

    // Método chamado pela UI para construir a mina
    public void BuildMine()
    {
        if (goldMinePrefab != null)
        {

            Vector3 spawnPos = transform.position + (Vector3.up * spawnHeightOffset);

            Quaternion spawnRot = transform.rotation * Quaternion.Euler(0, spawnYRotation, 0);

            GameObject mineGO = Instantiate(goldMinePrefab, spawnPos, spawnRot);


            // Passa o custo inicial para o script da mina (para cálculo de venda)
            GoldMine mineScript = mineGO.GetComponent<GoldMine>();
            if (mineScript != null)
            {
                mineScript.InitializeCost(buildCost);
            }

            // Destrói os destroços
            Destroy(gameObject);
        }
    }
}