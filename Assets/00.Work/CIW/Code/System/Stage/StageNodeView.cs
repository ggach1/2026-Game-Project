using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CIW.Code.System.Stage
{
    public class StageNodeView : MonoBehaviour, ISelectHandler, IPointerEnterHandler
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] GameObject lockMark;
        [SerializeField] GameObject completedMark;

        StageDefinition _stageDef;
        Action<StageDefinition> _onSelected;
        Action<StageNodeView> _onFocused;
        StageMapGraphic _door;
        RectTransform _arrow;
        bool _focused;
        public StageDefinition Stage => _stageDef;
        public bool Unlocked => button.interactable;
        public bool Completed { get; private set; }

        private void Awake()
        {
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClick);
        }

        public void Bind(StageDefinition stage, bool unlocked, bool completed, Action<StageDefinition> onSelected)
        {
            _stageDef = stage;
            _onSelected = onSelected;

            title.SetText(stage.DisplayName);
            button.interactable = unlocked;

            lockMark.SetActive(!unlocked);
            completedMark.SetActive(completed);
            Completed = completed;
        }

        public void UseMapStyle(Action<StageNodeView> onFocused)
        {
            _onFocused = onFocused;
            ((RectTransform)transform).sizeDelta = new Vector2(130,160);
            var background = GetComponent<Image>();
            if(background != null) background.color = Color.clear;
            button.transition = Selectable.Transition.None;
            // EventSystem의 방향키/게임패드 탐색과 Submit을 사용해 입력을 중복 처리하지 않습니다.
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
            lockMark.SetActive(false); completedMark.SetActive(false);
            title.rectTransform.anchoredPosition = new Vector2(0,-66);
            title.rectTransform.sizeDelta = new Vector2(180,36);
            title.fontSize = 20; title.enableAutoSizing=true; title.fontSizeMin=12; title.fontSizeMax=20;
            title.color = Unlocked ? StageMapGraphic.Gold : new Color32(118,60,39,255);
            if(!Unlocked) title.text = _stageDef.DisplayName + "\n<size=65%>LOCKED</size>";
            var rect=StageMapPresentation.Rect("Door Icon",transform,Vector2.zero,new Vector2(100,120));
            _door=rect.gameObject.AddComponent<StageMapGraphic>();
            _door.Door=true; _door.Unlocked=Unlocked; _door.Completed=Completed; _door.raycastTarget=false;
            // 화살표 역시 메시로 그려 폰트의 특수문자 지원 여부에 의존하지 않습니다.
            _arrow=StageMapPresentation.Rect("Selection Arrow",transform,new Vector2(0,90),new Vector2(30,24));
            var arrow=_arrow.gameObject.AddComponent<StageMapGraphic>();
            arrow.Arrow=true; arrow.raycastTarget=false;
            SetFocused(false);
        }

        public void SetFocused(bool focused)
        {
            _focused=focused;
            if(_arrow != null) _arrow.gameObject.SetActive(focused);
            if(_door != null) { _door.Focused=focused; _door.SetVerticesDirty(); }
        }

        void Update()
        {
            if(_focused && _arrow != null)
                _arrow.anchoredPosition=new Vector2(0,90+Mathf.Sin(Time.unscaledTime*5)*4);
        }

        public void Focus()
        {
            if(!Unlocked) return;
            if(Unlocked && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
            _onFocused?.Invoke(this);
        }

        public void OnSelect(BaseEventData eventData) => _onFocused?.Invoke(this);
        public void OnPointerEnter(PointerEventData eventData) => Focus();

        private void HandleClick()
        {
            if (button.interactable)
                _onSelected?.Invoke(_stageDef);
        }
    }
}

