using DG.Tweening;
using UnityEngine;

namespace KMJ.Code.Object
{
    public class TargetMoveObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform Secondtarget;
        [SerializeField] private float moveTime;
        [SerializeField] private GameObject thisGameObject;
        [SerializeField] private float waitTime;
        
        private Sequence _seq;
        
        public void Interact()
        {
            _seq = DOTween.Sequence();

            _seq.Append(thisGameObject.transform.DOMove(target.position, moveTime).SetEase(Ease.Linear))
                .AppendInterval(waitTime)
                .Append(thisGameObject.transform.DOMove(Secondtarget.position, moveTime).SetEase(Ease.Linear));
        }
    }
}