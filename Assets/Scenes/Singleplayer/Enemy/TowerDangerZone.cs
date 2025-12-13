using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(Tower))]
public class TowerDangerZone : MonoBehaviour
{
    private NavMeshModifierVolume dangerVolume;
    private Tower towerScript;
    private NavMeshSurface surface;

    private string[] dangerAreaNames = { "DangerLevel1", "DangerLevel2", "DangerLevel3" };

    void Start()
    {
        towerScript = GetComponent<Tower>();
        surface = FindFirstObjectByType<NavMeshSurface>();

        // Cria o volume de perigo
        GameObject volumeGO = new GameObject("DangerZoneVolume");
        volumeGO.transform.SetParent(transform);
        volumeGO.transform.localPosition = Vector3.zero;
        volumeGO.layer = LayerMask.NameToLayer("Ignore Raycast");

        dangerVolume = volumeGO.AddComponent<NavMeshModifierVolume>();
        dangerVolume.center = Vector3.zero;
        dangerVolume.size = Vector3.one * towerScript.range * 2f;

        UpdateDangerZone(towerScript.level, towerScript.range);
    }

    public void OnTowerUpgrade(int newLevel, float newRange)
    {
        UpdateDangerZone(newLevel, newRange);
    }

    private void UpdateDangerZone(int newLevel, float newRange)
    {
        int levelIndex = Mathf.Clamp(newLevel - 1, 0, dangerAreaNames.Length - 1);
        int areaIndex = NavMesh.GetAreaFromName(dangerAreaNames[levelIndex]);

        if (areaIndex == -1)
        {
            Debug.LogWarning($"Área '{dangerAreaNames[levelIndex]}' não encontrada! Verifica em Navigation > Areas.");
            return;
        }

        if (dangerVolume != null)
        {
            dangerVolume.area = areaIndex;
            dangerVolume.size = new Vector3(newRange * 2f, 5f, newRange * 2f);
            dangerVolume.center = Vector3.zero;
        }

        StartCoroutine(RebakeNavMesh());
    }

    private IEnumerator RebakeNavMesh()
    {
        yield return null; // espera 1 frame para evitar rebake precoce
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }

    public void DestroyZone()
    {
        if (dangerVolume != null)
        {
            Destroy(dangerVolume.gameObject);
        }

        StartCoroutine(RebakeNavMesh());
    }
}
