using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpot : MonoBehaviour
{
    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public Tower currentTower;

    private void OnMouseDown()
    {
        // impede clique se o rato estiver sobre o UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (isOccupied)
        {

            // Se o painel de CONSTRUÇÃO estiver aberto, fecha-o
            if (TowerPlacementUI.Instance != null && TowerPlacementUI.Instance.panel.activeSelf)
            {
                TowerPlacementUI.Instance.ClosePanel();
            }

            // Abre o painel de MELHORIA para esta torre
            if (currentTower != null && TowerUpgradeUI.Instance != null)
            {
                TowerUpgradeUI.Instance.OpenPanel(currentTower);
            }
        }
        else
        {
            // Clicámos num spot que esta vazio
            // Se o painel de MELHORIA estiver aberto, fecha-o.
            if (TowerUpgradeUI.Instance != null && TowerUpgradeUI.Instance.uiPanel.activeInHierarchy)
            {
                TowerUpgradeUI.Instance.ClosePanel();
            }

            // abre o painel de CONSTRUÇÃO
            if (TowerPlacementUI.Instance != null)
            {
                TowerPlacementUI.Instance.OpenPanel(this);
            }
        }
    }
}