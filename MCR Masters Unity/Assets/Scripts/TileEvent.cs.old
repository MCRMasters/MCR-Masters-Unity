using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Shared;
using System;

public class TileEvent : MonoBehaviour
{
    public GameObject Canvas;
    public GameObject PlayerHaipai;
    public GameObject playerManager;
    PlayerManager pm;

    private bool isDragging = false;
    private bool isDraggable = true;
    private bool isHoveringEnd = false;
    private GameObject startParent;
    private Vector2 defaultOffset;
    private Vector2 offset; // 마우스 클릭 위치와 중심점의 차이
    private Vector3 startPosition;
    private int siblingIndex = -1;

    private float discardThresholdY;

    RectTransform rectTransform;
    Canvas canvas;
    float hoverOffset;

    public void Awake()
    {
        Debug.Log("Awake: Initializing TileEvent.");
        hoverOffset = 10 * Screen.width / 1920f;

        Canvas = GameObject.Find("Main Canvas");
        PlayerHaipai = GameObject.Find("PlayerHaipai");

        if (Canvas == null) Debug.LogError("Canvas not found in scene.");
        if (PlayerHaipai == null) Debug.LogError("PlayerHaipai not found in scene.");
        pm = null;

        // Find all GameObjects with PlayerManager script
        PlayerManager[] allPlayerManagers = UnityEngine.Object.FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);

        // Filter by the `isOwned` property
        foreach (var manager in allPlayerManagers)
        {
            if (manager.isOwned) // Assuming isOwned is a public property or field
            {
                playerManager = manager.gameObject;
                pm = playerManager.GetComponent<PlayerManager>();
                Debug.Log($"PlayerManager found: {playerManager.name}");
                break;
            }
        }

        if (playerManager == null)
        {
            Debug.LogError("No owned PlayerManager found in the scene.");
        }

        discardThresholdY = Screen.height * 0.25f;
        Debug.Log($"Discard threshold set to {discardThresholdY} (25% of screen height).");

        AddEventListeners();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GameObject.Find("Main Canvas").GetComponent<Canvas>();
    }

    private void AddEventListeners()
    {
        Debug.Log("Adding event listeners.");
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        // Existing event listeners
        EventTrigger.Entry dragStartEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        dragStartEntry.callback.AddListener((data) => { StartDrag(); });
        trigger.triggers.Add(dragStartEntry);

        EventTrigger.Entry dragEndEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        dragEndEntry.callback.AddListener((data) => { EndDrag(); });
        trigger.triggers.Add(dragEndEntry);

        EventTrigger.Entry hoverEnterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        hoverEnterEntry.callback.AddListener((data) => { OnHoverEnter(); });
        trigger.triggers.Add(hoverEnterEntry);

        EventTrigger.Entry hoverExitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        hoverExitEntry.callback.AddListener((data) => { OnHoverExit(); });
        trigger.triggers.Add(hoverExitEntry);

        // New click event listener
        EventTrigger.Entry clickEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        clickEntry.callback.AddListener((data) => { OnTileClick(); });
        trigger.triggers.Add(clickEntry);
    }

    // New click handler
    private void OnTileClick()
    {
        if (!isDraggable || isDragging) return;
        if (!pm)
            pm = playerManager.GetComponent<PlayerManager>();
        if (pm == null || !pm.CheckIfPlayerTurn()) return;
        startParent = transform.parent.gameObject;
        siblingIndex = transform.GetSiblingIndex();
        startPosition = transform.position;
        // 비동기적으로 HandleTileDiscard 호출
        Wind roundWind = pm.playerStatus.RoundWind;
        Wind seatWind = pm.playerStatus.SeatWind;
        StartCoroutine(HandleTileDiscardCoroutine((isSuccess) =>
        {
            if (!(roundWind == pm.playerStatus.RoundWind && seatWind == pm.playerStatus.SeatWind))
            {
                return;
            }
            if (isSuccess)
            {
                isDraggable = false;
                Debug.Log($"OnTileClick: Tile {gameObject.name} clicked. Discard successful.");
                startParent = transform.parent.gameObject;
            }
            else
            {
                Debug.Log($"OnTileClick: Tile {gameObject.name} clicked, but discard failed.");
                ResetPosition();
            }
        }));
    }

    public void StartDrag()
    {
        if (!isDraggable) return;

        if (!pm)
            pm = playerManager.GetComponent<PlayerManager>();

        if (pm == null || !pm.CheckIfPlayerTurn()) return;

        Debug.Log($"StartDrag: Dragging started for {gameObject.name}");

        isDragging = true;
        startParent = transform.parent.gameObject;
        siblingIndex = transform.GetSiblingIndex();
        startPosition = transform.position;

        // 마우스 위치를 부모의 로컬 좌표계로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,  // 부모의 RectTransform 기준으로 변환
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 localMousePosition
        );
        // 오브젝트의 width, height 구하기
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        // 오프셋 계산 (width만큼 오른쪽, height/2만큼 위쪽으로 조정)
        offset = (Vector2)rectTransform.localPosition - localMousePosition
                 + new Vector2(0, hoverOffset);

        Debug.Log($"StartDrag: Calculated offset = {offset}");
    }


    public void EndDrag()
    {
        if (!isDraggable)
        {
            isDragging = false;
            return;
        }
        if (!pm)
            pm = playerManager.GetComponent<PlayerManager>();
        if (pm == null || !pm.CheckIfPlayerTurn())
        {
            isDragging = false;
            return;
        }

        Debug.Log($"EndDrag: Dragging ended for {gameObject.name}");

        if (transform.position.y >= discardThresholdY)
        {
            Debug.Log($"Tile {gameObject.name} dropped above discard threshold. Attempting to discard...");

            // 비동기적으로 HandleTileDiscard 호출
            Wind roundWind = pm.playerStatus.RoundWind;
            Wind seatWind = pm.playerStatus.SeatWind;
            StartCoroutine(HandleTileDiscardCoroutine((isSuccess) =>
            {
                if (!(roundWind == pm.playerStatus.RoundWind && seatWind == pm.playerStatus.SeatWind))
                {
                    Destroy(gameObject);
                    return;
                }
                if (isSuccess)
                {
                    isDraggable = false;
                    Debug.Log($"OnTileClick: Tile {gameObject.name} clicked. Discard successful.");
                    startParent = transform.parent.gameObject;
                }
                else
                {
                    Debug.Log($"OnTileClick: Tile {gameObject.name} clicked, but discard failed.");
                    ResetPosition();
                }
            }));
        }
        else
        {
            Debug.Log($"Tile {gameObject.name} dropped below discard threshold. Resetting position.");
            ResetPosition();
        }

        isDragging = false;
    }


    private void ResetPosition()
    {
        Debug.Log($"ResetPosition: Resetting {gameObject.name} to original position.");
        //transform.SetParent(Canvas.transform, true);
        transform.SetParent(startParent.transform, false);
        transform.SetSiblingIndex(siblingIndex);
        transform.position = startPosition;
        TileGrid tileGrid = startParent.GetComponent<TileGrid>();
        tileGrid.UpdateLayoutByIndex();
    }

    public void OnHoverEnter()
    {
        Debug.Log($"[OnHoverEnter] isDraggable: {isDraggable}, isDragging: {isDragging}");
        Debug.Log($"[OnHoverEnter] PlayerIndex: {pm.PlayerIndex}, IsPlayerTurn: {pm.playerStatus.IsPlayerTurn}");
        if (!isDraggable || isDragging) return;
        //if (!pm)
        //    pm = playerManager.GetComponent<PlayerManager>();
        //if (pm == null || !pm.CheckIfPlayerTurn()) return;

        Debug.Log($"OnHoverEnter: Mouse entered over {gameObject.name}");
        startParent = transform.parent.gameObject;
        siblingIndex = transform.GetSiblingIndex();
        startPosition = transform.position;
        transform.position = new Vector3(startPosition.x, startPosition.y + hoverOffset, startPosition.z);
    }

    public void OnHoverExit()
    {
        if (!isDraggable || isHoveringEnd || isDragging) return;
        //if (!pm)
        //    pm = playerManager.GetComponent<PlayerManager>();
        //if (pm == null || !pm.CheckIfPlayerTurn()) return;

        Debug.Log($"OnHoverExit: Mouse exited from {gameObject.name}");
        ResetPosition();
    }

    public void SetUndraggable()
    {
        isDraggable = false;
    }



    private IEnumerator HandleTileDiscardCoroutine(Action<bool> callback)
    {
        Debug.Log($"HandleTileDiscard: Handling discard logic for {gameObject.name}");

        if (!pm)
            pm = playerManager.GetComponent<PlayerManager>();
        if (pm == null)
        {
            Debug.LogError("HandleTileDiscard: PlayerManager is null.");
            callback?.Invoke(false);
            yield break;
        }

        if (!TileDictionary.StringToNum.TryGetValue(gameObject.name.Substring(0, 2), out int tileId))
        {
            Debug.LogError("HandleTileDiscard: Failed to get tile ID.");
            callback?.Invoke(false);
            yield break;
        }

        // 비동기적으로 유효성 검사 실행
        bool isValid = false;
        yield return StartCoroutine(pm.CheckVaildDiscardAsync(tileId, result =>
        {
            isValid = result;
        }));

        if (!isValid)
        {
            Debug.Log($"HandleTileDiscard: Invalid discard for {TileDictionary.NumToString[tileId]}");
            callback?.Invoke(false);
            yield break;
        }

        // Get the parent GameObject
        GameObject parentObject = startParent;
        if (parentObject == null)
        {
            Debug.LogError("HandleTileDiscard: Parent object not found.");
            callback?.Invoke(false);
            yield break;
        }

        // Get the TileGrid component from the parent
        TileGrid tileGrid = parentObject.GetComponent<TileGrid>();
        if (tileGrid == null)
        {
            Debug.LogError("HandleTileDiscard: TileGrid component not found on parent object.");
            callback?.Invoke(false);
            yield break;
        }

        tileGrid.DiscardSelectedTile(gameObject);
        Debug.Log($"HandleTileDiscard: DiscardSelectedTile called on parent TileGrid for {gameObject.name}");
        callback?.Invoke(true);
    }



    void Update()
    {
        if (isDragging)
        {
            // 마우스 위치를 부모의 로컬 좌표계로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );

            // 오프셋을 반영한 위치로 오브젝트 이동
            rectTransform.localPosition = localPoint + offset;
        }
    }
}
