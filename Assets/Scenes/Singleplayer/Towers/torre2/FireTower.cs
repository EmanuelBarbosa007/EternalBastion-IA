using UnityEngine;

public class FireTower : Tower
{

    protected override void Start()
    {
        base.Start(); // Chama o Start() da Tower
        towerName = "Fire Tower";
    }

    //guarda os stats da Fireball
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            Fireball fb = bulletPrefab.GetComponent<Fireball>();
            if (fb != null)
            {
                baseBulletDamage = fb.damage;
                baseBulletSpeed = fb.speed;
            }
        }
    }

    // aplica stats à Fireball
    protected override void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject go = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Fireball fb = go.GetComponent<Fireball>();
        if (fb != null)
        {
            // Aplica melhorias de Nível 3
            if (level == 3)
            {
                fb.damage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                fb.speed = baseBulletSpeed * 1.5f;       // +50% Velocidade
            }

            fb.Seek(target);
        }
    }
}