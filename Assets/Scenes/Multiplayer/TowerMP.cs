using UnityEngine;
using Unity.Netcode;
using UnityEngine.EventSystems;

public class TowerMP : NetworkBehaviour
{
    [Header("Stats")]
    public NetworkVariable<float> range = new NetworkVariable<float>(5f);
    public NetworkVariable<int> level = new NetworkVariable<int>(1);

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
    public int costLevel1 = 100;
    public int upgradeCostLevel2 = 75;
    public int upgradeCostLevel3 = 150;

    [HideInInspector] public int totalInvested;
    [HideInInspector] public TowerSpotMP myTowerSpot;
    [HideInInspector] public ulong donoDaTorreClientId;

    protected float baseRange;
    protected int baseBulletDamage;
    protected float baseBulletSpeed;

    protected Transform target;
    protected float fireCountdown = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        baseRange = range.Value;
        StoreBaseBulletStats();

        if (totalInvested == 0)
            totalInvested = costLevel1;

        SpawnModelForLevel(level.Value);
        level.OnValueChanged += OnLevelChanged;
    }

    // Mantemos o Start virtual vazio para compatibilidade com as classes filhas
    protected virtual void Start() { }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        level.OnValueChanged -= OnLevelChanged;
    }

    private void OnLevelChanged(int valorAntigo, int valorNovo)
    {
        SpawnModelForLevel(valorNovo);
    }

    protected virtual void StoreBaseBulletStats()
    {
        if (bulletPrefab != null)
        {
            BulletMP b = bulletPrefab.GetComponent<BulletMP>();
            if (b != null)
            {
                baseBulletDamage = b.damage;
                baseBulletSpeed = b.speed;
            }
        }
    }

    protected virtual void Update()
    {
        // Visual: Rotação para todos
        UpdateTarget();
        if (target != null)
            RotateToTarget();

        // Lógica: Só no servidor
        if (!IsServer) return;
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
        EnemyMP[] enemies = Object.FindObjectsByType<EnemyMP>(FindObjectsSortMode.None);
        float shortestDistance = Mathf.Infinity;
        EnemyMP nearest = null;

        foreach (EnemyMP e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < shortestDistance && d <= range.Value)
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
        bulletGO.GetComponent<NetworkObject>().Spawn();

        BulletMP bullet = bulletGO.GetComponent<BulletMP>();
        if (bullet != null)
        {
            bullet.ownerClientId = donoDaTorreClientId;
            if (level.Value == 3)
            {
                bullet.damage = (int)(baseBulletDamage * 1.5f);
                bullet.speed = baseBulletSpeed * 1.5f;
            }
            bullet.Seek(target);
        }
    }

    public void TryUpgrade(ulong playerClientId)
    {
        if (!IsServer) return;
        if (playerClientId != donoDaTorreClientId) return;

        if (level.Value == 1)
        {
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel2))
            {
                totalInvested += upgradeCostLevel2;
                level.Value = 2;
                range.Value = baseRange * 1.5f;
            }
        }
        else if (level.Value == 2)
        {
            if (CurrencySystemMP.Instance.SpendMoney(playerClientId, upgradeCostLevel3))
            {
                totalInvested += upgradeCostLevel3;
                level.Value = 3;
            }
        }
    }

    private void SpawnModelForLevel(int lvl)
    {
        if (currentModelInstance != null) Destroy(currentModelInstance);

        GameObject modelToSpawn = null;
        if (lvl == 1) modelToSpawn = level1Model;
        else if (lvl == 2) modelToSpawn = level2Model;
        else if (lvl == 3) modelToSpawn = level3Model;

        if (modelToSpawn == null) return;

        currentModelInstance = Instantiate(modelToSpawn, transform);
        currentModelInstance.transform.localPosition = Vector3.zero;
        currentModelInstance.transform.localRotation = Quaternion.identity;

        partToRotate = currentModelInstance.transform.Find("PartToRotate");
        firePoint = currentModelInstance.transform.Find("FirePoint");
    }

    // --- AQUI ESTÁ A CORREÇÃO DO CLIQUE ---
    private void OnMouseDown()
    {
        // 1. Verifica se está a clicar na UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (PlayerNetwork.LocalInstance == null) return;

        // 2. Descobre quem somos nós
        PlayerID localPlayerId = (PlayerNetwork.LocalInstance.OwnerClientId == 0)
                                 ? PlayerID.JogadorA
                                 : PlayerID.JogadorB;

        // 3. >>> CORREÇÃO: Recuperação do Spot no Cliente <<<
        // Se a torre não souber onde está (acontece no Cliente), ela procura o spot mais próximo
        if (myTowerSpot == null)
        {
            TowerSpotMP[] allSpots = FindObjectsByType<TowerSpotMP>(FindObjectsSortMode.None);
            float closestDist = Mathf.Infinity;
            TowerSpotMP bestSpot = null;

            foreach (var spot in allSpots)
            {
                // Medimos a distância ignorando a altura (Y), pois a torre pode ser alta
                float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                                              new Vector2(spot.transform.position.x, spot.transform.position.z));

                // Se estiver muito perto (menos de 1 metro), assumimos que é este o spot
                if (dist < 1.0f && dist < closestDist)
                {
                    closestDist = dist;
                    bestSpot = spot;
                }
            }
            myTowerSpot = bestSpot;
        }

        // 4. Se mesmo assim não encontrar, desiste (evita erros)
        if (myTowerSpot == null)
        {
            Debug.LogWarning("TowerMP: Clique ignorado porque a torre não encontrou o seu Spot.");
            return;
        }

        // 5. Verifica se o spot pertence ao jogador que clicou
        if (myTowerSpot.donoDoSpot != localPlayerId)
        {
            Debug.Log("Esta torre não te pertence!");
            return;
        }

        // 6. Abre o painel
        if (TowerUpgradeUIMP.Instance != null)
        {
            if (TowerPlacementUIMP.Instance != null)
                TowerPlacementUIMP.Instance.ClosePanel();

            TowerUpgradeUIMP.Instance.OpenPanel(this, myTowerSpot);
        }
    }
}