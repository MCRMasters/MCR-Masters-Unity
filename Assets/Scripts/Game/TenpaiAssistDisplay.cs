using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using MCRGame.Common;
using MCRGame.UI;

namespace MCRGame.Game
{
    public class TenpaiAssistDisplay : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RectTransform TenpaiAssistPanel;
        [SerializeField] private GameObject assistEntryPrefab;
        [SerializeField] private Button assistButton;
        [SerializeField] private float margin = 20f;
        [SerializeField] private float spacing = 30f;

        private GameObject tenpaiAssistContainer;
        private GameObject buttonAssistContainer;
        private GameTile? lastHoverTile;
        private string lastEntriesKey;

        void Update()
        {
            var nowList = GameManager.Instance.NowTenpaiAssistList;
            bool hasEntries = nowList != null && nowList.Count > 0;
            assistButton.gameObject.SetActive(hasEntries);
            if (!hasEntries) HideButtonAssist();

            var hover = GameManager.Instance.NowHoverTile;
            if (hover.HasValue)
                ShowTenpaiAssist(hover.Value);
            else
                HideTenpaiAssist();
        }

        public void OnAssistButtonEnter()
        {
            var entries = GameManager.Instance.NowTenpaiAssistList;
            if (entries != null && entries.Count > 0)
                BuildTenpaiContainer(entries, ref buttonAssistContainer);
        }

        public void OnAssistButtonExit()
        {
            HideButtonAssist();
        }

        private void ShowTenpaiAssist(GameTile tile)
        {
            if (TenpaiAssistPanel == null || assistEntryPrefab == null)
            {
                HideTenpaiAssist();
                return;
            }
            var dict = GameManager.Instance.tenpaiAssistDict;
            if (!dict.TryGetValue(tile, out var entries) || entries == null || entries.Count == 0)
            {
                HideTenpaiAssist();
                return;
            }

            string newKey = string.Join(";", entries.Select(e =>
                $"{e.TenpaiTile}:{e.TsumoResult.total_score},{e.DiscardResult.total_score}"
            ));
            if (tenpaiAssistContainer != null &&
                lastHoverTile == tile &&
                lastEntriesKey == newKey)
                return;

            HideTenpaiAssist();
            lastHoverTile = tile;
            lastEntriesKey = newKey;
            BuildTenpaiContainer(entries, ref tenpaiAssistContainer);
        }

        private void HideTenpaiAssist()
        {
            if (tenpaiAssistContainer != null) Destroy(tenpaiAssistContainer);
            tenpaiAssistContainer = null;
            lastHoverTile = null;
            lastEntriesKey = null;
        }

        private void HideButtonAssist()
        {
            if (buttonAssistContainer != null) Destroy(buttonAssistContainer);
            buttonAssistContainer = null;
        }

        private void ScaleWidthRecursively(Transform t, float factor)
        {
            if (t.TryGetComponent<RectTransform>(out var rt))
            {
                Vector2 sd = rt.sizeDelta;
                sd.x *= factor;
                rt.sizeDelta = sd;
            }
            for (int i = 0; i < t.childCount; i++)
                ScaleWidthRecursively(t.GetChild(i), factor);
        }

        private void SetRaycastTargetsRecursively(Transform t, bool enable)
        {
            if (t.TryGetComponent<Image>(out var img))
                img.raycastTarget = enable;
            if (t.TryGetComponent<Text>(out var txt))
                txt.raycastTarget = enable;
            for (int i = 0; i < t.childCount; i++)
                SetRaycastTargetsRecursively(t.GetChild(i), enable);
        }

        private void BuildTenpaiContainer(
            List<TenpaiAssistEntry> entries,
            ref GameObject container
        )
        {
            if (container != null) return;

            // 1) 루트 컨테이너 생성 & 중앙 정렬
            container = new GameObject(
                "TenpaiAssistContainer",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasRenderer)
            );
            container.transform.SetParent(TenpaiAssistPanel, false);

            var containerRt = container.GetComponent<RectTransform>();
            containerRt.anchorMin = new Vector2(0.5f, 1f);
            containerRt.anchorMax = new Vector2(0.5f, 1f);
            containerRt.pivot = new Vector2(0.5f, 1f);
            containerRt.anchoredPosition = Vector2.zero;

            // 2) 배경 & raycast 해제
            var bg = container.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);
            bg.raycastTarget = false;

            // 3) 가로 레이아웃 + ContentSizeFitter
            var hl = container.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset((int)margin, (int)margin, (int)margin, (int)margin + 40); // bottom에 +40 추가
            hl.spacing = (int)spacing;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;

            var contentFitter = container.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 4) 더미 엔트리: 1만 스프라이트 + 쯔모/론
            var dummyGo = Instantiate(assistEntryPrefab, container.transform, false);
            SetRaycastTargetsRecursively(dummyGo.transform, false);

            var oneManSprite = Tile2DManager.Instance.get_sprite_by_name("1m");
            var dummyUi = dummyGo.GetComponent<TenpaiAssistEntryUI>();
            dummyUi.SetupDummyEntry(oneManSprite, "쯔모", "론");
            ScaleWidthRecursively(dummyGo.transform, 0.8f);

            // 5) 실제 entries 나열
            foreach (var entry in entries)
            {
                var go = Instantiate(assistEntryPrefab, container.transform, false);
                SetRaycastTargetsRecursively(go.transform, false);
                var ui = go.GetComponent<TenpaiAssistEntryUI>();
                ui.Setup(
                    Tile2DManager.Instance
                              .get_sprite_by_name(entry.TenpaiTile.ToCustomString()),
                    entry.TsumoResult.total_score,
                    entry.DiscardResult.total_score,
                    entry.TsumoResult.total_score < 8 && entry.DiscardResult.total_score < 8,
                    8
                );
            }

            // 6) 컨테이너 높이 재조정: 모든 자식 중 최대 높이 기준으로
            contentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            float maxChildH = 0f;
            foreach (Transform child in container.transform)
            {
                if (child.TryGetComponent<LayoutElement>(out var le))
                {
                    maxChildH = Mathf.Max(maxChildH, le.preferredHeight);
                }
                else if (child.TryGetComponent<RectTransform>(out var crt))
                {
                    maxChildH = Mathf.Max(maxChildH, crt.rect.height);
                }
            }

            // bottom 패딩 10픽셀 더해줌
            float totalH = maxChildH + hl.padding.top + hl.padding.bottom;
            containerRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalH);
        }
    }
}
