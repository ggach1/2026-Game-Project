using CIW.Code.System;
using CIW.Code.System.Interface;
using UnityEngine;

namespace CIW.Code.Player
{
    public class Player : Entity, IKillable, IPlayerRuleTarget
    {
        [field : SerializeField] public InputSO PlayerInput { get; private set; }

        public PlayerMotor2D Motor { get; private set; }
        public PlayerLife Life { get; private set; }
        public PlayerRuleController Rules { get; private set; }
        public PlayerView View { get; private set; }
        public bool IsAlive => Life != null && Life.IsAlive;

        protected override void Awake()
        {
            base.Awake();

            Motor = GetModule<PlayerMotor2D>();
            Life = GetModule<PlayerLife>();
            Rules = GetModule<PlayerRuleController>();
            View = GetModule<PlayerView>();
        }

        public void Kill(DeathContext context)
        {
            Life.Kill(context);
        }

        public void Respawn(PlayerSpawnData data)
        {
            Life.Respawn(data);
        }

        // 레벨 오브젝트는 내부 모듈을 직접 참조하지 않고 이 공개 API를 통해 규칙을 변경합니다.
        public void SetHorizontalInverted(bool inverted) => Rules.SetHorizontalInverted(inverted);
        public void SetGravityDirection(Vector2 direction) => Rules.SetGravityDirection(direction);
        public void SetMovementEnabled(bool enabled) => Rules.SetMovementEnabled(enabled);
        public void SetJumpEnabled(bool enabled) => Rules.SetJumpEnabled(enabled);
        public void ResetRules() => Rules.ResetAll();
    }
}
