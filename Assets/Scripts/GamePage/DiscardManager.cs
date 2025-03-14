using UnityEngine;
using System.Collections.Generic;

public enum PlayerSeat { E, S, W, N }

public class DiscardManager : MonoBehaviour
{
    public Transform discardPosE;
    public Transform discardPosS;
    public Transform discardPosW;
    public Transform discardPosN;

    [Header("3D 타일 매핑")]
    public Tile3DMapping[] tile3DMappings; // Inspector에 34개 이상 등록

    public float tileSpacingX = 25f; // 가로(열) 간격
    public float tileSpacingZ = 20f; // 세로(행) 간격 (Z축 이동)
    public int maxTilesPerRow = 6;

    private Dictionary<PlayerSeat, int> discardCounts = new Dictionary<PlayerSeat, int>();

    void Awake()
    {
        discardCounts[PlayerSeat.E] = 0;
        discardCounts[PlayerSeat.S] = 0;
        discardCounts[PlayerSeat.W] = 0;
        discardCounts[PlayerSeat.N] = 0;
    }

    // 타일 데이터에 따라 3D 버림패를 생성
    public void DiscardTile(PlayerSeat seat, TileData tileData)
    {
        Transform origin = GetDiscardPosition(seat);
        int index = discardCounts[seat];

        int row = index / maxTilesPerRow;
        int col = index % maxTilesPerRow;

        Vector3 offset = origin.right * (col * tileSpacingX)
                       + -origin.forward * (row * tileSpacingZ);
        Vector3 newPos = origin.position + offset;

        Quaternion finalRotation = origin.rotation;
        if (seat == PlayerSeat.E)
        {
            finalRotation = origin.rotation * Quaternion.Euler(-90f, 180f, 0f);
        }

        // 3D 프리팹 매핑을 통해 타일 데이터에 맞는 3D 프리팹 선택
        GameObject prefab3D = Get3DPrefab(tileData.suit, tileData.value);
        if (prefab3D == null)
        {
            Debug.LogWarning($"3D 프리팹 매핑 없음: {tileData.suit} {tileData.value}");
            return;
        }

        Instantiate(prefab3D, newPos, finalRotation);
        discardCounts[seat]++;
    }

    private GameObject Get3DPrefab(string suit, int value)
    {
        foreach (var mapping in tile3DMappings)
        {
            if (mapping.suit.ToLower() == suit.ToLower() && mapping.value == value)
            {
                return mapping.prefab3D;
            }
        }
        return null;
    }

    private Transform GetDiscardPosition(PlayerSeat seat)
    {
        switch (seat)
        {
            case PlayerSeat.E: return discardPosE;
            case PlayerSeat.S: return discardPosS;
            case PlayerSeat.W: return discardPosW;
            case PlayerSeat.N: return discardPosN;
            default: return null;
        }
    }
}
