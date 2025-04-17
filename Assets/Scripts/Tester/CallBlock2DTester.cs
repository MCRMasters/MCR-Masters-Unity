// Assets/Scripts/UI/CallBlockTestUI.cs
using UnityEngine;
using MCRGame.Common;
using MCRGame.UI;


namespace MCRGame.Tester
{
    public class CallBlock2DTester : MonoBehaviour
    {
        // 필드 테스트용 컴포넌트 (씬에 배치한 CallBlockField2D 오브젝트를 드래그해 연결)
        public CallBlockField2D field2D;

        // 테스트 파라미터
        private int callTypeIndex = 0;
        private int firstTileIndex = 0;
        private int sourceSeatIndex = 0;
        private int sourceTileIndex = 0;

        private string[] callTypeNames = System.Enum.GetNames(typeof(CallBlockType));
        private string[] tileNames = System.Enum.GetNames(typeof(GameTile));
        private string[] seatNames = System.Enum.GetNames(typeof(RelativeSeat));

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 500), "CallBlock2D Tester", GUI.skin.window);

            GUILayout.Label("1) CallBlockData 설정");

            GUILayout.Label("Type:");
            callTypeIndex = GUILayout.SelectionGrid(callTypeIndex, callTypeNames, 2);

            GUILayout.Label($"FirstTile: {(GameTile)firstTileIndex}");
            firstTileIndex = (int)GUILayout.HorizontalSlider(firstTileIndex, 0, tileNames.Length - 1);

            GUILayout.Label($"SourceSeat: {(RelativeSeat)sourceSeatIndex}");
            sourceSeatIndex = (int)GUILayout.HorizontalSlider(sourceSeatIndex, 0, seatNames.Length - 1);

            GUILayout.Label($"SourceTileIndex: {sourceTileIndex}");
            sourceTileIndex = (int)GUILayout.HorizontalSlider(sourceTileIndex, 0, 2);

            // CallBlockData 생성
            var data = new CallBlockData(
                (CallBlockType)callTypeIndex,
                (GameTile)firstTileIndex,
                (RelativeSeat)sourceSeatIndex,
                sourceTileIndex
            );

            GUILayout.Space(10);
            if (GUILayout.Button("▶ 단일 CallBlock2D 생성"))
            {
                // CallBlock2D 테스트
                var go = new GameObject("TestCallBlock2D", typeof(RectTransform));
                go.transform.SetParent(transform, false);
                var cb2d = go.AddComponent<CallBlock2D>();
                cb2d.Initialize(data);
            }

            GUILayout.Space(5);
            if (field2D != null && GUILayout.Button("▶ CallBlockField2D 에 추가"))
            {
                field2D.AddCallBlock(data);
            }
            else if (field2D == null)
            {
                GUILayout.Label("※ field2D 참조를 연결하세요");
            }

            GUILayout.EndArea();
        }
    }
}