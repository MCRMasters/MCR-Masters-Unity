using UnityEngine;
using System.Collections.Generic;

public enum PlayerSeat { E, S, W, N }

public class DiscardManager : MonoBehaviour
{
    public Transform discardPosE;
    public Transform discardPosS;
    public Transform discardPosW;
    public Transform discardPosN;

    [Header("EW (E/W) 타일 간격 (X, Z)")]
    public float tileSpacingX_EW = 25f;
    public float tileSpacingZ_EW = 20f;

    [Header("SN (S/N) 타일 간격 (X, Z)")]
    public float tileSpacingX_SN = 20f;
    public float tileSpacingZ_SN = 25f;

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
            offset = origin.right * (row * tileSpacingX_SN) + origin.forward * (col * tileSpacingZ_SN);
        else if (seat == PlayerSeat.N)
            offset = -origin.right * (row * tileSpacingX_SN) + -origin.forward * (col * tileSpacingZ_SN);
        else if (seat == PlayerSeat.W)
            offset = -origin.right * (col * tileSpacingX_EW) + origin.forward * (row * tileSpacingZ_EW);
        else // E
            offset = origin.right * (col * tileSpacingX_EW) + -origin.forward * (row * tileSpacingZ_EW);

        Vector3 newPos = origin.position + offset;
        Quaternion finalRotation = origin.rotation;
        switch (seat)
        {
            case PlayerSeat.E: finalRotation *= Quaternion.Euler(-90f, 180f, 0f); break;
            case PlayerSeat.S: finalRotation *= Quaternion.Euler(-90f, 90f, 0f); break;
            case PlayerSeat.W: finalRotation *= Quaternion.Euler(-90f, 0f, 0f); break;
            case PlayerSeat.N: finalRotation *= Quaternion.Euler(-90f, 0f, -90f); break;
        }

        // TileLoader 사용
        GameObject prefab3D = TileLoader.Instance.Get3DPrefab(tileData.suit, tileData.value);
        if (prefab3D == null)
        {
            Debug.LogWarning($"3D 프리팹 없음: {tileData.suit}{tileData.value}");
            return;
        }

        Instantiate(prefab3D, newPos, finalRotation);
        discardCounts[seat]++;
    }

    private Transform GetDiscardPosition(PlayerSeat seat)
    {
        return seat switch
        {
            PlayerSeat.E => discardPosE,
            PlayerSeat.S => discardPosS,
            PlayerSeat.W => discardPosW,
            PlayerSeat.N => discardPosN,
            _ => null
        };
    }
}
