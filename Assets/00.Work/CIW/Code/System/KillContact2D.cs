using CIW.Code.System.Interface;
using UnityEngine;

namespace CIW.Code.System
{
    public static class KillContact2D
    {
        // 사망 요청만 전달합니다. 오브젝트 비활성화/연출/리스폰은 대상의 생명 시스템이 담당합니다.
        public static bool TryKill(Collider2D other, LayerMask targetMask, DeathContext context)
        {
            if (other == null || !other.enabled || !other.gameObject.activeInHierarchy)
                return false;

            var target = other.GetComponentInParent<IKillable>();
            if (target == null || !target.IsAlive)
                return false;

            // 자식 Collider가 Default 레이어여도 부모 생명 오브젝트가 Player 레이어면 허용합니다.
            var owner = target as Component;
            int layers = 1 << other.gameObject.layer;
            if (owner != null) layers |= 1 << owner.gameObject.layer;
            if ((targetMask.value & layers) == 0)
                return false;

            if (target is IKillableHitbox hitbox)
            {
                if (!hitbox.IsDeathHitbox(other)) return false;
            }
            else if (other.isTrigger)
            {
                // 명시적인 피격 계약이 없는 대상의 센서 Trigger는 기본적으로 제외합니다.
                return false;
            }

            target.Kill(context);
            return true;
        }
    }
}
