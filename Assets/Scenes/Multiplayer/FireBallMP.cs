using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic; // Para a lista de inimigos atingidos

// Requer NetworkObject para existir na rede e NetworkTransform para sincronizar posição
[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class FireballMP : NetworkBehaviour
{
    private Transform target;
    public float speed = 10f;
    public float explosionRadius = 3f;
    public int damage = 50; // Dano base, será modificado pela torre Nível 3

    public GameObject impactEffectPrefab; // Prefab do efeito visual de explosão

    [HideInInspector]
    public ulong ownerClientId; // ID do jogador que disparou

    // Lista para garantir que cada inimigo só leva dano uma vez pela explosão
    private List<EnemyHealthMP> hitEnemies = new List<EnemyHealthMP>();

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        // --- SÓ O SERVER MOVIMENTA E VERIFICA COLISÃO ---
        if (!IsServer)
        {
            // Os clientes recebem a posição pelo NetworkTransform
            return;
        }

        // Se o alvo morreu ou desapareceu enquanto a bola voava
        if (target == null)
        {
            NetworkObject.Despawn(); // Destrói na rede
            return;
        }

        // Lógica de movimento (igual à original)
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Verifica se alcançou ou ultrapassou o alvo neste frame
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget(); // Chama a função de impacto (só no server)
            return; // Sai do Update após atingir o alvo
        }

        // Move a bola de fogo
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        // Opcional: fazer a bola olhar para o alvo (NetworkTransform pode sincronizar rotação)
        // transform.LookAt(target);
    }

    // --- Chamada APENAS NO SERVER quando atinge o alvo ---
    void HitTarget()
    {
        // 1. Aplica dano em área
        Explode();

        // 2. Manda os clientes spawnarem o efeito visual
        SpawnImpactEffectClientRpc(transform.position);

        // 3. Destrói a bola de fogo na rede
        NetworkObject.Despawn();
    }

    // --- Aplica dano em área (SÓ NO SERVER) ---
    void Explode()
    {
        hitEnemies.Clear(); // Limpa a lista para esta explosão

        // Encontra todos os colliders dentro do raio de explosão
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider col in colliders)
        {
            // Tenta encontrar o componente de vida do inimigo (versão MP)
            EnemyHealthMP enemyHealth = col.GetComponent<EnemyHealthMP>();

            // Se encontrou um inimigo E ainda não o atingimos nesta explosão
            if (enemyHealth != null && !hitEnemies.Contains(enemyHealth))
            {
                // Aplica dano, passando o ID do dono da torre/bala
                enemyHealth.TakeDamage(damage, ownerClientId);
                hitEnemies.Add(enemyHealth); // Adiciona à lista para não atingir de novo
            }
        }
    }

    // --- ClientRpc para mostrar o efeito visual ---
    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position)
    {
        // Este código corre em TODOS os clientes (e no host/server)
        if (impactEffectPrefab != null)
        {
            GameObject effect = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Destrói o efeito visual após 2 segundos (localmente)
        }
    }

    // (Opcional) Desenhar o raio no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}