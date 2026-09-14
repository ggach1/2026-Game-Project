using System.Collections;
using UnityEngine;

namespace CIW.Code.Player
{
    [CreateAssetMenu(fileName = "MovementData", menuName = "SO/Player/MovementData", order = 2)]
    public class PlayerMovementDataSO : ScriptableObject
    {
        [Header("Horizontal Movement")]
        [Min(0f)] public float MoveSpeed = 7f;
        [Min(0f)] public float GroundAcceleration = 70f;
        [Min(0f)] public float GroundDeceleration = 90f;
        [Min(0f)] public float AirAcceleration = 45f;

        [Header("Jump")]
        [Min(0f)] public float JumpPower = 12f;
        [Range(0f, 1f)] public float JumpCutMultiplier = 0.45f;

        [Header("Gravity")]
        [Min(0f)] public float RisingGravity = 28f;
        [Min(0f)] public float FallingGravity = 40f;
        [Min(0f)] public float MaxFallSpeed = 20f;

        [Header("Input Forgiveness")]
        [Min(0f)] public float CoyoteTime = 0.08f;
        [Min(0f)] public float JumpBufferTime = 0.1f;
    }
}
