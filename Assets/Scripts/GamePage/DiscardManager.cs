using UnityEngine;
using System.Collections.Generic;

public enum PlayerSeat { E, S, W, N }

public class DiscardManager : MonoBehaviour
{
    public Transform discardPosE;
    public Transform discardPosS;
    public Transform discardPosW;
    public Transform discardPosN;

    [Header("타일 간격 설정")]
    public float tileSpacing = 15f;  // 같은 행 내 타일 간격
    public float rowSpacing = 20f;   // 6개마다 다음 행으로 이동하는 간격
    public int maxTilesPerRow = 6;

    private Dictionary<PlayerSeat, int> discardCounts = new Dictionary<PlayerSeat, int>();

    void Awake()
    {
        discardCounts[PlayerSeat.E] = 0;
        discardCounts[PlayerSeat.S] = 0;
        discardCounts[PlayerSeat.W] = 0;
        discardCounts[PlayerSeat.N] = 0;
    }

    public void DiscardTile(PlayerSeat seat, TileData tileData)
    {
        Transform origin = GetDiscardPosition(seat);
        int index = discardCounts[seat];
        int row = index / maxTilesPerRow;
        int col = index % maxTilesPerRow;

        Vector3 offset;
        if (seat == PlayerSeat.S)
        {
            offset = Vector3.forward * (col * tileSpacing)
                   + Vector3.right * (row * rowSpacing);
        }
        else if (seat == PlayerSeat.N)
        {
            offset = Vector3.left * (row * rowSpacing)
                   + Vector3.back * (col * tileSpacing);
        }
        else if (seat == PlayerSeat.W)
        {
            offset = Vector3.left * (col * tileSpacing)
                   + Vector3.forward * (row * rowSpacing);
        }
        else // PlayerSeat.E
        {
            offset = Vector3.right * (col * tileSpacing)
                   + Vector3.back * (row * rowSpacing);
        }

        Vector3 newPos = origin.position + offset;
        Quaternion finalRotation = origin.rotation;

        GameObject prefab3D = TileLoader.Instance.Get3DPrefab(tileData.suit, tileData.value);
        if (prefab3D == null)
        {
            Debug.LogWarning($"3D prefab not found: {tileData.suit}{tileData.value}");
            return;
        }

        Instantiate(prefab3D, newPos, finalRotation);
        discardCounts[seat]++;
    }

    private Transform GetDiscardPosition(PlayerSeat seat) => seat switch
    {
        PlayerSeat.E => discardPosE,
        PlayerSeat.S => discardPosS,
        PlayerSeat.W => discardPosW,
        PlayerSeat.N => discardPosN,
        _ => null
    };
}
