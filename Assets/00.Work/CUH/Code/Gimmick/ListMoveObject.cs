using System;
using System.Collections.Generic;
using KMJ.Code.Object;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick
{
    public class ListMoveObject : MonoBehaviour, IInteractable
    {
        [Serializable]
        private class MoveTarget
        {
            [field: SerializeField] public Transform Target { get; private set; }
            [field: SerializeField, Min(0f)] public float MoveTime { get; private set; } = 1f;
            [field: SerializeField, Min(0f)] public float WaitTime { get; private set; }
        }

        [Header("Target")]
        [SerializeField] private List<MoveTarget> moveTargets = new();

        [Header("Setting")]
        [SerializeField] private GameObject thisGameObject;
        [SerializeField] private bool onlyOnce;

        private int _moveVersion;
        private bool _hasInteracted;

        public bool IsMoving { get; private set; }

        public async void Interact()
        {
            if (isActiveAndEnabled == false || IsMoving || (onlyOnce && _hasInteracted)) return;
            if (moveTargets == null || moveTargets.Count == 0) return;
            if (ValidateTargets() == false) return;

            if (thisGameObject == null)
                thisGameObject = gameObject;

            Transform movingTransform = thisGameObject.transform;
            int moveVersion = ++_moveVersion;
            IsMoving = true;
            _hasInteracted = true;

            for (int i = 0; i < moveTargets.Count; i++)
            {
                if (CanMove(movingTransform, moveVersion) == false) break;

                MoveTarget moveTarget = moveTargets[i];
                if (moveTarget == null || moveTarget.Target == null) break;

                await MoveAsync(movingTransform, moveTarget.Target.position, moveTarget.MoveTime, moveVersion);

                if (CanMove(movingTransform, moveVersion) == false) break;

                await WaitAsync(movingTransform, moveTarget.WaitTime, moveVersion);
            }

            if (this != null && moveVersion == _moveVersion)
                IsMoving = false;
        }

        private async Awaitable MoveAsync(Transform movingTransform, Vector3 targetPosition, float moveTime,
            int moveVersion)
        {
            Vector3 startPosition = movingTransform.position;
            float elapsedTime = 0f;

            while (elapsedTime < moveTime)
            {
                await Awaitable.NextFrameAsync();

                if (CanMove(movingTransform, moveVersion) == false) return;

                elapsedTime += Time.deltaTime;
                movingTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveTime);
            }

            movingTransform.position = targetPosition;
        }

        private async Awaitable WaitAsync(Transform movingTransform, float waitTime, int moveVersion)
        {
            float elapsedTime = 0f;

            while (elapsedTime < waitTime)
            {
                await Awaitable.NextFrameAsync();

                if (CanMove(movingTransform, moveVersion) == false) return;

                elapsedTime += Time.deltaTime;
            }
        }

        private bool CanMove(Transform movingTransform, int moveVersion)
        {
            return this != null && isActiveAndEnabled && movingTransform != null && moveVersion == _moveVersion;
        }

        private bool ValidateTargets()
        {
            for (int i = 0; i < moveTargets.Count; i++)
            {
                MoveTarget moveTarget = moveTargets[i];
                if (moveTarget == null || moveTarget.Target == null)
                {
                    Debug.LogError($"{gameObject.name}의 이동 목록 {i}번에 타겟을 설정해야 합니다.", this);
                    return false;
                }

                if (float.IsNaN(moveTarget.MoveTime) || float.IsInfinity(moveTarget.MoveTime) ||
                    float.IsNaN(moveTarget.WaitTime) || float.IsInfinity(moveTarget.WaitTime) ||
                    moveTarget.MoveTime < 0f || moveTarget.WaitTime < 0f)
                {
                    Debug.LogError($"{gameObject.name}의 이동 목록 {i}번 시간은 유한한 0 이상의 값이어야 합니다.", this);
                    return false;
                }
            }

            return true;
        }

        private void OnDisable()
        {
            // 다시 활성화되어도 이전 비동기 실행이 새 이동을 변경하지 않도록 구분합니다.
            _moveVersion++;
            IsMoving = false;
        }
    }
}
