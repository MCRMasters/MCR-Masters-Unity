using UnityEngine;
using UnityEngine.UI;
using MCRGame.Common;
using MCRGame.UI;

public class TestCallBlockButton : MonoBehaviour
{
    // UI Button에 연결 (Inspector에서 할당)
    public Button testButton;
    
    // CallBlock prefab (Inspector에서 할당)
    public GameObject callBlockPrefab;
    
    // CallBlock을 생성할 부모 Transform (예: Canvas의 하위 오브젝트)
    public Transform callBlockParent;
    
    // 테스트용 CallBlock 기본 설정
    public CallBlockType type = CallBlockType.CHII;
    public GameTile firstTile = GameTile.M1;
    public RelativeSeat sourceSeat = RelativeSeat.KAMI;
    public int sourceTileIndex = 0;

    void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestCreateCallBlock);
        }
        else
        {
            Debug.LogError("TestButton이 할당되지 않았습니다.");
        }
    }

    void TestCreateCallBlock()
    {
        if (callBlockPrefab == null || callBlockParent == null)
        {
            Debug.LogError("CallBlock Prefab 또는 Parent가 할당되지 않았습니다.");
            return;
        }
        
        // CallBlock prefab 인스턴스화
        GameObject newCallBlockObj = Instantiate(callBlockPrefab, callBlockParent);
        
        // localPosition을 명시적으로 (0,0,0)으로 설정
        newCallBlockObj.transform.localPosition = Vector3.zero;
        
        // CallBlock 컴포넌트 가져오기
        CallBlock callBlock = newCallBlockObj.GetComponent<CallBlock>();
        if (callBlock == null)
        {
            Debug.LogError("인스턴스화된 CallBlock prefab에 CallBlock 컴포넌트가 없습니다.");
            return;
        }

        // 새 CallBlockData 생성 후 callBlock.Data에 할당
        var data = new CallBlockData(type, firstTile, sourceSeat, sourceTileIndex);
        callBlock.Data = data;
        
        // CallBlock 초기화 (타일 생성 및 배치)
        callBlock.InitializeCallBlock();
    }
}
