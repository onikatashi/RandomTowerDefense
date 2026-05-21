using UnityEngine;

public class HitEffectDestroy : StateMachineBehaviour
{
    // 애니메이션 상태가 종료될 때 호출
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 애니메이터가 붙어있는 게임오브젝트 파괴
        Destroy(animator.gameObject);
    }
}