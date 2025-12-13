using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform), typeof(Rigidbody))]
public class PiercingBulletMP : NetworkBehaviour
{
    private Vector3 moveDirection;

    public float speed = 20f;
    public int damage = 3; // Dano base
    public float lifetime = 10f; // Tempo de vida da bala

    private float lifeTimer = 0f;

    [HideInInspector]
    public ulong ownerClientId;

    private List<EnemyHealthMP> enemiesHit = new List<EnemyHealthMP>();

    public void SetDirection(Vector3 direction)
    {
        // Ignora a componente Y para manter a trajetória plana
        direction.y = 0f;

        moveDirection = direction.normalized;

        if (moveDirection != Vector3.zero)
        {
            // 1. Calcula a rotação para olhar na direção do movimento
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);

            // 2. Aplica a rotação extra de 180 graus no eixo Y para virar o modelo
            // Se continuar torta, experimente 90 ou -90 aqui
            transform.rotation = lookRotation * Quaternion.Euler(0f, 0f, 180f);
        }
    }

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
            else
            {
                Debug.LogError("PiercingBulletMP precisa de um Collider para detectar colisões (OnTriggerEnter)!", this);
            }
        }
        else
        {
            Debug.LogError("PiercingBulletMP precisa de um Rigidbody!", this);
        }
    }

    void Update()
    {
        // Apenas o servidor move a bala
        if (!IsServer) return;

        // Move a bala em Espaço Global (World) baseado na direção calculada
        // Isto garante que a rotação visual do modelo não afeta a direção do movimento
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);

        // Controlo de tempo de vida
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifetime)
        {
            NetworkObject.Despawn(); // Destrói na rede
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Apenas o servidor deteta colisões
        if (!IsServer) return;

        EnemyHealthMP enemyHealth = other.GetComponent<EnemyHealthMP>();

        // Garante que só dá dano uma vez por inimigo (piercing)
        if (enemyHealth != null && !enemiesHit.Contains(enemyHealth))
        {
            enemyHealth.TakeDamage(damage, ownerClientId);
            enemiesHit.Add(enemyHealth);
        }
    }
}