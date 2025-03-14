
using UnityEngine;
using System.Collections.Generic;

public class HaipaiManager : MonoBehaviour
{

    /*앞으로 대체되어야할 replacementmanager 예시
    // 타일 Prefab들을 관리하는 딕셔너리(혹은 배열, ScriptableObject 등)
    // key: "character", "dot", "bamboo" 등
    public Dictionary<string, GameObject> tilePrefabMap = new Dictionary<string, GameObject>();

    // 또는 인스펙터에서 각 종류별로 연결:
    // public GameObject characterPrefab;
    // public GameObject dotPrefab;
    // public GameObject bambooPrefab;
    // ...

    // 타일 배치에 사용할 위치들 (N, E, S, W 등)
    public Transform tilePositionN;
    public Transform tilePositionE;
    public Transform tilePositionS;
    public Transform tilePositionW;

    // 간단한 예시용
    public float tileSpacing = 1.0f;
    public int tileCount = 13;

    void Start()
    {
        // Start()에서 서버로부터 타일 데이터를 받아온다고 가정.
        // 실제로는 코루틴, UnityWebRequest 등을 통해 서버 요청/응답을 처리합니다.
        StartCoroutine(LoadTilesFromServer());
    }

    // 서버 통신 (코루틴 예시)
    private System.Collections.IEnumerator LoadTilesFromServer()
    {
        // 1) 서버에 요청
        // 예) UnityWebRequest를 사용
        // 2) 응답이 오면 JSON을 역직렬화해서 TileData[]로 변환
        // 여기서는 예시로 직접 데이터 생성

        yield return new WaitForSeconds(1f); // 가짜 통신 대기

        // 서버에서 받아온 가짜 JSON을 만든다고 가정
        string fakeJson = @"
        [
            { ""tileId"": 1, ""type"": ""character"", ""value"": 2 },
            { ""tileId"": 2, ""type"": ""dot"",       ""value"": 9 },
            { ""tileId"": 3, ""type"": ""dot"",       ""value"": 3 },
            { ""tileId"": 4, ""type"": ""character"", ""value"": 5 },
            ...
        ]
        ";

        // 역직렬화
        TileData[] tiles = JsonHelper.FromJson<TileData>(fakeJson);

        // 타일을 배치
        PlaceTiles(tilePositionN, Vector3.back, tiles);
        // 필요에 따라 E, S, W도 따로 호출
        // PlaceTiles(tilePositionE, Vector3.right, someOtherTiles);
        // ...
    }

    // 실제로 타일을 생성하는 함수
    private void PlaceTiles(Transform origin, Vector3 direction, TileData[] tileDatas)
    {
        for (int i = 0; i < tileDatas.Length && i < tileCount; i++)
        {
            // 서버에서 받은 정보에 따라 Prefab을 선택
            GameObject prefabToSpawn = GetTilePrefab(tileDatas[i].type);

            if (prefabToSpawn == null)
            {
                Debug.LogWarning($"타일 타입 {tileDatas[i].type}에 해당하는 프리팹이 없습니다.");
                continue;
            }

            // 위치 계산
            Vector3 newPos = origin.position + direction * tileSpacing * i;
            // 회전은 필요에 따라 origin.rotation 등
            Quaternion newRot = origin.rotation;

            // 생성
            GameObject tile = Instantiate(prefabToSpawn, newPos, newRot);

            // 생성된 타일에 서버 데이터(TileData)를 전달하거나, 
            // TileController 같은 컴포넌트를 붙여서 값 세팅
            // ex) tile.GetComponent<TileController>().Initialize(tileDatas[i]);
        }
    }

    private GameObject GetTilePrefab(string tileType)
    {
        // tilePrefabMap 딕셔너리에서 type 키로 Prefab을 가져오거나,
        // switch-case를 써서 prefab을 분기 처리할 수도 있음
        if (tilePrefabMap.TryGetValue(tileType, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }
}

// JsonHelper는 배열 JSON 역직렬화용으로 종종 사용되는 예시 코드
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    */
}

