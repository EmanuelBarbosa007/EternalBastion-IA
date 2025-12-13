using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TrojanHorseBoss : MonoBehaviour
{
    [Header("Navegação")]
    public float speed = 2f;

    private Transform baseTarget;
    private NavMeshAgent agent;
    private static BaseHealth baseHealth;

    [Header("Atributos do Boss")]
    public int damageToBase = 10;
    public int enemiesToSpawn = 5;
    public GameObject[] enemyPrefabs;

    [Header("Configuração do Spawn")]
    public float timeBetweenSpawns = 0.5f;

    private bool isDead = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent não encontrado no Boss!", this);
            return;
        }

        // --- NOVA ALTERAÇÃO AQUI ---
        // Chamamos esta função para "lobotomizar" a IA deste agente específico,
        // forçando-o a ignorar os perigos que os outros respeitam.
        IgnorarPerigo();
        // ---------------------------

        GameObject baseObject = GameObject.FindGameObjectWithTag("Base");
        if (baseObject != null)
        {
            baseTarget = baseObject.transform;
            agent.speed = speed;
            agent.SetDestination(baseTarget.position);
        }
        else
        {
            Debug.LogError("Erro: Objeto 'Base' não encontrado.", this);
        }

        if (baseHealth == null)
            baseHealth = Object.FindFirstObjectByType<BaseHealth>();
    }

    // --- MÉTODOS NOVOS PARA O "TANK PATHFINDING" ---
    void IgnorarPerigo()
    {
        // Define o custo das áreas perigosas como 1.
        // Custo 1 = Custo de andar em chão normal ("Walkable").
        // Isto faz com que o cálculo do caminho seja puramente pela distância.
        SetCost("DangerLevel1", 1f);
        SetCost("DangerLevel2", 1f);
        SetCost("DangerLevel3", 1f);
    }

    void SetCost(string areaName, float cost)
    {
        int index = NavMesh.GetAreaFromName(areaName);
        if (index != -1)
        {
            agent.SetAreaCost(index, cost);
        }
    }
    // ------------------------------------------------

    void Update()
    {
        if (isDead) return;

        if (agent != null && !agent.pathPending && agent.isActiveAndEnabled)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    ReachBase();
                }
            }
        }
    }

    void ReachBase()
    {
        if (isDead) return;

        if (baseHealth != null)
            baseHealth.TakeDamage(damageToBase);

        if (EnemySpawner.EnemiesAlive > 0)
            EnemySpawner.EnemiesAlive--;

        StartDeathSequence();
    }

    public void StartDeathSequence()
    {
        if (isDead) return;
        isDead = true;

        StartCoroutine(SpawnTroopsRoutine());
    }

    IEnumerator SpawnTroopsRoutine()
    {
        Debug.Log("Cavalo destruído. A libertar tropas com intervalo...");

        if (agent != null) agent.enabled = false;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders) c.enabled = false;

        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0.5f, Random.Range(-1f, 1f));
                Vector3 spawnPos = transform.position + offset;

                Instantiate(enemyPrefabs[randomIndex], spawnPos, transform.rotation);
                EnemySpawner.EnemiesAlive++;

                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        Destroy(gameObject);
    }
}