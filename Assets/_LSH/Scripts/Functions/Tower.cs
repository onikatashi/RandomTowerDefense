using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("해당 TowerData")]
    public TowerData towerData;                             // 타워 데이터

    public float CurrentAttack { get; set; }                // 현재 타워 공격력
    public float CurrentAttackSpeed { get; set; }           // 현재 타워 공격 속도
    public float CurrentAttackRange { get; set; }           // 현재 타워 사거리
    public float CurrentAttackSplashRange { get; set; }     // 현재 스플래시 범위
    public int CurrentAttackTargetCount { get; set; }       // 현재 공격 타겟 수

    [Header("투사체 발사 설정")]
    public Transform firePoint;                             // 투사체가 발사될 위치
    public GameObject projectilePrefab;                     // 투사체 프리팹

    private float attackCooldown = 0f;                      // 공격 쿨타임

    [Header("타워 애니메이션")]
    [SerializeField]
    private Animator animator;                              // 타워 애니메이터

    private readonly List<Collider2D> colliderBuffer = new List<Collider2D>();
    private readonly List<Enemy> enemyBuffer = new List<Enemy>();

    private ContactFilter2D contactFilter;

    UpgradeManager upgradeManager;
    SoundManager soundManager;
    void Start()
    {
        upgradeManager = UpgradeManager.Instance;
        soundManager = SoundManager.Instance;

        contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = false;  // 트리거 콜라이더 제외
        contactFilter.SetLayerMask(LayerMask.GetMask("Enemy")); // Enemy 레이어만 검출

        if (upgradeManager != null)
        {
            var state = upgradeManager.GetUpgradeState(towerData.towerType);
            if (state != null)
            {
                ApplyUpgrade(state);
            }
        }

        // UpgradeManager가 없을 경우, 기존 기본 데이터로 초기화
        else
        {
            CurrentAttack = towerData.attack;
            CurrentAttackSpeed = towerData.attackSpeed;
            CurrentAttackRange = towerData.attackRange;
            CurrentAttackSplashRange = towerData.attackSplashRange;
            CurrentAttackTargetCount = towerData.attackTargetCount;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // 애니메이션 속도 설정
        SetAnimationSpeed(CurrentAttackSpeed);
    }

    void Update()
    {
        attackCooldown -= Time.deltaTime;

        if (attackCooldown <= 0f)
        {
            Shoot();
            attackCooldown = 1f / CurrentAttackSpeed;
        }
    }

    // 업그레이드 적용 함수
    public void ApplyUpgrade(UpgradeManager.UpgradeState state)
    {
        int upgradeCount = state.level;

        CurrentAttack = towerData.attack + (towerData.attackBonus * upgradeCount);
        CurrentAttackSpeed = towerData.attackSpeed + (towerData.attackSpeedBonus * upgradeCount);
        CurrentAttackRange = towerData.attackRange + (towerData.attackRangeBonus * upgradeCount);
        CurrentAttackSplashRange = towerData.attackSplashRange +
            (towerData.attackSplashRangeBonus * upgradeCount);
        CurrentAttackTargetCount = towerData.attackTargetCount +
            Mathf.FloorToInt(towerData.attackTargetCountBonus * upgradeCount);

        // 공격 속도에 맞춰 애니메이션 속도 조절
        SetAnimationSpeed(CurrentAttackSpeed);
    }

    // 타워 공격 로직
    private void Shoot()
    {
        if (projectilePrefab == null) return;

        Enemy[] targets = FindEnemiesInAttackRange();
        if (targets.Length == 0)
        {
            animator.SetBool("IsAttack", false);
            return;
        }
        animator.SetBool("IsAttack", true);
        int count = Mathf.Min(targets.Length, CurrentAttackTargetCount);

        // 타워 공격 사운드 이름
        string effectName = towerData.towerType.ToString() + "Attack";

        // 타워 사운드 재생
        soundManager.Play(effectName);

        for (int i = 0; i < count; i++)
        {
            CreateProjectile(targets[i]);
        }

    }

    // 공격 범위 내 적 찾기
    private Enemy[] FindEnemiesInAttackRange()
    {
        enemyBuffer.Clear();
        colliderBuffer.Clear();

        int hitCount = Physics2D.OverlapCircle(
            transform.position,
            CurrentAttackRange,
            new ContactFilter2D().NoFilter(),
            colliderBuffer);

        foreach (Collider2D col in colliderBuffer)
        {
            if (col.TryGetComponent(out Enemy e))
            {
                enemyBuffer.Add(e);
            }
        }

        return enemyBuffer.ToArray();
    }

    // 투사체 생성
    private void CreateProjectile(Enemy target)
    {
        GameObject go = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = go.GetComponent<Projectile>();

        if (p == null)
        {
            p = go.AddComponent<Projectile>();
        }

        bool isSplash = CurrentAttackSplashRange > 0f;

        p.InitProjectile(CurrentAttack, target, CurrentAttackSplashRange,
            towerData.attackType, isSplash);
    }

    // 타워 공격 애니메이션 속도를 공속에 맞추어 조절
    private void SetAnimationSpeed(float speed)
    {
        if (animator != null)
        {
            animator.speed = speed;
        }
    }

    // 타워 공격 범위 기즈모 그리기
    //private void OnDrawGizmos()
    //{
    //    if (towerData == null || currentAttackRange < 0f) return;
    //
    //    Gizmos.color = Color.red;
    //
    //    Gizmos.DrawWireSphere(transform.position, currentAttackRange);
    //}
}