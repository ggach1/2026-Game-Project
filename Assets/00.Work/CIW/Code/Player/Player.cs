using CIW.Code.System;
using UnityEngine;

namespace CIW.Code.Player
{
    public class Player : Entity
    {
        [field : SerializeField] public InputSO PlayerInput { get; private set; }
    }
}
