
/*
using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject tilePrefab; // 3D 타일 프리팹

    // Start는 게임 시작 시 한 번 실행되는 함수입니다.
    void Start()
    {
        // 게임이 시작될 때 (0, 0, 0) 위치에 타일을 스폰
        SpawnTile(Vector3.zero);
    }

    // 타일을 스폰하는 함수
    void SpawnTile(Vector3 position)
    {
        // 타일 프리팹을 해당 위치에 인스턴스화 (생성)
        Instantiate(tilePrefab, position, Quaternion.identity);
    }
}
*/


using UnityEngine;

public class TileSpawner : MonoBehaviour
{
    public GameObject TilePrefab;  // 3D Tile Prefab
    public Transform spawnLocation; // Tile을 생성할 위치
    public Canvas worldCanvas;  // World Space로 설정된 Canvas

    // Start is called before the first frame update
    void Start()
    {
        SpawnTile();
    }

    // Tile을 생성하는 함수
    public void SpawnTile()
    {
        if (TilePrefab != null && worldCanvas != null)
        {
            // 3D 오브젝트를 World Space Canvas 안에 생성
            GameObject spawnedTile = Instantiate(TilePrefab, spawnLocation.position, Quaternion.identity);

            // TilePrefab의 크기를 조정하여 Canvas에 맞게 위치
            RectTransform rectTransform = spawnedTile.GetComponent<RectTransform>();
            rectTransform.SetParent(worldCanvas.transform, false);  // World Space Canvas 안에 넣기

            // 오브젝트 크기 및 위치 조정 (필요한 경우)
            rectTransform.sizeDelta = new Vector2(200, 200);  // 예시로 크기 설정
        }
    }
}
