using UnityEngine;
using UnityEngine.AI;

public class Tower : MonoBehaviour
{
    [Header("Stats")]
    public float range = 5f;
    public float fireRate = 1f;
    public float rotationSpeed = 10f;

    [Header("Prefabs")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Parts")]
    public Transform partToRotate;

    [Header("Tower Models (visuals)")]
    public GameObject level1Model;
    public GameObject level2Model;
    public GameObject level3Model;

    private GameObject currentModelInstance;

    [Header("Upgrade Stats")]
    public string towerName = "Archer Tower";
    public int level = 1;

    [Tooltip("Custo para comprar esta torre (Nível 1)")]
    public int costLevel1 = 100;
    public int upgradeCostLevel2 = 75;
    public int upgradeCostLevel3 = 150;

    [HideInInspector] public int totalInvested;
    [HideInInspector] public TowerSpot myTowerSpot;

    protected float baseRange;
    protected int baseBulletDamage;
    protected float baseBulletSpeed;

    protected Transform target;
    protected float fireCountdown = 0f;

    protected virtual void Start()
    {
        baseRange = range;
        StoreBaseBulletStats();

        if (totalInvested == 0)
            totalInvested = costLevel1;

        // Instancia o modelo inicial (nível 1)
        SpawnModelForLevel(level);
    }

    protected virtual void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            Bullet b = bulletPrefab.GetComponent<Bullet>();
            if (b != null)
            {
                baseBulletDamage = b.damage;
                baseBulletSpeed = b.speed;
            }
        }
    }

    protected virtual void Update()
    {
        UpdateTarget();

        if (target != null)
            RotateToTarget();

        if (target == null) return;

        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / fireRate;
        }

        fireCountdown -= Time.deltaTime;
    }

    protected virtual void UpdateTarget()
    {
        EnemyHealth[] allEnemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        EnemyHealth nearest = null;

        foreach (EnemyHealth e in allEnemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortestDistance && d <= range)
            {
                shortestDistance = d;
                nearest = e;
            }
        }

        target = nearest != null ? nearest.transform : null;
    }

    protected virtual void RotateToTarget()
    {
        if (partToRotate == null || target == null) return;
        Vector3 dir = target.position - partToRotate.position;
        dir.y = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        lookRotation *= Quaternion.Euler(0f, 180f, 0f);
        partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    protected virtual void Shoot()
    {
        if (bulletPrefab == null || firePoint == null || target == null) return;

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
        {
            if (level == 3)
            {
                bullet.damage = (int)(baseBulletDamage * 1.5f);
                bullet.speed = baseBulletSpeed * 1.5f;
            }

            bullet.Seek(target);
        }
    }

    public virtual void UpgradeTower()
    {
        if (level == 1)
        {
            if (CurrencySystem.SpendMoney(upgradeCostLevel2))
            {
                totalInvested += upgradeCostLevel2;
                level = 2;

                range = baseRange * 1.5f;

                Debug.Log("Torre melhorada para Nível 2!");
                SpawnModelForLevel(level);

                TowerDangerZone dangerZone = GetComponent<TowerDangerZone>();
                if (dangerZone != null)
                    dangerZone.OnTowerUpgrade(level, range);
            }
        }
        else if (level == 2)
        {
            if (CurrencySystem.SpendMoney(upgradeCostLevel3))
            {
                totalInvested += upgradeCostLevel3;
                level = 3;

                Debug.Log("Torre melhorada para Nível 3!");
                SpawnModelForLevel(level);

                TowerDangerZone dangerZone = GetComponent<TowerDangerZone>();
                if (dangerZone != null)
                    dangerZone.OnTowerUpgrade(level, range);
            }
        }
    }

    private void SpawnModelForLevel(int lvl)
    {
        // Destroi modelo anterior se existir
        if (currentModelInstance != null)
            Destroy(currentModelInstance);

        GameObject modelToSpawn = null;
        if (lvl == 1) modelToSpawn = level1Model;
        else if (lvl == 2) modelToSpawn = level2Model;
        else if (lvl == 3) modelToSpawn = level3Model;

        if (modelToSpawn == null)
        {
            Debug.LogWarning("Modelo para o nível " + lvl + " não atribuído!");
            return;
        }

        // Instancia modelo e define como filho da torre
        currentModelInstance = Instantiate(modelToSpawn, transform);
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = Quaternion.identity;

        // Atualiza referências (partToRotate, firePoint)
        partToRotate = currentModelInstance.transform.Find("PartToRotate");
        firePoint = currentModelInstance.transform.Find("FirePoint");

        if (partToRotate == null || firePoint == null)
        {
            Debug.LogWarning($"Modelo de torre (nível {lvl}) não contém PartToRotate ou FirePoint!");
        }
    }

    public virtual void SellTower()
    {
        int sellAmount = totalInvested / 2;
        CurrencySystem.AddMoney(sellAmount);

        if (myTowerSpot != null)
        {
            myTowerSpot.isOccupied = false;
            myTowerSpot.currentTower = null;
        }

        TowerDangerZone dangerZone = GetComponent<TowerDangerZone>();
        if (dangerZone != null)
            dangerZone.DestroyZone();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
