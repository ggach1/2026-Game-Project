using CIW.Code.Player;
using System;
using UnityEngine;

namespace CIW.Code.System.Stage
{
    public class SectionContext : MonoBehaviour
    {
        [SerializeField] Transform spawnPoint;
        [SerializeField] bool faceRight = true;
        [SerializeField] ExitDoor[] exitDoors;

        public Transform SpawnPoint => spawnPoint;
        public bool FaceRight => faceRight;

        // 어느 구간에서 누가 탈출했는지를 전달한다
        public event Action<SectionContext, Player.Player> Escaped;

        private void OnEnable()
        {
            foreach (var door in exitDoors)
            {
                if (door != null)
                    door.OnEscaped.AddListener(HandleEscaped);
            }
        }

        private void OnDisable()
        {
            foreach (var door in exitDoors)
            {
                if (door != null)
                    door.OnEscaped.RemoveListener(HandleEscaped);
            }
        }

        private void HandleEscaped(Player.Player entity)
        {
            Escaped?.Invoke(this, entity);
        }
    }
}

