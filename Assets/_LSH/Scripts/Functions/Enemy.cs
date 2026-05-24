using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static GameEnums;

public class Enemy : MonoBehaviour
{
    EnemyData enemyData;                // 적 데이터가 들어 있는 SO (ScriptableObject)

    private string enemyName;           // 적 이름
    public float currentHp;             // 적 현재 체력
    private float maxHp;                // 적 최대 체력
    private float speed;                // 적 이동 속도
    private float armor;                // 적 방어력
    public ArmorType armorType;         // 적 방어 타입
    private int rewardGold;             // 적 처치 시 획득 골드
    private bool isDying;               // 적 사망 상태 여부

    private Transform target;           // 적이 이동할 목표지점
    private int wayPointIndex = 0;      // 현재 향하는 웨이포인트 인덱스

    [Header("적 체력바 UI")]
    [SerializeField]
    private Image healthBar;            // 적 HpBar
    [SerializeField]
    private Image healthBarBackground;  // 적 HpBar 배경

    [Header("적 애니메이션 설정")]
    [SerializeField]
    private Animator animator;              // 적 애니메이터
    [SerializeField]
    private SpriteRenderer spriteRenderer;  // 적 스프라이트 렌더러
    public bool hasLeftAnimation;           // 좌측 방향 애니메이션 존재 여부

    GameManager gameManager;
    UIManager uiManager;
    WaveManager waveManager;
    SoundManager soundManager;
    private void Awake()
    {
        gameManager = GameManager.Instance;
        uiManager = UIManager.Instance;
        waveManager = WaveManager.Instance;
        soundManager = SoundManager.Instance;
    }

    // 적 정보 초기화
    public void InitEnemy(EnemyData data, float multiplier = 1f)
    {
        enemyData = data;

        enemyName = data.enemyName;
        currentHp = data.hp * multiplier;
        speed = data.speed;
        armor = data.armor * multiplier;
        armorType = data.armorType;
        rewardGold = Mathf.FloorToInt(data.rewardGold * multiplier);

        if (multiplier != 1f)
        {
            rewardGold /= 2;
        }

        // 체력바 설정
        if (healthBar != null && healthBarBackground != null)
        {
            // 최대 체력 설정
            maxHp = currentHp;

            // 시작 시 체력바 비활성화
            healthBar.fillAmount = currentHp / maxHp;
            healthBar.gameObject.SetActive(false);
            healthBarBackground.gameObject.SetActive(false);
        }

        // 적 시작 위치
        transform.position = WayPoints.points[0].position;

        // 다음 목표 웨이포인트 설정
        wayPointIndex = 1;
        target = WayPoints.points[wayPointIndex];
    }

    void Update()
    {
        // 다음 웨이포인트 타겟을 향해 이동
        if (target != null)
        {
            MoveToTarget();
        }
    }

    // 적 이동 함수
    private void MoveToTarget()
    {
        // 몬스터가 죽었다면 이동하지 않음
        if (isDying) return;

        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;

        Vector2 dir = (targetPos - currentPos).normalized;

        // 방향 애니메이션 업데이트
        UpdateDirectionAnimation(dir);

        transform.position = currentPos + speed * Time.deltaTime * dir;

        if (Vector2.Distance(currentPos, targetPos) < 0.1f)
        {
            GetNextWayPoint();
        }
    }

    // 다음 웨이포인트 설정 함수
    private void GetNextWayPoint()
    {
        if (wayPointIndex < WayPoints.points.Count - 1)
        {
            wayPointIndex++;
            target = WayPoints.points[wayPointIndex];
        }
        else
        {
            ReachDestination();
        }
    }

    // 마지막 웨이포인트에 도달했을 때
    private void ReachDestination()
    {
        int decreaseHp = 1;

        // 보스가 목적지에 도달했을 때
        if (enemyData.isBoss)
        {
            decreaseHp = 10;
        }

        // 미션 몬스터가 목적지에 도달했을 때
        else if (enemyData.isMission)
        {
            decreaseHp = 5;
            uiManager.UpdateMissionEnemyUI(waveManager.missionEnemyMultiplier);
        }

        // 일반 웨이브 몬스터가 목적지에 도달했을 때
        else
        {
            gameManager.Gold += rewardGold * 2;
        }

        // 플레이어 남은 체력(목숨) 감소
        gameManager.DecreasePlayerHp(decreaseHp);

        Destroy(gameObject);
        waveManager.aliveEnemyCount--;
    }

    // 적 데미지 처리
    public void TakeDamage(float damage)
    {
        // 방어력을 적용한 최종 데미지
        float finalDamage = Mathf.Max(damage - armor, 0);
        currentHp -= finalDamage;

        if (healthBar != null)
        {
            healthBar.fillAmount = currentHp / maxHp;

            if (!healthBar.IsActive() && !healthBarBackground.IsActive())
            {
                healthBar.gameObject.SetActive(true);
                healthBarBackground.gameObject.SetActive(true);
            }
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // 적 사망 처리
    private void Die()
    {
        // 중복 실행 방지
        if (isDying) return;
        isDying = true;

        // 처치 보상 획득
        gameManager.Gold += rewardGold;
        if (enemyData.isMission)
        {
            uiManager.UpdateMissionEnemyUI(waveManager.missionEnemyMultiplier);
        }

        // 사운드 이름
        string enemyDieSound = enemyData.enemyName + "Die";

        // 적 사망 사운드 재생
        soundManager.Play(enemyDieSound);

        StartCoroutine(EnemyDestroy());

        waveManager.aliveEnemyCount--;
    }

    // 사망 애니메이션 후 적 게임오브젝트 파괴
    IEnumerator EnemyDestroy()
    {
        PlayDieAnimation();
        float animationSeconds = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationSeconds);
        Destroy(gameObject);
    }

    // 현재 방향에 따른 사망 애니메이션 재생
    private void PlayDieAnimation()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;

        Vector2 dir = (targetPos - currentPos).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // 애니메이션에 좌측 방향이 있다.
            if (hasLeftAnimation)
            {
                animator.Play("LeftDie");
                // 우측 방향이면 스프라이트 반전을 한다.
                // 우측 방향
                if (dir.x > 0)
                {
                    spriteRenderer.flipX = true;
                }

                // 좌측 방향
                else
                {
                    spriteRenderer.flipX = false;
                }
            }
            // 애니메이션에 우측 방향만 있다.
            else
            {
                animator.Play("RightDie");
                if (dir.x > 0)
                {
                    spriteRenderer.flipX = false;
                }

                else
                {
                    spriteRenderer.flipX = true;
                }
            }
        }
        else
        {
            // 위쪽 방향
            if (dir.y > 0)
            {
                animator.Play("UpDie");
            }

            // 아래쪽 방향
            if (dir.y < 0)
            {
                animator.Play("DownDie");
            }
        }
    }

    // 방향에 따른 이동 애니메이션
    private void UpdateDirectionAnimation(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // 애니메이션에 좌측 방향이 있다.
            if (hasLeftAnimation)
            {
                animator.Play("LeftMove");

                // 우측 방향이면 스프라이트 반전을 한다.
                // 우측 방향
                if (dir.x > 0)
                {
                    spriteRenderer.flipX = true;
                }

                // 좌측 방향
                else
                {
                    spriteRenderer.flipX = false;
                }
            }
            // 애니메이션에 우측 방향만 있다.
            else
            {
                animator.Play("RightMove");
                if (dir.x > 0)
                {
                    spriteRenderer.flipX = false;
                }

                else
                {
                    spriteRenderer.flipX = true;
                }
            }

        }
        else
        {
            // 위쪽 방향
            if (dir.y > 0)
            {
                animator.Play("UpMove");
            }

            // 아래쪽 방향
            if (dir.y < 0)
            {
                animator.Play("DownMove");
            }
        }
    }
}