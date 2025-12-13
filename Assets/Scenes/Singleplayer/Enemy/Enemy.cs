using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;

    [Header("AI Pathfinding")]
    public float pathUpdateRate = 1.0f;

    private Transform baseTarget;
    private NavMeshAgent agent;
    private static BaseHealth baseHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado!", this);
            return;
        }

        // Procura a base
        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseTarget = baseObject.transform;
        }
        else
        {
            Debug.LogError("Base não encontrada! Verifica a Tag 'Base'.", this);
            return;
        }

        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();

        agent.speed = speed;

        // IMPORTANTE: Stopping Distance a 0 para ele tentar ir até ao centro mesmo
        agent.stoppingDistance = 0f;

        ApplyAreaCosts();
        StartCoroutine(UpdatePath());
    }


    private void ApplyAreaCosts()
    {
        SetCost("Walkable", 1f);
        SetCost("DangerLevel1", 5f);
        SetCost("DangerLevel2", 15f);
        SetCost("DangerLevel3", 40f);
    }

    private void SetCost(string areaName, float cost)
    {
        int index = NavMesh.GetAreaFromName(areaName);
        if (index != -1) agent.SetAreaCost(index, cost);
    }

    IEnumerator UpdatePath()
    {
        while (true)
        {
            if (baseTarget != null && agent.isOnNavMesh && agent.enabled)
            {
                agent.SetDestination(baseTarget.position);
            }
            yield return new WaitForSeconds(pathUpdateRate);
        }
    }

    // NOVA LÓGICA: Detecta colisão física com a Base
    void OnTriggerEnter(Collider other)
    {
        // Se entrarmos num objeto com a Tag "Base"
        if (other.CompareTag("Base"))
        {
            ReachBase();
        }
    }

    void ReachBase()
    {
        // Impede que o código corra duas vezes se houver colliders duplos
        if (!this.enabled) return;

        StopAllCoroutines();

        if (baseHealth != null)
            baseHealth.TakeDamage(1);

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        Destroy(gameObject);
    }
}