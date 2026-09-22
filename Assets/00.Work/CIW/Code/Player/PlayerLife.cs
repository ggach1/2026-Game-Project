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
        Respawning
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
            _motor.ResetMotion(data.Position);
            _groundSensor.ResetContactState();
            _view.ResetView(data.FaceRight);
            _view.PlayRespawn();

            State = PlayerLifeState.Alive;
            _motor.SetSimulationEnabled(true);
            _inputController.SetInputEnabled(true);

            Respawned?.Invoke();
        }
    }
}
