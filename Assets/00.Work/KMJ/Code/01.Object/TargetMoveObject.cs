using DG.Tweening;
using UnityEngine;

namespace KMJ.Code.Object
{
    public class TargetMoveObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform Secondtarget;
        [SerializeField] private Transform rightTarget;
        [SerializeField] private float moveTime;
        [SerializeField] private GameObject thisGameObject;
        [SerializeField] private float waitTime;
        
        
        private Sequence _seq;
        
        public void Interact()
        {
            _seq = DOTween.Sequence();

            if (Secondtarget != null)
            {
                _seq.Append(thisGameObject.transform.DOMove(target.position, moveTime).SetEase(Ease.Linear))
                    .AppendInterval(waitTime)
                    .Append(thisGameObject.transform.DOMove(Secondtarget.position, moveTime).SetEase(Ease.Linear));
            }
            else
            {
                _seq.Append(thisGameObject.transform.DOMove(target.position, moveTime).SetEase(Ease.Linear));
            }
        }

        public void InteractOnce()
        {
            _seq = DOTween.Sequence();

            _seq.Append(thisGameObject.transform.DOMove(target.position, 0.2f).SetEase(Ease.Linear))
                .AppendInterval(0.1f)
                .Append(thisGameObject.transform.DOMove(rightTarget.position, 0.2f).SetEase(Ease.Linear));
        }
    }
}