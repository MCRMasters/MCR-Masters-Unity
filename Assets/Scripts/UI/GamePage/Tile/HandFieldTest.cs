using UnityEngine;
using MCRGame.UI;

public class HandFieldTest : MonoBehaviour
{
    private HandField handField;

    private void Start()
    {
        // 같은 게임 오브젝트에 HandField 컴포넌트가 붙어있어야 합니다.
        handField = GetComponent<HandField>();
        if (handField == null)
        {
            Debug.LogError("HandField 컴포넌트를 찾을 수 없습니다. HandFieldTest 스크립트를 붙인 오브젝트에 HandField 컴포넌트를 추가하세요.");
        }
    }

    private void OnGUI()
    {
        // AddTile 버튼: white tile (GameTile.Z5) 추가
        if (GUI.Button(new Rect(10, 10, 150, 30), "Add Tile"))
        {
            handField.AddTile();
        }

        // SetTsumoTile 버튼: tsumotile 설정
        if (GUI.Button(new Rect(10, 50, 150, 30), "Set Tsumo Tile"))
        {
            handField.SetTsumoTile();
        }

        // Discard 버튼: 랜덤 인덱스의 일반 타일 제거
        if (GUI.Button(new Rect(10, 90, 150, 30), "Discard Random Tile"))
        {
            if (handField.handTiles.Count > 0)
            {
                int randomIndex = Random.Range(0, handField.handTiles.Count);
                handField.Discard(randomIndex, false);
            }
            else
            {
                Debug.LogWarning("제거할 타일이 없습니다.");
            }
        }

        // Discard Tsumo 버튼: tsumotile 제거
        if (GUI.Button(new Rect(10, 130, 150, 30), "Discard Tsumo Tile"))
        {
            handField.Discard(0, true);
        }
    }
}
