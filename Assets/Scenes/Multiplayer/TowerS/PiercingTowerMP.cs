using UnityEngine;
using Unity.Netcode;

public class PiercingTowerMP : TowerMP
{
    protected override void Start()
    {
        base.Start();
        if (IsOwner) // Ou sem a verificação, se o UI for buscar o nome diretamente
        {
            towerName = "Piercing Tower";
        }
    }

    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            PiercingBulletMP pb = bulletPrefab.GetComponent<PiercingBulletMP>();
            if (pb != null)
            {
                baseBulletDamage = pb.damage;
                baseBulletSpeed = pb.speed;
            }
            else
            {
                Debug.LogError($"O prefab '{bulletPrefab.name}' atribuído a {gameObject.name} não tem o script PiercingBulletMP!", this);
            }
        }
        else
        {
            Debug.LogError($"A torre {gameObject.name} não tem prefab de bala atribuído!", this);
        }
    }

    protected override void Shoot()
    {
        // Só corre no Server
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // 1. Instancia o prefab da PiercingBulletMP
        // Usa a rotação da torre (firePoint) para a direção inicial
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Obtém e Spawna o NetworkObject
        NetworkObject bulletNO = bulletGO.GetComponent<NetworkObject>();
        if (bulletNO == null)
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' não tem NetworkObject!", this);
            Destroy(bulletGO);
            return;
        }
        bulletNO.Spawn();

        // 3. Obtém o script PiercingBulletMP
        PiercingBulletMP bullet = bulletGO.GetComponent<PiercingBulletMP>();
        if (bullet != null)
        {
            // 4. Define o dono e stats
            bullet.ownerClientId = this.donoDaTorreClientId;

            int currentDamage = baseBulletDamage;
            float currentSpeed = baseBulletSpeed;

            if (level.Value == 3)
            {
                currentDamage = (int)(baseBulletDamage * 1.5f);
                currentSpeed = baseBulletSpeed * 1.5f;
            }

            // Define stats na instância da bala
            bullet.damage = currentDamage;
            bullet.speed = currentSpeed;

            // 5. Define a direção inicial (IMPORTANTE para bala perfurante)
            // A bala vai seguir em frente a partir do firePoint na direção do alvo inicial
            if (target != null)
            {
                Vector3 direction = (target.position - firePoint.position).normalized;
                bullet.SetDirection(direction); // Usa um método para definir a direção
            }
            else
            {
                // Fallback: Dispara na direção para onde a torre está a apontar
                bullet.SetDirection(firePoint.forward);
            }
        }
        else
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' não tem o script PiercingBulletMP!", this);
            bulletNO.Despawn();
        }
    }
}