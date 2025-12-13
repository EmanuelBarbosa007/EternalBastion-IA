using UnityEngine;

public class PiercingTower : Tower
{
    protected override void Start()
    {
        base.Start(); // Chama o Start() da Tower
        towerName = "Piercing Tower";
    }

    // guarda os stats da PiercingBullet
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            PiercingBullet pb = bulletPrefab.GetComponent<PiercingBullet>();
            if (pb != null)
            {
                baseBulletDamage = pb.damage;
                baseBulletSpeed = pb.speed;
            }
        }
    }

    // aplica stats à PiercingBullet
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        PiercingBullet pb = go.GetComponent<PiercingBullet>();
        if (pb != null)
        {
            // Aplica melhorias de Nível 3
            if (level == 3)
            {
                pb.damage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                pb.speed = baseBulletSpeed * 1.5f;       // +50% Velocidade
            }

            pb.Seek(target);
        }
    }
}