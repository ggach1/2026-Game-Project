using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace KMJ.Code.Object
{
    /// <summary>
    /// 크기가 커지거나 작아지는 오브젝트
    /// </summary>
    public class GrowObject : MonoBehaviour, IInteractable
    {
        [Header("Setting")]
        [SerializeField] private GameObject thisGameObj;
        [SerializeField] private Vector3 changeScale;
        [SerializeField] private float duration;
        [SerializeField] private Ease easeType;
        [SerializeField] private bool isOnceGrow;
        
        [Header("ReturnSetting")]
        [SerializeField] private bool isReturnOwnScale;
        [SerializeField] private float waitTime;
        
        private Vector3 _ownScale;
        private int _growCnt = 0;
        
        private void Awake()
        {
            if (thisGameObj != null)
                _ownScale = thisGameObj.transform.localScale;
        }

        public void Interact()
        {
            if (isOnceGrow && _growCnt > 0)
                return;
            
            thisGameObj.transform.DOKill();
            
            Sequence mySequence = DOTween.Sequence();

            if (isReturnOwnScale)
            {
                mySequence.Append(thisGameObj.transform.DOScale(changeScale, duration).SetEase(easeType))
                    .OnComplete(() =>
                    {
                        StartCoroutine(ReturnOwnScale());
                    });
            }
            else
                mySequence.Append(thisGameObj.transform.DOScale(changeScale, duration).SetEase(easeType));

            _growCnt += 1;
        }

        public IEnumerator ReturnOwnScale()
        {
            yield return new WaitForSeconds(waitTime);
            
            thisGameObj.transform.DOKill();
            thisGameObj.transform.DOScale(_ownScale,duration).SetEase(easeType);
        }
    }
}