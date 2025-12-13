using UnityEngine;
using Unity.Netcode;
using System;
using Unity.Netcode.Components;

// NetworkTransform lida com a sincronização de posição/rotação
[RequireComponent(typeof(NetworkTransform))]
public class EnemyMP : NetworkBehaviour
{
    [Header("Stats Base (Nível 1)")]
    public float baseSpeed = 3f; // RENOMEADO
    public float rotateSpeed = 10f;

    [Header("Multiplicadores Nível 2")]
    public float speedMultiplierLvl2 = 1.2f; // Bónus de 20%

    // --- Variáveis internas ---
    private float speed; // Esta é a velocidade final, calculada no Setup
    private ulong donoDaTropa;
    private Transform[] meuCaminho;
    private BaseHealthMP baseAlvo;

    private int waypointIndex = 0;
    private float startY;
    private Transform target;

    // --- MODIFICADO: Aceita o Nível ---
    // Chamado pelo Server no GameServerLogic.cs
    public void Setup(ulong dono, Transform[] caminho, BaseHealthMP alvo, int nivel)
    {
        // Esta função SÓ corre no Servidor
        if (!IsServer) return;

        donoDaTropa = dono;
        meuCaminho = caminho;
        baseAlvo = alvo;

        // --- NOVO: Aplica Stats de Nível (Velocidade) ---
        if (nivel >= 2)
        {
            speed = baseSpeed * speedMultiplierLvl2;
        }
        else
        {
            speed = baseSpeed;
        }

        // --- NOVO: Chama o setup da vida ---
        // Passa o nível para o script de vida
        GetComponent<EnemyHealthMP>().SetNivelServer(nivel);


        // --- Resto da lógica de setup (igual à tua) ---
        if (meuCaminho == null || meuCaminho.Length == 0)
        {
            Debug.LogError("Tropa spawnada sem caminho!");
            return;
        }

        target = meuCaminho[0];
        startY = transform.position.y;
    }

    void Update()
    {
        // --- SÓ O SERVER MOVIMENTA OS INIMIGOS ---
        if (!IsServer)
        {
            // Os clientes recebem a posição via NetworkTransform
            return;
        }

        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
        }

        // --- MODIFICADO: Usa a variável 'speed' calculada ---
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);

        if (HasReachedTarget())
        {
            GetNextWaypoint();
        }
    }

    // Função auxiliar (só corre no server)
    bool HasReachedTarget()
    {
        Vector3 posXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetXZ = new Vector3(target.position.x, 0, target.position.z);
        return Vector3.Distance(posXZ, targetXZ) < 0.2f;
    }

    // Função auxiliar (só corre no server)
    void GetNextWaypoint()
    {
        waypointIndex++;

        if (waypointIndex >= meuCaminho.Length)
        {
            // Chegou à base
            if (baseAlvo != null)
                baseAlvo.TakeDamage(1);

            // Despawna o inimigo da rede
            if (NetworkObject != null && NetworkObject.IsSpawned)
            {
                NetworkObject.Despawn();
            }
            else
            {
                Destroy(gameObject); // Fallback
            }
            return;
        }

        target = meuCaminho[waypointIndex];
    }
}