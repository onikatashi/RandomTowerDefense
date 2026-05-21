using UnityEngine;
using static GameEnums;

public class Projectile : MonoBehaviour
{
    private float damage;                   // 투사체 데미지
    private Enemy target;                   // 투사체 목표 타겟
    private float splashRange;              // 투사체 스플래시 범위
    private AttackType attackType;          // 투사체 공격 타입
    private bool isSplash;                  // 스플래시 여부 확인

    public float projectileSpeed = 10f;     // 투사체 속도

    [Header("피격 시 이펙트 설정")]
    public GameObject hitEffectPrefab;      // 피격 시 생성할 이펙트 프리팹

    // 투사체 초기화
    public void InitProjectile(float damage, Enemy target, float splashRange,
        AttackType attackType, bool isSplash)
    {
        this.damage = damage;
        this.target = target;
        this.splashRange = splashRange;
        this.attackType = attackType;
        this.isSplash = isSplash;
    }

    void Update()
    {
        // 타겟이 사라지면 투사체 파괴
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 dir = (target.transform.position - transform.position).normalized;

        transform.up = dir;

        transform.position = (Vector2)transform.position + projectileSpeed * Time.deltaTime * dir;

        // 투사체가 타겟에 도달하면 적중 처리
        if (Vector2.Distance(transform.position, target.transform.position) < 0.1f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        // 스플래시 데미지 처리
        if (isSplash)
        {
            ApplySplashDamage();
        }
        // 일반 단일 타겟 데미지 처리
        else
        {
            ApplyDamage(target);
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, target.transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // 데미지 적용
    private void ApplyDamage(Enemy e)
    {
        if (e != null)
        {
            e.TakeDamage(CalculateDamage(e));
        }
    }

    // 스플래시 범위 내의 타겟 주위 몬스터들에게 데미지 적용
    private void ApplySplashDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, splashRange);

        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponent<Enemy>();
            ApplyDamage(e);
        }

    }

    // 공격 타입, 방어 타입에 따른 최종 데미지 계산
    private float CalculateDamage(Enemy e)
    {
        switch (attackType)
        {
            case AttackType.Normal:
                return damage;

            case AttackType.Explosive:
                switch (e.armorType)
                {
                    case ArmorType.Large: return damage * 1f;
                    case ArmorType.Medium: return damage * 0.75f;
                    case ArmorType.Small: return damage * 0.5f;
                }
                break;

            case AttackType.Concussive:
                switch (e.armorType)
                {
                    case ArmorType.Large: return damage * 0.25f;
                    case ArmorType.Medium: return damage * 0.5f;
                    case ArmorType.Small: return damage * 1.0f;
                }
                break;
        }
        return damage;
    }
}