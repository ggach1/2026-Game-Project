using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    public class PlayerRuleController : DevLib.ModuleSystem.Module
    {
        public bool CanMove { get; private set; } = true;
        public bool CanJump { get; private set; } = true;
        public bool IsHorizontalInverted { get; private set; }

        public float MoveMultiplier { get; private set; } = 1f;
        public float JumpMultiplier { get; private set; } = 1f;
        public Vector2 GravityDirection { get; private set; } = Vector2.down;

        public void SetHorizontalInverted(bool inverted)
        {
            IsHorizontalInverted = inverted;
        }

        public void SetGravityDirection(Vector2 dir)
        {
            // 0 벡터로는 중력과 점프 방향을 계산할 수 없어 기본 방향으로 되돌립니다.
            GravityDirection = dir.sqrMagnitude > Mathf.Epsilon ? dir.normalized : Vector2.down;
        }

        public void SetMovementEnabled(bool enabled)
        {
            CanMove = enabled;
        }

        public void SetJumpEnabled(bool enabled)
        {
            CanJump = enabled;
        }

        public void SetMovementMultiplier(float multi)
        {
            MoveMultiplier = Mathf.Max(0f, multi);
        }

        public void SetJumpMultiplier(float multi)
        {
            JumpMultiplier = Mathf.Max(0f, multi);
        }

        public void ResetAll()
        {
            CanMove = true;
            CanJump = true;
            IsHorizontalInverted = false;
            MoveMultiplier = 1f;
            JumpMultiplier = 1f;
            GravityDirection = Vector2.down;
        }
    }
}
