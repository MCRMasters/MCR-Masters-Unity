using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MCRGame.Net; // RoomApiManager와 RoomDataManager 포함
using MCRGame.UI;

public class RoomCreationController : MonoBehaviour
{
    public RoomApiManager roomApiManager;
    public RoomSceneTransition sceneTransition;
    public Button makeRoomButton;

    private void Start()
    {
        if (makeRoomButton != null)
        {
            makeRoomButton.onClick.AddListener(OnClickMakeRoom);
        }
    }

    private void OnClickMakeRoom()
    {
        if (roomApiManager == null || sceneTransition == null)
        {
            Debug.LogError("RoomApiManager 또는 RoomSceneTransition이 할당되어 있지 않습니다.");
            return;
        }

        StartCoroutine(roomApiManager.CreateRoom(
            onSuccess: (CreateRoomResponse response) =>
            {
                Debug.Log("[RoomCreationController] Room created successfully. Room Number: " + response.room_number);
                // 현재 사용자가 방 생성 시 호스트이므로, PlayerDataManager.Instance.Uid 대신 Nickname을 사용하거나,
                // 필요하다면 Uid로 사용하고, 서버에서 host nickname을 반환하도록 할 수도 있습니다.
                string hostNickname = PlayerDataManager.Instance != null ? PlayerDataManager.Instance.Nickname : "";
                sceneTransition.TransitionToRoomScene(response.room_number.ToString(), response.name, hostNickname);
            },
            onError: (string error) =>
            {
                Debug.LogError("[RoomCreationController] Room creation failed: " + error);
            }
        ));
    }
}
