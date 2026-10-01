using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CIW.Code.System.Stage
{
    // 기존 씬의 직렬화 참조를 유지하면서 표현 계층만 생성합니다. 원본 프리팹/진행 데이터는 변경하지 않습니다.
    public sealed class StageMapPresentation
    {
        readonly RectTransform _panel;
        readonly RectTransform _content;
        readonly TMP_Text _details;
        readonly TMP_Text _progress;
        readonly StageMapGraphic _map;

        public StageMapPresentation(GameObject panel, RectTransform nodeRoot, string heading)
        {
            _panel = (RectTransform)panel.transform;
            _panel.anchorMin = Vector2.zero; _panel.anchorMax = Vector2.one;
            _panel.offsetMin = _panel.offsetMax = Vector2.zero;
            var image = panel.GetComponent<Image>();
            if (image != null) image.color = new Color32(105,43,43,255);
            // 테스트 제목만 감춥니다. 사용자 추가 오브젝트나 다른 Canvas는 건드리지 않습니다.
            var oldTitle = panel.transform.Find("Title");
            var oldSubtitle = panel.transform.Find("Subtitle");
            if(oldTitle != null) oldTitle.gameObject.SetActive(false);
            if(oldSubtitle != null) oldSubtitle.gameObject.SetActive(false);

            _content = Rect("World Map Presentation",_panel,Vector2.zero,new Vector2(1280,720));
            Label("World",_content,"W O R L D   0 1",new Vector2(0,277),new Vector2(900,30),18);
            var shadow=Label("Title Shadow",_content,heading,new Vector2(3,214),new Vector2(1000,80),64);
            shadow.color=StageMapGraphic.Ink;
            Label("Map Title",_content,heading,new Vector2(0,220),new Vector2(1000,80),64);
            var terrain=Rect("Terrain and Unlock Routes",_content,new Vector2(0,-5),new Vector2(1050,380));
            _map=terrain.gameObject.AddComponent<StageMapGraphic>(); _map.raycastTarget=false;
            nodeRoot.SetParent(terrain,false); nodeRoot.anchoredPosition=Vector2.zero;
            nodeRoot.sizeDelta=new Vector2(1000,350);
            _details=Label("Selected Stage",_content,"CHOOSE A DOOR",new Vector2(0,-233),new Vector2(1100,42),26);
            _progress=Label("Completion",_content,"",new Vector2(0,-273),new Vector2(1000,32),18);
            var help=Label("Navigation Hint",_content,"CLICK A DOOR TO ENTER    /    ARROWS + ENTER",new Vector2(0,-322),new Vector2(1100,30),16);
            help.color=new Color32(222,160,111,255);
            Fit();
        }

        public void Refresh(WorldDefinition world, StageProgressService progress)
        {
            _map.SetRoutes(world,progress);
            int total=0, complete=0;
            foreach(var entry in world.Entries)
            {
                if(entry.Stage==null) continue;
                total++; if(progress.IsCompleted(entry.Stage.StageId)) complete++;
            }
            _progress.text=$"{complete:00} / {total:00} DOORS CLEARED";
            _details.text=total==0 ? "NO STAGES REGISTERED" : "CHOOSE A DOOR";
        }

        public void Describe(StageNodeView node)
        {
            string state = !node.Unlocked ? "LOCKED" : node.Completed ? "CLEARED  /  REPLAY" : "READY";
            _details.text=$"{node.Stage.DisplayName}   /   {node.Stage.Sections.Count} SECTIONS   /   {state}";
        }

        public void Fit()
        {
            // 16:9를 강제하지 않고 지도 전체를 화면 안에 맞춥니다(와이드/작은 Game 뷰 공통).
            float scale=Mathf.Min(_panel.rect.width/1280f,_panel.rect.height/720f);
            _content.localScale=Vector3.one*Mathf.Max(.01f,scale);
        }

        public static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one*.5f;
            rect.anchoredPosition=position; rect.sizeDelta=size; return rect;
        }

        public static TextMeshProUGUI Label(string name, Transform parent, string text, Vector2 position, Vector2 size, float fontSize)
        {
            var label=Rect(name,parent,position,size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font=TMP_Settings.defaultFontAsset; label.text=text; label.fontSize=fontSize;
            label.fontStyle=FontStyles.Bold; label.alignment=TextAlignmentOptions.Center;
            label.color=StageMapGraphic.Gold; label.raycastTarget=false;
            label.enableAutoSizing=true; label.fontSizeMin=12; label.fontSizeMax=fontSize;
            return label;
        }
    }
}
