using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(NetworkObject), typeof(NetworkTransform))]
public class BulletMP : NetworkBehaviour
{
    private Transform target;
    public float speed = 10f;
    public int damage = 1;

    [HideInInspector]
    public ulong ownerClientId;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        // --- SÓ O SERVER MOVIMENTA A BALA ---
        if (!IsServer)
        {
            return;
        }

        if (target == null)
        {
            NetworkObject.Despawn();
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }


        // 1. Calcula a rotação normal para olhar para o alvo
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        // 2. Combina com uma rotação de 180 graus no eixo Y para virar a flecha
        // Se continuar torta, tente mudar o 180 para 90 ou -90
        transform.rotation = lookRotation * Quaternion.Euler(0f, 0f, 180f);

        // Move a bala
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        EnemyHealthMP e = target.GetComponent<EnemyHealthMP>();
        if (e != null)
        {
            e.TakeDamage(damage, ownerClientId);
        }

        NetworkObject.Despawn();
    }
}