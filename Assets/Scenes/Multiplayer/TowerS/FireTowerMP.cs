using UnityEngine;
using Unity.Netcode;

// Garante que herda de TowerMP
public class FireTowerMP : TowerMP
{
    // Override Start para definir o nome e ler stats base
    protected override void Start()
    {
        // Chama o Start da classe base (TowerMP) para inicializar NetworkVariables
        base.Start();

        // Define o nome APENAS DEPOIS da inicialização base
        // Não precisa ser NetworkVariable, é só para o UI local
        if (IsOwner) // Ou talvez nem precise verificar IsOwner, depende de como o UI lê
        {
            towerName = "Fire Tower";
        }
    }

    // Override para guardar os stats base da FireballMP
    protected override void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            // Procura o componente FireballMP no prefab
            FireballMP fb = bulletPrefab.GetComponent<FireballMP>();
            if (fb != null)
            {
                baseBulletDamage = fb.damage; // Guarda o dano base
                baseBulletSpeed = fb.speed;   // Guarda a velocidade base
            }
            else
            {
                Debug.LogError($"O prefab '{bulletPrefab.name}' atribuído a {gameObject.name} não tem o script FireballMP!", this);
            }
        }
        else
        {
            Debug.LogError($"A torre {gameObject.name} não tem prefab de bala atribuído!", this);
        }
    }

    // Override Shoot para spawnar FireballMP
    protected override void Shoot()
    {
        // Esta função SÓ corre no Server (verificação já feita no TowerMP.Update)
        if (bulletPrefab == null || firePoint == null || target == null) return;

        // 1. Instancia o prefab da FireballMP
        GameObject fireballGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. Obtém o NetworkObject para spawnar na rede
        NetworkObject fireballNO = fireballGO.GetComponent<NetworkObject>();
        if (fireballNO == null)
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' não tem NetworkObject!", this);
            Destroy(fireballGO); // Destrói o objeto local se não puder ser spawnado
            return;
        }
        fireballNO.Spawn(); // Spawna a bola de fogo para todos os clientes

        // 3. Obtém o script FireballMP
        FireballMP fireball = fireballGO.GetComponent<FireballMP>();
        if (fireball != null)
        {
            // 4. Define o dono e aplica stats de upgrade (se nível 3)
            fireball.ownerClientId = this.donoDaTorreClientId; // Diz à bola de fogo quem a disparou

            int currentDamage = baseBulletDamage;
            float currentSpeed = baseBulletSpeed;

            // Aplica melhorias de Nível 3 (lendo o valor da NetworkVariable 'level')
            if (level.Value == 3)
            {
                currentDamage = (int)(baseBulletDamage * 1.5f); // +50% Dano
                currentSpeed = baseBulletSpeed * 1.5f;        // +50% Velocidade
            }

            fireball.damage = currentDamage;
            fireball.speed = currentSpeed;


            // 5. Define o alvo
            fireball.Seek(target);
        }
        else
        {
            Debug.LogError($"Prefab '{bulletPrefab.name}' não tem o script FireballMP!", this);
            fireballNO.Despawn(); // Despawna o objeto da rede se o script estiver em falta
        }
    }
}