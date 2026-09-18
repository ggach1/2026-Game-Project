using UnityEngine;

namespace CIW.Code.System.Interface
{
    // 어떻게 죽었는지 원인
    // 계속해서 추가해 나갈듯함
    public enum DeathCause
    {
        Unknown,
        Spike,
        Fall,
        Crusher,
        Saw,
        Projectile,

        End,
    }

    public readonly struct DeathContext
    {
        public readonly Vector2 HitPoint;
        public readonly Vector2 HitDirection;
        public readonly DeathCause Cause;

        public DeathContext(Vector2 point, Vector2 dir, DeathCause cause)
        {
            HitPoint = point;
            HitDirection = dir;
            Cause = cause;
        }
    }

    public readonly struct PlayerSpawnData
    {
        public Vector2 Position { get; }
        public bool FaceRight { get; }

        public PlayerSpawnData(Vector2 pos, bool faceR)
        {
            Position = pos;
            FaceRight = faceR;
        }
    }

    public interface IKillable
    {
        bool IsAlive { get; }

        void Kill(DeathContext context);
    }

    public interface IPlayerRuleTarget
    {
        void SetHorizontalInverted(bool inverted);
        void SetGravityDirection(Vector2 dir);
        void SetMovementEnabled(bool enabled);
        void SetJumpEnabled(bool enabled);
        void ResetRules();
    }
}
