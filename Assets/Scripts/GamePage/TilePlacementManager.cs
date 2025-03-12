using UnityEngine;

public class TilePlacementManager : MonoBehaviour
{
    public GameObject mahjongTilePrefab;   // 마작 패 Prefab
    public Transform[] tilePositions;      // 13개의 Spawn Point (빈 GameObject)

    void Start()
    {
        // tilePositions 배열의 각 위치에 대해 마작 패를 Instantiate합니다.
        for (int i = 0; i < tilePositions.Length; i++)
        {
            GameObject tile = Instantiate(mahjongTilePrefab,
                                          tilePositions[i].position,
                                          tilePositions[i].rotation);
            // 추가적인 초기화 로직 (예: 패 정보 설정 등)을 여기에 추가할 수 있습니다.
        }
    }
}
