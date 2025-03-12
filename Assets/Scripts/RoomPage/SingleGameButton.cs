using UnityEngine;
using UnityEngine.UI;

public class SingleGameButton : MonoBehaviour
{
    public RoomManager roomManager;

    // 플레이어 인덱스 (0=호스트, 1~3=게스트)
    public int currentPlayerIndex;

    // 버튼에 표시할 Image
    public Image buttonImage;

    // 준비된 Sprite들
    public Sprite startSprite;      // 호스트용 "Start" 이미지
    public Sprite readySprite;      // 게스트용 "Ready?" 이미지
    public Sprite readyCheckedSprite; // 게스트용 "Ready ✔" 이미지

    void Start()
    {
        UpdateButtonImage();
    }

    public void OnClickButton()
    {
        if (currentPlayerIndex == 0)
        {
            // 호스트 → Start 로직
            roomManager.OnHostStartGame();
        }
        else
        {
            // 게스트 → Ready 토글
            roomManager.OnGuestToggleReady(currentPlayerIndex);
        }

        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        if (buttonImage == null || roomManager == null) return;

        if (currentPlayerIndex == 0)
        {
            // 호스트 → Start 이미지
            buttonImage.sprite = startSprite;
        }
        else
        {
            // 게스트
            int arrIndex = currentPlayerIndex - 1; // 1->0, 2->1, 3->2
            bool isReady = roomManager.guestReady[arrIndex];
            // Ready? or Ready ✔ 이미지
            buttonImage.sprite = isReady ? readyCheckedSprite : readySprite;
        }
    }
}
