using System;
using System.Collections;
using UnityEngine;
using static GameEnums;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    public WaveData waveData;                       // WaveData Scriptable Object 참조
    public Transform spawnPoint;                    // 몬스터 스폰 포인트 위치

    public EnemyData bossEnemy;                     // 보스 적 데이터
    public float bossEnemyMultiplier = 1f;          // 보스 스펙 증가 배율

    public EnemyData missionEnemy;                  // 미션 적 데이터
    public float missionEnemyMultiplier = 1f;       // 미션 적 스펙 증가 배율
    public float missionCooldown = 60f;             // 미션 재사용 대기시간 (쿨타임)
    public ArmorType randomArmorType;               // 미션 및 보스용 무작위 방어 타입

    private bool isMissionCooldown = false;         // 미션이 쿨타임 상태인지 여부
    private float missionCooldownTimer = 0f;        // 미션 쿨타임 남은 시간 타이머

    public int currentWave = 0;                     // 현재 진행 중인 웨이브 인덱스
    [SerializeField]
    private float waveWaitingTime = 10f;            // 웨이브 클리어 후 다음 웨이브까지의 정비 시간
    public float timeUntilNextWave = 0f;            // 다음 웨이브 시작까지 남은 시간

    public int aliveEnemyCount = 0;                 // 현재 필드에 살아있는 적의 총 숫자

    UIManager uiManager;
    GameManager gameManager;
    SoundManager soundManager;

    // 현재 웨이브의 EnemyData를 안전하게 가져오는 프로퍼티
    public EnemyData CurrentWaveEnemyData
    {
        get
        {
            if (currentWave < waveData.waves.Length)
            {
                return waveData.waves[currentWave].enemyData;
            }
            return null;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        uiManager = UIManager.Instance;
        gameManager = GameManager.Instance;
        soundManager = SoundManager.Instance;

        // 웨이브 루프 코루틴 시작
        StartCoroutine(SpawnWavesCoroutine());
    }

    void Update()
    {
        // 미션 쿨타임 실시간 업데이트
        if (isMissionCooldown)
        {
            missionCooldownTimer -= Time.deltaTime;

            if (uiManager != null)
            {
                float cooldownRatio = missionCooldownTimer / missionCooldown;
                uiManager.UpdateMissionCooldown(cooldownRatio);
            }

            if (missionCooldownTimer <= 0f)
            {
                isMissionCooldown = false;
                missionCooldownTimer = 0f;

                // 쿨타임이 종료되었으므로 UI 미션 버튼 상호작용 활성화
                if (uiManager != null)
                {
                    uiManager.SetMissionButtonInteractable(true);
                }
            }
        }

        // 다음 웨이브 카운트다운 타이머 갱신
        if (timeUntilNextWave > 0f)
        {
            timeUntilNextWave -= Time.deltaTime;
        }
    }

    // 전체 웨이브 루프 및 흐름 제어 코루틴
    private IEnumerator SpawnWavesCoroutine()
    {
        timeUntilNextWave = waveWaitingTime;
        yield return new WaitForSeconds(10f); // 첫 웨이브 시작 전 대기

        timeUntilNextWave = 0f;

        // 준비된 모든 웨이브 데이터를 순회할 때까지 반복
        while (currentWave < waveData.waves.Length)
        {
            // 현재 웨이브 데이터 추출
            var wave = waveData.waves[currentWave];

            // 10웨이브 단위(10, 20, 30...)마다 보스 몬스터 소환 조건 체크
            if ((currentWave + 1) % 10 == 0 && bossEnemy != null)
            {
                // 보스 스폰 및 전용 UI 활성화
                SpawnBoss();
            }
            else
            {
                // 일반 웨이브일 경우 보스 패널 숨기기
                if (uiManager != null)
                {
                    uiManager.HideBossInfoPanel();
                }
            }

            // 해당 웨이브에 지정된 적 마리수만큼 순차 스폰
            for (int i = 0; i < wave.enemyCount; i++)
            {
                // 적 오브젝트 풀링 또는 생성
                SpawnEnemy(wave.enemyData);

                // 스폰 간격만큼 대기 후 다음 몬스터 소환
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            // 필드의 모든 적이 처치될 때까지 다음 웨이브 진행을 일시 대기
            while (aliveEnemyCount > 0)
            {
                timeUntilNextWave = 0;
                yield return null;
            }

            // 웨이브 지수 증가
            currentWave++;

            // 다음 웨이브로 넘어가기 전 정비(대기) 시간 적용
            timeUntilNextWave = waveWaitingTime;
            yield return new WaitForSeconds(waveWaitingTime);
        }

        // 모든 웨이브 스폰이 끝난 후, 맵에 남아있는 마지막 잔당들이 모두 죽을 때까지 대기
        while (aliveEnemyCount > 0)
        {
            yield return null;
        }

        // 최종 승리 판정 전 약간의 딜레이 확보
        yield return new WaitForSecondsRealtime(1f);

        CheckVictory();
    }

    // 최종 승리 조건 검증
    private void CheckVictory()
    {
        if (gameManager.playerHp > 0)
        {
            uiManager.EndGame(true);
        }
    }

    // 일반 웨이브 적 생성 및 초기화
    private void SpawnEnemy(EnemyData data)
    {
        if (data == null) return;

        GameObject enemyGo = Instantiate(data.enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyGo.GetComponent<Enemy>();

        enemy.InitEnemy(data);

        aliveEnemyCount++;
    }

    // 10단계 보스 적 생성 및 스펙 조정
    private void SpawnBoss()
    {
        if (bossEnemy == null) return;

        if (uiManager != null)
        {
            uiManager.ShowBossPanel(bossEnemy);
        }

        GameObject enemyGo = Instantiate(bossEnemy.enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyGo.GetComponent<Enemy>();

        // 배율(Multiplier)을 적용하여 보스 능력치 셋팅
        enemy.InitEnemy(bossEnemy, bossEnemyMultiplier);
        enemy.armorType = randomArmorType;

        // 보스가 소환될 때마다 미션 적의 능력치를 크게 늘려 난이도 상승 조절
        missionEnemyMultiplier *= 3f;
        aliveEnemyCount++;
    }

    // UI 미션 버튼 클릭 시 연동되는 미션 몬스터 소환 함수
    public void SpawnMissionEnemy()
    {
        if (isMissionCooldown) return;
        if (missionEnemy == null) return;

        isMissionCooldown = true;
        missionCooldownTimer = missionCooldown;

        if (uiManager != null)
        {
            uiManager.SetMissionButtonInteractable(!isMissionCooldown);
        }

        // 미션 소환 효과음 재생
        soundManager.Play("MissionClick");

        GameObject enemyGo = Instantiate(missionEnemy.enemyPrefab, spawnPoint.position, Quaternion.identity);
        Enemy enemy = enemyGo.GetComponent<Enemy>();

        // 축적된 미션 배율을 대입하여 강화된 미션 몹 생성
        enemy.InitEnemy(missionEnemy, missionEnemyMultiplier);
        enemy.armorType = randomArmorType;

        // 다음 소환될 미션 몬스터의 스펙 누적 강화
        missionEnemyMultiplier *= 1.2f;
        aliveEnemyCount++;
    }

    // 미션 및 보스 유닛에게 부여할 방어 타입을 무작위로 추첨하는 함수
    public void SetRandomArmorType()
    {
        Array armorTypeValues = Enum.GetValues(typeof(ArmorType));
        int randomIndex = UnityEngine.Random.Range(0, armorTypeValues.Length);
        randomArmorType = (ArmorType)armorTypeValues.GetValue(randomIndex);
    }
}