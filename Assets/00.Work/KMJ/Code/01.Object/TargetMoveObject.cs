using DG.Tweening;
using UnityEngine;

namespace KMJ.Code.Object
{
    public class TargetMoveObject : MonoBehaviour, IInteractable
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        /// <summary>
        /// 2번 움직이는 방향
        /// </summary>
        [Space(5),SerializeField] private bool isSecondMove;
        [SerializeField] private Transform secondTarget;

        /// <summary>
        /// 1번 움직인 후 개별적으로 한번 더 움직이는 방향
        /// </summary>
        [Space(5),SerializeField] private bool isOnceMove;
        [SerializeField] private Transform onceTarget;
        
        [Space(5)]
        [Header("Setting")]
        
        [Range(0,10)]
        [SerializeField] private float moveTime;
        
        [SerializeField] private GameObject thisGameObject;
        
        [Range(0,10)]
        [SerializeField] private float waitTime;
        
        
        private Sequence _seq;
        
        /// <summary>
        /// 물체를 움직이게 하는 함수
        /// </summary>
        public void Interact()
        {
            _seq = DOTween.Sequence();

            if (isSecondMove)
            {
                _seq.Append(thisGameObject.transform.DOMove(target.position, moveTime).SetEase(Ease.Linear))
                    .AppendInterval(waitTime)
                    .Append(thisGameObject.transform.DOMove(secondTarget.position, moveTime).SetEase(Ease.Linear));
            }
            else
            {
                _seq.Append(thisGameObject.transform.DOMove(target.position, moveTime).SetEase(Ease.Linear));
            }
        }

        /// <summary>
        /// 물체를 움직인 직후에도 한번 다른 방향으로 움직이게 하는 함수
        /// </summary>
        public void InteractOnce()
        {
            if (!isOnceMove)
                return;
            
            _seq = DOTween.Sequence();

            _seq.Append(thisGameObject.transform.DOMove(target.position, 0.2f).SetEase(Ease.Linear))
                .AppendInterval(0.1f)
                .Append(thisGameObject.transform.DOMove(onceTarget.position, 0.2f).SetEase(Ease.Linear));
        }
    }
}