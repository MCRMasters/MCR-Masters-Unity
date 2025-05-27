using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MCRGame.Net;

namespace MCRGame.UI
{
    public class RoomListUIController : MonoBehaviour
    {
        [SerializeField] private Button createRoomButton;
        
        [SerializeField] private Button RefreshRoomListButton;
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject roomItemPrefab;

        private Coroutine autoRefreshCoroutine;

        private void OnEnable()
        {
            RoomService.Instance.OnRoomListReceived += PopulateRoomList;
            createRoomButton.onClick.AddListener(OnCreateRoom);
            RefreshRoomListButton.onClick.AddListener(OnRefresh);

            // 즉시 한 번 호출
            RoomService.Instance.FetchRooms();
            // 자동 갱신 시작
            StartAutoRefresh();
        }

        private void OnRefresh()
        {
            RoomService.Instance.FetchRooms();
        }

        private void OnDisable()
        {
            RoomService.Instance.OnRoomListReceived -= PopulateRoomList;
            createRoomButton.onClick.RemoveListener(OnCreateRoom);

            // 자동 갱신 중지
            StopAutoRefresh();
        }

        private void PopulateRoomList(List<RoomInfo> rooms)
        {
            foreach (Transform t in contentParent)
                Destroy(t.gameObject);

            foreach (var r in rooms)
            {
                var go = Instantiate(roomItemPrefab, contentParent);
                var item = go.GetComponent<RoomItemUI>();
                item.Setup(r);
            }
        }

        private void OnCreateRoom()
        {
            RoomService.Instance.CreateRoom();
            SceneManager.LoadScene("RoomScene", LoadSceneMode.Single);
        }

        private void StartAutoRefresh()
        {
            if (autoRefreshCoroutine == null)
                autoRefreshCoroutine = StartCoroutine(AutoRefreshCoroutine());
        }

        private void StopAutoRefresh()
        {
            if (autoRefreshCoroutine != null)
            {
                StopCoroutine(autoRefreshCoroutine);
                autoRefreshCoroutine = null;
            }
        }

        private IEnumerator AutoRefreshCoroutine()
        {
            var wait = new WaitForSeconds(5f);
            while (true)
            {
                RoomService.Instance.FetchRooms();
                yield return wait;
            }
        }
    }
}
