using UnityEngine;
using System.Collections.Generic;

public enum PlayerSeat { E, S, W, N }

public class DiscardManager : MonoBehaviour
{
    public Transform discardPosE;
    public Transform discardPosS;
    public Transform discardPosW;
    public Transform discardPosN;

    [Header("3D 타일 매핑 (Inspector에 34개 이상 등록)")]
    public Tile3DMapping[] tile3DMappings;

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

    // 타일 데이터에 따라 3D 버림패를 생성
    public void DiscardTile(PlayerSeat seat, TileData tileData)
    {
        Transform origin = GetDiscardPosition(seat);
        int index = discardCounts[seat];

        int row = index / maxTilesPerRow;
        int col = index % maxTilesPerRow;

        Vector3 offset = Vector3.zero;
        // EW와 SN의 타일 간격을 각 세트로 적용
        if (seat == PlayerSeat.S)
        {
            // S: SN 간격, 6개마다 X(오른쪽) 증가, 각 열은 Z(앞쪽) 증가
            offset = origin.right * (row * tileSpacingX_SN)
                   + origin.forward * (col * tileSpacingZ_SN);
        }
        else if (seat == PlayerSeat.N)
        {
            // N: SN 간격, 6개마다 -X(왼쪽) 감소, 각 열은 -Z(뒤쪽) 감소
            offset = -origin.right * (row * tileSpacingX_SN)
                   + -origin.forward * (col * tileSpacingZ_SN);
        }
        else if (seat == PlayerSeat.W)
        {
            // W: EW 간격, 6개마다 X(오른쪽) 증가, 각 열은 +Z(앞쪽) 감소
            offset = -origin.right * (col * tileSpacingX_EW)
                   + origin.forward * (row * tileSpacingZ_EW);
        }
        else // PlayerSeat.E
        {
            // E: EW 간격, 6개마다 X(오른쪽) 증가, 각 열은 -Z(뒤쪽) 감소
            offset = origin.right * (col * tileSpacingX_EW)
                   + -origin.forward * (row * tileSpacingZ_EW);
        }
        Vector3 newPos = origin.position + offset;

        // 좌석별 회전값 적용
        Quaternion finalRotation = origin.rotation;
        switch (seat)
        {
            case PlayerSeat.E:
                finalRotation = origin.rotation * Quaternion.Euler(-90f, 180f, 0f);
                break;
            case PlayerSeat.S:
                finalRotation = origin.rotation * Quaternion.Euler(-90f, 90f, 0f);
                break;
            case PlayerSeat.W:
                finalRotation = origin.rotation * Quaternion.Euler(-90f, 0f, 0f);
                break;
            case PlayerSeat.N:
                finalRotation = origin.rotation * Quaternion.Euler(-90f, 0f, -90f);
                break;
            default:
                break;
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
