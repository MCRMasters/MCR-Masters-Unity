using UnityEngine;
using MCRGame.Common;
using MCRGame.UI;
using System;

namespace MCRGame.Tester
{
    public class CallBlockTester : MonoBehaviour
    {
        // 테스트 시 생성될 CallBlock들이 배치될 부모.
        // 별도로 할당하지 않으면, 현재 GameObject의 transform을 사용합니다.
        public Transform testParent;

        // 현재 생성된 CallBlock을 저장합니다.
        private CallBlock currentCallBlock;

        void OnGUI()
        {
            // 버튼 위치, 크기 등은 필요에 따라 조절하세요.
            if (GUI.Button(new Rect(10, 10, 220, 40), "Create PUNG CallBlock"))
            {
                CreateCallBlock(CallBlockType.PUNG);
            }

            if (GUI.Button(new Rect(10, 60, 220, 40), "Create AN_KONG CallBlock"))
            {
                CreateCallBlock(CallBlockType.AN_KONG);
            }

            if (GUI.Button(new Rect(10, 110, 220, 40), "Apply ShominKong (on PUNG)"))
            {
                if (currentCallBlock != null)
                {
                    currentCallBlock.ApplyShominKong();
                }
                else
                {
                    Debug.LogWarning("CallBlock이 생성되지 않았습니다.");
                }
            }

            if (GUI.Button(new Rect(10, 160, 220, 40), "Clear CallBlock"))
            {
                ClearCallBlock();
            }
        }

        /// <summary>
        /// 지정된 타입의 CallBlock을 생성합니다.
        /// SHOMIN_KONG은 PUNG 타입 CallBlock에 대해 ApplyShominKong을 통해 적용되므로,
        /// 여기서는 PUNG 또는 AN_KONG 타입만 직접 생성합니다.
        /// </summary>
        /// <param name="type">CallBlockType: PUNG 또는 AN_KONG</param>
        void CreateCallBlock(CallBlockType type)
        {
            // 기존 CallBlock 있으면 제거
            ClearCallBlock();

            // 부모 transform 지정: testParent가 할당되어 있으면 사용, 그렇지 않으면 자기 자신의 transform
            Transform parentTr = testParent != null ? testParent : transform;

            // 새로운 GameObject 생성 및 CallBlock 컴포넌트 추가
            GameObject cbObj = new GameObject("TestCallBlock");
            cbObj.transform.SetParent(parentTr, false);
            CallBlock cb = cbObj.AddComponent<CallBlock>();

            // CallBlockData 생성: 여기서는 기본값으로 설정 (원하는 값으로 수정 가능)
            CallBlockData data = new CallBlockData();
            data.Type = type;
            // 예시로 PUNG나 AN_KONG 타입에서는 3장으로 처리하도록 합니다.
            data.FirstTile = GameTile.M1; // 예시 tile (원하는 tile로 변경 가능)
            data.SourceTileIndex = 0;     // 회전 타일 인덱스 (예제에서는 0번을 회전된 타일로 가정)
            data.SourceSeat = RelativeSeat.KAMI; // 예시로 KAMI 선택
            cb.Data = data;

            // CallBlock 초기화: 내부 타일 생성 및 배치
            cb.InitializeCallBlock();

            // 생성한 CallBlock을 currentCallBlock에 할당
            currentCallBlock = cb;
        }

        void ClearCallBlock()
        {
            if (currentCallBlock != null)
            {
                Destroy(currentCallBlock.gameObject);
                currentCallBlock = null;
            }
        }
    }
}