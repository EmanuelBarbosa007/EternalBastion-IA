using System.Collections;
using UnityEngine;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTDOWN }

    [Header("Prefabs")]
    public GameObject baseEnemyPrefab; // Para Normal e Tanque
    public GameObject horsePrefab;     // Prefab do Cavalo 
    public GameObject bossPrefab;      // Prefab do Boss

    [Header("Assets para Decorators")]
    public Material tankMaterial;

    [Header("Referências UI e Spawn")]
    public Transform[] spawnPoints;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI countdownText;

    [Header("Stats das Waves")]
    public float timeBetweenWaves = 20f;
    public int enemiesPerWave = 5;
    public float spawnInterval = 1f;

    [Header("Dificuldade")]
    public int enemiesPerWaveIncrease = 2;
    public int waveToStartTanks = 3;
    public int waveToStartHorses = 5;
    public float spawnIntervalDecrease = 0.05f;
    public float minSpawnInterval = 0.2f;
    public float timeBetweenWavesDecrease = 0.5f;
    public float minTimeBetweenWaves = 5f;

    [Header("Boss")]
    public int bossWaveFrequency = 5;
    public int bossCount = 1;

    private int waveNumber = 1;
    private float countdown;
    private SpawnState state = SpawnState.COUNTDOWN;
    public static int EnemiesAlive = 0;

    private enum EnemyType { Normal, Tank, Horse }

    private void Start()
    {
        EnemiesAlive = 0;
        countdown = timeBetweenWaves;
        state = SpawnState.COUNTDOWN;
        UpdateWaveUI();
    }

    private void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (EnemiesAlive <= 0) StartWaveCountdown();
            return;
        }

        if (state == SpawnState.COUNTDOWN)
        {
            countdown -= Time.deltaTime;
            countdown = Mathf.Max(0, countdown);
            if (countdownText) countdownText.text = $"Próxima Wave: {Mathf.CeilToInt(countdown)}s";

            if (countdown <= 0f)
            {
                state = SpawnState.SPAWNING;
                if (countdownText) countdownText.text = "ATAQUE!";
                StartCoroutine(SpawnWave());
            }
        }
    }

    void StartWaveCountdown()
    {
        state = SpawnState.COUNTDOWN;
        enemiesPerWave += enemiesPerWaveIncrease;
        spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
        timeBetweenWaves = Mathf.Max(minTimeBetweenWaves, timeBetweenWaves - timeBetweenWavesDecrease);
        countdown = timeBetweenWaves;
        waveNumber++;
        UpdateWaveUI();
    }

    void UpdateWaveUI()
    {
        if (waveText) waveText.text = "Wave " + waveNumber;
    }

    IEnumerator SpawnWave()
    {
        EnemiesAlive = 0;

        // 1. Spawna TANQUES (Usa Decorator)
        if (waveNumber >= waveToStartTanks)
        {
            int tankCount = waveNumber / waveToStartTanks;
            for (int i = 0; i < tankCount; i++)
            {
                SpawnEnemy(EnemyType.Tank);
                yield return new WaitForSeconds(spawnInterval * 2);
            }
        }

        // 2. Spawna INIMIGOS NORMAIS (Usa Decorator)
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy(EnemyType.Normal);
            yield return new WaitForSeconds(spawnInterval);
        }

        // 3. Spawna CAVALOS (Usa Prefab)
        if (waveNumber >= waveToStartHorses)
        {
            int horseCount = waveNumber / waveToStartHorses;
            for (int i = 0; i < horseCount; i++)
            {
                SpawnEnemy(EnemyType.Horse);
                yield return new WaitForSeconds(spawnInterval * 0.5f);
            }
        }

        // 4. Boss Logic
        if (waveNumber % bossWaveFrequency == 0 && bossPrefab != null)
        {
            yield return new WaitForSeconds(spawnInterval * 3);
            for (int i = 0; i < bossCount; i++)
            {
                Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Instantiate(bossPrefab, sp.position, sp.rotation);
                EnemiesAlive++;
                yield return new WaitForSeconds(spawnInterval * 2);
            }
        }

        state = SpawnState.WAITING;
    }

    void SpawnEnemy(EnemyType type)
    {
        if (spawnPoints.Length == 0) return;
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject objectToSpawn = null;

        
        // Se for Cavalo, usamos o prefab específico dele.
        // Se for Normal ou Tanque, usamos o prefab Base e aplicamos Decorator.

        if (type == EnemyType.Horse)
        {
            // CAMINHO DO CAVALO (SEM DECORATOR)
            if (horsePrefab != null)
            {
                Instantiate(horsePrefab, sp.position, sp.rotation);
                EnemiesAlive++;
            }
            return; // Sai da função, não precisa de decorator
        }
        else
        {
            //  CAMINHO DO NORMAL/TANQUE (COM DECORATOR) 
            objectToSpawn = Instantiate(baseEnemyPrefab, sp.position, sp.rotation);
        }

        // Aplica o Decorator apenas para Normal e Tanque
        IEnemyDecorator decorator = null;

        switch (type)
        {
            case EnemyType.Normal:
                decorator = new NormalEnemyDecorator();
                break;
            case EnemyType.Tank:
                decorator = new TankEnemyDecorator(tankMaterial);
                break;
        }

        if (decorator != null && objectToSpawn != null)
        {
            decorator.Decorate(objectToSpawn);
        }

        EnemiesAlive++;
    }
}