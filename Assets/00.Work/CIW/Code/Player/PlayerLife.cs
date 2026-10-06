using CIW.Code.System.Interface;
using System;
using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    public enum PlayerLifeState
    {
        Alive,
        Dead,
        Respawning,
        Escaped,
        EnteringExit
    }

    public class PlayerLife : DevLib.ModuleSystem.Module
    {
        public PlayerLifeState State { get; private set; } = PlayerLifeState.Alive;
        public PlayerSpawnData SpawnData { get; private set; }
        public bool IsAlive => State == PlayerLifeState.Alive;

        PlayerInputController _inputController;
        PlayerMotor2D _motor;
        PlayerView _view;
        PlayerRuleController _rules;
        PlayerGroundSensor _groundSensor;

        public event Action<DeathContext> Died;
        public event Action Respawned;
        public event Action Escaped;

        public bool TryEscape()
        {
            if (State != PlayerLifeState.Alive && State != PlayerLifeState.EnteringExit) return false;
            // 완료 상태를 먼저 확정해 같은 프레임의 다른 출구/함정이 중복 처리하지 못하게 합니다.
            State = PlayerLifeState.Escaped;
            _inputController.SetInputEnabled(false);
            _motor.SetSimulationEnabled(false);
            _view.PlayEscape();
            Escaped?.Invoke();
            return true;
        }

        public bool TryBeginExit(Vector3 target)
        {
            if (State != PlayerLifeState.Alive || !_groundSensor.IsGrounded) return false;
            State = PlayerLifeState.EnteringExit;
            _inputController.SetInputEnabled(false);
            _motor.SetSimulationEnabled(false);
            _view.BeginExit(target);
            return true;
        }

        public void CancelExit()
        {
            if (State != PlayerLifeState.EnteringExit) return;
            _view.ResetView(_owner.GetModule<CIW.Code.System.EntityAnimator>().GetFacingDirection().x >= 0f);
            State = PlayerLifeState.Alive;
            _motor.SetSimulationEnabled(true);
            _inputController.SetInputEnabled(true);
        }

        public override void Initialize(DevLib.ModuleSystem.ModuleOwner owner)
        {
            base.Initialize(owner);
            _inputController = owner.GetModule<PlayerInputController>();
            _motor = owner.GetModule<PlayerMotor2D>();
            _view = owner.GetModule<PlayerView>();
            _rules = owner.GetModule<PlayerRuleController>();
            _groundSensor = owner.GetModule<PlayerGroundSensor>();
            SpawnData = new PlayerSpawnData(owner.transform.position, true);
        }

        public void Kill(DeathContext context)
        {
            if (State != PlayerLifeState.Alive)
                return;

            State = PlayerLifeState.Dead;

            // 이벤트를 보내기 전에 조작과 물리를 막아 한 프레임에 사망이 중복 처리되는 것을 방지합니다.
            _inputController.SetInputEnabled(false);
            _motor.SetSimulationEnabled(false);
            _view.PlayDeath(context);

            Died?.Invoke(context);
        }

        public void Respawn(PlayerSpawnData data)
        {
            if (State == PlayerLifeState.Respawning)
                return;

            State = PlayerLifeState.Respawning;
            SpawnData = data;

            // 살아 있는 상태에서 외부 리스폰을 요청해도 초기화 중 물리와 입력을 차단합니다.
            _inputController.SetInputEnabled(false);
            _motor.SetSimulationEnabled(false);
            _rules.ResetAll();
            _view.ResetView(data.FaceRight);
            _view.PlayRespawn();
            // 외형/Animator 초기화 이후 위치를 최종 확정한 뒤 물리와 피격을 다시 허용합니다.
            _motor.ResetMotion(data.Position);
            _groundSensor.ResetContactState();

            State = PlayerLifeState.Alive;
            _motor.SetSimulationEnabled(true);
            _inputController.SetInputEnabled(true);

            Respawned?.Invoke();
        }
    }
}
