using UnityEngine;

public class GameEnums
{
    // 타워 타입 enum
    public enum TowerType
    {
        Archer,     // 단일 투사체 공격
        Cannon,     // 스플래시 데미지
        Magic,      // 마법 데미지 공격
        Flame,      // 화염 지속 데미지
        Lightning   // 체인 연쇄 공격
    }

    // 타워 등급 enum
    // 낮은 등급부터 높은 순서
    public enum TowerRank
    {
        Normal, // 노멀 등급
        Magic,  // 매직 등급
        Rare,   // 레어 등급
        Unique, // 유니크 등급
        Epic    // 에픽 등급
    }

    // 타워 공격 타입 enum
    public enum AttackType
    {
        Normal,     // 일반형 공격타입 (모든 방어타입에 데미지 1배)
        Explosive,  // 폭발형 공격타입 (대형 방어타입 데미지 1배, 중형 0.75배, 소형 0.5배)
        Concussive  // 진동형 공격타입 (대형 방어타입 데미지 0.25배, 중형 0.5배, 소형 1배)
    }

    public enum AttackMode
    {
        SingleTarget,   // 단일 타겟 공격
        MultiTarget,    // 다중 타겟 공격
        Splash          // 스플래시 공격
    }

    // 적 방어 장갑 타입 enum
    public enum ArmorType
    {
        Small,          // 소형 방어타입
        Medium,         // 중형 방어타입
        Large           // 대형 방어타입
    }
}