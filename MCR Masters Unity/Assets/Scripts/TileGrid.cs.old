using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;
using System.Collections;
using Game.Shared;

public class TileGrid : MonoBehaviour
{
    public int columns; // Default number of columns in the grid
    public float cellWidth; // Default width of each cell
    public float cellHeight; // Default height of each cell
    public float spacingX; // Default horizontal spacing between cells
    public float spacingY; // Default vertical spacing between cells
    public float additionalSpacingX; // Additional spacing for ShowTsumoTile

    public GameObject playerManager;
    private GameObject LastTsumoTileObject; // Reference to the last added Tsumo tile
    private List<GameObject> indexToChild;
    private bool isTsumoState;

    public void EmptyAll()
    {
        if (LastTsumoTileObject != null)
        {
            Destroy(LastTsumoTileObject);
            LastTsumoTileObject = null;
        }
        foreach (var item in indexToChild)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        indexToChild.Clear();
    }
    private void Awake()
    {
        Debug.Log("TileGrid component added to GameObject and Awake called.");
        // 초기화 작업
        LastTsumoTileObject = null;
        indexToChild = new List<GameObject>();


    }

    private void Start()
    {
        Debug.Log("TileGrid component's Start called.");
        // Start 단계에서 필요한 추가 작업 수행
        // Find all GameObjects with PlayerManager script
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        // Filter by the `isOwned` property
        foreach (var manager in allPlayerManagers)
        {
            if (manager.isOwned) // Assuming isOwned is a public property or field
            {
                playerManager = manager.gameObject;
                Debug.Log($"[TileGrid] PlayerManager found: {playerManager.name}");
                break;
            }
        }
    }

    private void ArrangeChildrenByIndexAndName()
    {
        List<GameObject> childList = new List<GameObject>();
        foreach (var child in indexToChild)
        {
            if (!!child)
            {
                childList.Add(child);
            }
        }
        if (!!LastTsumoTileObject)
        {
            Debug.Log($"LastTsumoTIleObject: {LastTsumoTileObject.name}");
            childList.Add(LastTsumoTileObject);
            LastTsumoTileObject = null;
        }
        childList = childList.Select(child => child.gameObject)
            .OrderBy(child =>
            {
                string namePart = child.name.Substring(0, 2);

                // "0f"는 항상 최후의 우선순위로 설정
                if (namePart == "0f") return (2, string.Empty);

                // Reverse 기준으로 정렬
                char[] reversed = namePart.ToCharArray();
                Array.Reverse(reversed);
                string reversedString = new string(reversed);

                return (1, reversedString); // 일반 정렬은 reverse된 문자열 사용
            })
            .ToList();
        for (int i = 0; i < childList.Count; i++)
        {
            GameObject child = childList[i];
            int row = i / columns;
            int column = i % columns;
            if (row >= 3)
            {
                column += (row - 2) * columns;
                row = 2;
            }

            // Adjust child's size (if it has a RectTransform)
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);

            }
            else
            {
                // If not RectTransform, adjust localScale
                child.transform.localScale = new Vector3(cellWidth / 100f, cellHeight / 100f, 1);

            }

            // Set child's position
            Vector3 position = new Vector3(
                column * (cellWidth + spacingX),
                -row * (cellHeight + spacingY),
                0
            );
            child.transform.localPosition = position;

            // Log each child's new position
            //Debug.Log($"Child: {child.name}, Index: {i}, Position: {position}");
        }
        indexToChild = childList;
        // indexToChild 내용을 한 줄로 출력
        string childDebugInfo = string.Join(", ", indexToChild.Select(child => child?.name ?? "null"));
        Debug.Log($"indexToChild contents: [{childDebugInfo}]");

        Debug.Log("Completed ArrangeChildrenByIndexAndName.");
    }

    private void ArrangeChildrenByIndex()
    {
        Debug.Log("Starting ArrangeChildrenByIndex.");

        List<GameObject> childList = new List<GameObject>();
        foreach (var child in indexToChild)
        {
            if (child != null && child != LastTsumoTileObject)
            {
                childList.Add(child);
            }
        }

        for (int i = 0; i < childList.Count; i++)
        {
            GameObject child = childList[i];
            int row = i / columns;
            int column = i % columns;
            if (row >= 3)
            {
                column += (row - 2) * columns;
                row = 2;
            }
            // Adjust child's size (if it has a RectTransform)
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
            }
            else
            {
                // If not RectTransform, adjust localScale
                child.transform.localScale = new Vector3(cellWidth / 100f, cellHeight / 100f, 1);

            }

            // Set child's position
            Vector3 position = new Vector3(
                column * (cellWidth + spacingX),
                -row * (cellHeight + spacingY),
                0
            );
            child.transform.localPosition = position;

            // Log each child's new position
            //Debug.Log($"Child: {child.name}, Index: {i}, Position: {position}");
        }
        indexToChild = childList;
        string childDebugInfo = string.Join(", ", indexToChild.Select(child => child?.name ?? "null"));
        Debug.Log($"indexToChild contents: [{childDebugInfo}]");
        if (LastTsumoTileObject != null)
        {
            ShowTsumoTile(LastTsumoTileObject);
        }

        Debug.Log("Completed ArrangeChildrenByIndex.");
    }

    private void ArrangeChildrenByName()
    {
        Debug.Log("Starting ArrangeChildrenByName.");

        var children = transform.Cast<Transform>()
            .Select(child => child.gameObject)
            .OrderBy(child =>
            {
                string namePart = child.name.Substring(0, 2);

                // "0f"는 항상 최후의 우선순위로 설정
                if (namePart == "0f") return (2, string.Empty);

                // Reverse 기준으로 정렬
                char[] reversed = namePart.ToCharArray();
                Array.Reverse(reversed);
                string reversedString = new string(reversed);

                return (1, reversedString); // 일반 정렬은 reverse된 문자열 사용
            })
            .ToList();


        indexToChild = new List<GameObject>(children);
        string childDebugInfo = string.Join(", ", indexToChild.Select(child => child?.name ?? "null"));
        Debug.Log($"indexToChild contents: [{childDebugInfo}]");

        ArrangeChildrenByIndex();
        Debug.Log("Completed ArrangeChildrenByName.");
    }

    public void DestoryByTileId(int tileId)
    {
        Debug.Log($"[DestoryByTileId] Called with tileId: {tileId}");

        if (!TileDictionary.NumToString.ContainsKey(tileId))
        {
            Debug.LogError($"[DestoryByTileId] Invalid tileId: {tileId}. Key not found in TileDictionary.NumToString.");
            return;
        }

        var tileName = TileDictionary.NumToString[tileId];
        Debug.Log($"[DestoryByTileId] Mapped tileId {tileId} to tileName: {tileName}");

        if (LastTsumoTileObject != null && LastTsumoTileObject.name.Substring(0, 2) == tileName)
        {
            Debug.Log($"[DestoryByTileId] Destroying LastTsumoTileObject: {LastTsumoTileObject.name}");
            Destroy(LastTsumoTileObject);
            LastTsumoTileObject = null;
        }
        else
        {
            Debug.Log($"[DestoryByTileId] Searching indexToChild for tileName: {tileName}");
            for (int i = 0; i < indexToChild.Count; i++)
            {
                if (indexToChild[i] == null)
                {
                    Debug.Log($"[DestoryByTileId] indexToChild[{i}] is null. Skipping.");
                    continue;
                }

                Debug.Log($"[DestoryByTileId] Checking indexToChild[{i}]: {indexToChild[i].name}");
                if (indexToChild[i].name.Substring(0, 2) == tileName)
                {
                    Debug.Log($"[DestoryByTileId] Match found. Destroying indexToChild[{i}]: {indexToChild[i].name}");
                    Destroy(indexToChild[i]);
                    indexToChild[i] = null;
                    break;
                }
            }
        }

        Debug.Log("[DestoryByTileId] Calling ArrangeChildrenByIndexAndName.");
        ArrangeChildrenByIndexAndName();
        Debug.Log("[DestoryByTileId] Completed.");
    }


    public void ShowTedashiByTileId(int tileId)
    {
        if (LastTsumoTileObject == null)
        {
            StartCoroutine(ShowTedashiCoroutine());
        }
        else
        {
            if(LastTsumoTileObject.name.Substring(0, 2) != TileDictionary.NumToString[tileId])
            {
                StartCoroutine(ShowTedashiCoroutine());
            }
            else
            {
                Destroy(LastTsumoTileObject);
                LastTsumoTileObject = null;
                isTsumoState = false;
            }
        }
    }

    public void ShowTedashi(bool IsTedashi)
    {
        if (!IsTedashi)
        {
            if (LastTsumoTileObject)
                Destroy(LastTsumoTileObject);
            LastTsumoTileObject = null;
            isTsumoState = false;
        }
        else
        {
            StartCoroutine(ShowTedashiCoroutine());
        }
    }

    HashSet<int> TedashiSet = new HashSet<int>();
    int TedashiId = 0;
    private IEnumerator ShowTedashiCoroutine()
    {
        // 1. Remove null values from indexToChild and update the list
        List<int> activeIndices = indexToChild
            .Select((child, index) => new { child, index })
            .Where(item => item.child != null && item.child.activeSelf)
            .Select(item => item.index)
            .ToList();

        TedashiId++;
        int localTedashiId = TedashiId;
        TedashiSet.Add(localTedashiId);
        if (activeIndices.Count == 0)
        {
            Debug.LogError("All objects in indexToChild are null.");
            yield break;
        }

        // 2. Pick random object from updated indexToChild
        int randomIndex = activeIndices[UnityEngine.Random.Range(0, activeIndices.Count)];
        // 3. Hide it
        indexToChild[randomIndex].SetActive(false);


        // 4. Wait 1 second
        yield return new WaitForSeconds(1f); //Destroy(randomObject ); 1초 기다리는동안 Destroy 당함
        if (indexToChild[randomIndex] == null)
        {
            yield break;
        }
        // 5. Show it
        indexToChild[randomIndex].SetActive(true);
        TedashiSet.Remove(localTedashiId);
        Debug.Log($"[ShowTedashiCoroutine] Tedashi Set Count: {TedashiSet.Count}");
        if (TedashiSet.Count == 0)
        {
            indexToChild = indexToChild.Where(obj => obj != null && obj.activeSelf == true).ToList();
            //indexToChild.Add(LastTsumoTileObject);
            if (isTsumoState)
            {
                DestoryLastTile();
            }
        }
        isTsumoState = false;
    }



    public void ShowTsumoTile(GameObject lastTsumoTileObject)
    {
        isTsumoState = true;
        if (lastTsumoTileObject == null)
        {
            //Debug.LogError("remove last tsumo tile.");
            //if (LastTsumoTileObject)
            //    Destroy(LastTsumoTileObject);
            //LastTsumoTileObject = null;
            return;
        }
        if (LastTsumoTileObject != null && lastTsumoTileObject != LastTsumoTileObject)
        {
            Destroy(LastTsumoTileObject);
            LastTsumoTileObject = null;
        }

        LastTsumoTileObject = lastTsumoTileObject;
        // Adjust child's size (if it has a RectTransform)
        RectTransform rectTransform = LastTsumoTileObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
        }
        else
        {
            // If not RectTransform, adjust localScale
            LastTsumoTileObject.transform.localScale = new Vector3(cellWidth / 100f, cellHeight / 100f, 1);
        }
        for (int i= indexToChild.Count - 1; i >= 0; --i)
        {
            if (indexToChild[i] != null)
            {

                Debug.Log($"[ShowTsumoTile] Last Tile index : {i+1}");

                Vector3 lastTilePosition = indexToChild[i].transform.localPosition;
                Vector3 position = new Vector3(
                    lastTilePosition.x + cellWidth + spacingX + additionalSpacingX,
                    lastTilePosition.y + spacingY,
                    lastTilePosition.z
                );
                lastTsumoTileObject.transform.localPosition = position;

                Debug.Log("A new Tsumo tile has been added with adjusted position.");
                return;
            }
        }

        Debug.LogError("No tile in hand, failed to add tsumo tile.");
    }


    public void DiscardTsumoTile()
    {
        DiscardSelectedTile(LastTsumoTileObject);
    }

    public void DiscardTileByTileId(int tileId)
    {
        if (tileId < 0 || tileId >= 35)
        {
            return;
        }
        if (LastTsumoTileObject != null && LastTsumoTileObject.name.Substring(0, 2) == TileDictionary.NumToString[tileId])
        {
            DiscardSelectedTile(LastTsumoTileObject);
            return;
        }
        for (int i=indexToChild.Count-1; i >= 0; i--)
        {
            if (indexToChild[i] != null)
            {
                if (indexToChild[i].name.Substring(0, 2) == TileDictionary.NumToString[tileId])
                {
                    DiscardSelectedTile(indexToChild[i]);
                    return;
                }
            }
        }
    }


    public void DestoryLastKawaTile()
    {
        if (indexToChild.Count > 0)
        {
            if (indexToChild[indexToChild.Count - 1] != null)
            {
                Destroy(indexToChild[indexToChild.Count - 1]);
                indexToChild.RemoveAt(indexToChild.Count - 1);
            }
        }
    }

    public void DestoryLastTile()
    {

        if (LastTsumoTileObject != null)
        {
            Destroy(LastTsumoTileObject);
            LastTsumoTileObject = null;
        }
        else
        {
            for (int i = indexToChild.Count - 1; i >= 0; --i)
            {
                if (indexToChild[i] != null)
                {
                    Destroy(indexToChild[i]);
                    indexToChild[i] = null;
                    break;
                }
            }
        }

        Debug.Log("[DestoryLastTile] Calling ArrangeChildrenByIndexAndName.");
        ArrangeChildrenByIndexAndName();
        Debug.Log("[DestoryLastTile] Completed.");
    }
    public void DiscardSelectedTile(GameObject tile)
    {
        if (tile == null)
        {
            Debug.LogError("Tile is not part of this grid or is invalid.");
            return;
        }

        var tileName = tile.name;
        bool isTsumoTile = false;

        if (tile == LastTsumoTileObject)
        {
            isTsumoTile = true;
            LastTsumoTileObject = null;
        }
        Debug.Log($"Attempting to remove tile: {tile.name}");
        indexToChild.Remove(tile);
        Debug.Log($"Tile {tile.name} removed from indexToChild list.");

        Debug.Log($"Attempting to destroy tile: {tile.name}");
        Destroy(tile);
        Debug.Log($"Tile {tile.name} destroyed.");

        Debug.Log("Rearranging children after discard.");
        ArrangeChildrenByIndexAndName();
        Debug.Log("Children rearranged after discard.");


        if (playerManager == null)
        {
            Debug.LogError("playerManager is null.");
            return;
        }

        PlayerManager pm = playerManager.GetComponent<PlayerManager>();
        if (pm == null)
        {
            Debug.LogError("PlayerManager component not found on playerManager.");
            return;
        }


        Debug.Log($"PlayerManager component found. PlayerIndex: {pm.PlayerIndex}, PlayerName: {pm.PlayerName}");

        Debug.Log($"playerManager GameObject name: {playerManager.name}");
        Debug.Log($"playerManager components: {string.Join(", ", playerManager.GetComponents<Component>().Select(c => c.GetType().Name))}");
        pm.CmdDiscardTile(tileName, isTsumoTile);
        pm.setRemainingTimeZero();
        pm.DeleteButtons();

        Debug.Log($"The selected tile {tileName} has been discarded and the grid rearranged.");

    }


    public void AddTileToLastIndex(GameObject tile)
    {
        if (tile == null)
        {
            Debug.LogError("Cannot add a null tile to the grid.");
            return;
        }

        indexToChild.Add(tile);
        ArrangeChildrenByIndex();

        Debug.Log($"Tile {tile.name} added to the grid and rearranged.");
    }


    public void SetChildIndex(GameObject child, int index)
    {
        if (child == null || !indexToChild.Contains(child))
        {
            Debug.LogError("Child is not part of this grid.");
            return;
        }

        ArrangeChildrenByIndex();
        Debug.Log($"Index for child {child.name} set to {index}.");
    }

    public void UpdateLayoutByIndex()
    {
        ArrangeChildrenByIndex();
    }

    public void UpdateLayoutByName()
    {
        ArrangeChildrenByName();
    }
}
