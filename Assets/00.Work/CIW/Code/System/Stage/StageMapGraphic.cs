using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CIW.Code.System.Stage
{
    // 스프라이트 의존성 없이 UI 메시로 지도/문을 그립니다. 게임 판정과는 무관한 표현 전용입니다.
    [RequireComponent(typeof(CanvasRenderer))]
    public class StageMapGraphic : MaskableGraphic
    {
        protected override void Awake()
        {
            // 기본 Graphic은 구형 메시 경로를 사용합니다. VertexHelper로 직접 그리는
            // 이 컴포넌트는 해당 경로를 끄고, 렌더러도 생성 시점부터 반드시 갖춥니다.
            useLegacyMeshGeneration = false;
            base.Awake();
        }

        public bool Door { get; set; }
        public bool Arrow { get; set; }
        public bool Unlocked { get; set; }
        public bool Completed { get; set; }
        public bool Focused { get; set; }
        readonly List<(Vector2 from, Vector2 to, bool open)> _routes = new();
        public static readonly Color Ink = new Color32(64, 30, 34, 255);
        public static readonly Color Gold = new Color32(255, 222, 103, 255);

        public void SetRoutes(WorldDefinition world, StageProgressService progress)
        {
            _routes.Clear();
            // 배열 순서가 아닌 실제 해금 관계를 연결하므로 분기형 월드에서도 거짓 경로가 생기지 않습니다.
            foreach (var entry in world.Entries)
            {
                if (entry.Stage == null) continue;
                foreach (var id in entry.Stage.UnlockStage)
                    foreach (var target in world.Entries)
                        if (target.Stage != null && target.Stage.StageId == id)
                            _routes.Add((entry.Position + Vector2.down * 36,
                                target.Position + Vector2.down * 36,
                                progress.IsUnlocked(target.Stage, world)));
            }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (Arrow)
            {
                vh.AddVert(new Vector3(-12,8,0),Gold,Vector2.zero);
                vh.AddVert(new Vector3(12,8,0),Gold,Vector2.zero);
                vh.AddVert(new Vector3(0,-8,0),Gold,Vector2.zero);
                vh.AddTriangle(0,1,2);
                return;
            }
            if (Door) { DrawDoor(vh); return; }
            var land = new Vector2[] { new(-490,-90), new(-430,-158), new(-240,-180),
                new(-100,-156), new(65,-180), new(280,-154), new(455,-100), new(495,5),
                new(420,105), new(260,150), new(100,128), new(-60,163), new(-280,142), new(-465,55) };
            Polygon(vh, land, new Vector2(0,-18), new Color32(87,37,37,255));
            Polygon(vh, land, Vector2.zero, new Color32(191,104,57,255));
            // 작은 지형 흔적은 버튼이 아닙니다. 입력을 가로채지 않도록 raycastTarget을 끕니다.
            for (int i = 0; i < 18; i++)
            {
                float x = -390 + (i * 137 % 780);
                float y = -105 + (i * 59 % 210);
                Quad(vh, new Rect(x,y,18 + i % 3 * 8,4), new Color32(166,81,48,255));
            }
            foreach (var route in _routes)
            {
                float distance = Vector2.Distance(route.from, route.to);
                int count = Mathf.Max(1, Mathf.CeilToInt(distance / 18));
                for (int i = 1; i < count; i++)
                {
                    float t = (float)i / count;
                    var p = Vector2.Lerp(route.from,route.to,t);
                    p.y -= Mathf.Sin(t * Mathf.PI) * 35;
                    Quad(vh,new Rect(p.x-4,p.y-3,8,6),route.open ? Gold : new Color32(133,65,44,255));
                }
            }
        }

        void DrawDoor(VertexHelper vh)
        {
            var frame = !Unlocked ? new Color32(143,87,52,255) : Focused ? Gold : new Color32(239,175,76,255);
            Quad(vh,new Rect(-43,-40,86,10),new Color32(128,62,43,255));
            // 계단형 아치와 굵은 외곽선으로 작은 화면에서도 '카드'가 아닌 문으로 읽히게 합니다.
            Quad(vh,new Rect(-34,-32,68,78),Ink);
            Quad(vh,new Rect(-25,46,50,10),Ink);
            Quad(vh,new Rect(-15,56,30,7),Ink);
            Quad(vh,new Rect(-27,-32,54,74),frame);
            Quad(vh,new Rect(-19,42,38,8),frame);
            Quad(vh,new Rect(-19,-32,38,69),Unlocked ? Ink : new Color32(121,68,43,255));
            Quad(vh,new Rect(9,-5,5,6),frame);
            if (Completed)
            {
                Quad(vh,new Rect(-8,2,7,13),Gold);
                Quad(vh,new Rect(-1,2,7,7),Gold);
                Quad(vh,new Rect(6,9,7,18),Gold);
            }
            if (!Unlocked)
            {
                Quad(vh,new Rect(-8,2,16,14),Ink);
                Quad(vh,new Rect(-5,16,10,7),Ink);
                Quad(vh,new Rect(-2,7,4,5),frame);
            }
        }

        static void Quad(VertexHelper vh, Rect r, Color c)
        {
            int n=vh.currentVertCount;
            vh.AddVert(new Vector3(r.xMin,r.yMin),c,Vector2.zero);
            vh.AddVert(new Vector3(r.xMin,r.yMax),c,Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMax),c,Vector2.zero);
            vh.AddVert(new Vector3(r.xMax,r.yMin),c,Vector2.zero);
            vh.AddTriangle(n,n+1,n+2); vh.AddTriangle(n,n+2,n+3);
        }

        static void Polygon(VertexHelper vh, Vector2[] points, Vector2 offset, Color c)
        {
            int n=vh.currentVertCount;
            vh.AddVert(offset,c,Vector2.zero);
            foreach(var point in points) vh.AddVert(point+offset,c,Vector2.zero);
            for(int i=0;i<points.Length;i++) vh.AddTriangle(n,n+1+i,n+1+(i+1)%points.Length);
        }
    }
}
