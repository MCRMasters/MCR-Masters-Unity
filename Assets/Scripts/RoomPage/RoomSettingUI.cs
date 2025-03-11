using UnityEngine;
using UnityEngine.UI;

public class RoomSettingUI : MonoBehaviour
{
    // RoomSetting panel (contains the input fields)
    public GameObject roomSettingPanel;

    // Two InputFields (for room title and room description)
    public InputField inputFieldTitle;
    public InputField inputFieldInfo;

    // Reference to NewRoomManager (handles room creation)
    public NewRoomManager newRoomManager;

    // Called by the "MakeRoom" button inside the RoomSetting panel
    public void OnClickMakeRoom()
    {
        // 1️⃣ Get text from the InputFields
        string title = inputFieldTitle.text;
        string info = inputFieldInfo.text;

        // 2️⃣ Request NewRoomManager to create a room using the input data
        newRoomManager.CreateRoomFromInput();

        // 3️⃣ Close the RoomSetting panel
        roomSettingPanel.SetActive(false);

        // 4️⃣ Clear the InputFields (optional)
        inputFieldTitle.text = "";
        inputFieldInfo.text = "";
    }
}
