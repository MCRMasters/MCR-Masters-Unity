using UnityEngine;
using System.Collections;
using MCRGame.Common;
using MCRGame.UI;

public class CallBlockFieldTester : MonoBehaviour
{
    public CallBlockField callBlockField;
    public float spawnInterval = 1.0f;
    
    private Coroutine spawnRoutine;
    private bool isTesting = false;

    private void Start()
    {
        if (callBlockField == null)
        {
            callBlockField = GetComponent<CallBlockField>();
            if (callBlockField == null)
            {
                Debug.LogError("CallBlockField 컴포넌트를 찾을 수 없습니다.");
                return;
            }
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Start Random Test"))
        {
            if (!isTesting)
                StartRandomTest();
        }

        if (GUI.Button(new Rect(10, 50, 150, 30), "Stop Test"))
        {
            StopTest();
        }

        if (GUI.Button(new Rect(10, 90, 150, 30), "Clear All"))
        {
            callBlockField.ClearAllCallBlocks();
        }
    }

    private void StartRandomTest()
    {
        if (isTesting) return;
        
        isTesting = true;
        spawnRoutine = StartCoroutine(SpawnRandomCallBlocks());
    }

    private void StopTest()
    {
        if (!isTesting) return;
        
        isTesting = false;
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
    }

    private IEnumerator SpawnRandomCallBlocks()
    {
        while (isTesting)
        {
            // 랜덤한 CallBlock 생성
            CreateRandomCallBlock();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void CreateRandomCallBlock()
    {
        // 랜덤 타입 선택 (FLOWER 제외)
        CallBlockType randomType = GetRandomCallBlockType();
        
        // 랜덤 타일 선택 (FLOWER 제외)
        GameTile randomTile = GetRandomNonFlowerTile();
        
        // 랜덤 소스 시트 선택
        RelativeSeat randomSeat = (RelativeSeat)Random.Range(0, 4);
        
        // 소스 타일 인덱스 (CHII인 경우에만 의미 있음)
        int sourceIndex = Random.Range(0, 3);
        
        // 새 CallBlockData 생성
        CallBlockData data = new CallBlockData(randomType, randomTile, randomSeat, sourceIndex);
        
        // CallBlockField에 데이터 전달하여 CallBlock 생성 및 배치
        callBlockField.AddCallBlock(data);
    }

    private CallBlockType GetRandomCallBlockType()
    {
        // FLOWER 제외한 랜덤 타입 (여기서는 0~4까지의 값)
        int random = Random.Range(0, 5);
        return (CallBlockType)random;
    }

    private GameTile GetRandomNonFlowerTile()
    {
        // FLOWER 제외한 랜덤 타일 (GameTile.F0 이상의 값은 꽃 타일이므로 제외)
        int random = Random.Range((int)GameTile.M1, (int)GameTile.F0);
        return (GameTile)random;
    }
}
