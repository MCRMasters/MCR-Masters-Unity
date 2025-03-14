using UnityEngine;
using System.Collections.Generic;

public enum PlayerSeat { E, S, W, N }

[System.Serializable]
public class TileData
{
    public string suit;
    public int value;
}

public class DiscardManager : MonoBehaviour
{
    public Transform discardPosE;
    public Transform discardPosS;
    public Transform discardPosW;
    public Transform discardPosN;

    public GameObject discardTilePrefab;

    public float tileSpacingX = 25f; // 가로(열) 간격
    public float tileSpacingZ = 20f; // 세로(행) 간격 (Z축 이동)
    public int maxTilesPerRow = 6;

    private Dictionary<PlayerSeat, int> discardCounts = new Dictionary<PlayerSeat, int>();

    private void Awake()
    {
        discardCounts[PlayerSeat.E] = 0;
        discardCounts[PlayerSeat.S] = 0;
        discardCounts[PlayerSeat.W] = 0;
        discardCounts[PlayerSeat.N] = 0;
    }

    public void DiscardTile(PlayerSeat seat, TileData tileData)
    {
        Transform discardOrigin = GetDiscardPosition(seat);
        int index = discardCounts[seat];

        int row = index / maxTilesPerRow;
        int col = index % maxTilesPerRow;

        Vector3 offset = discardOrigin.right * (col * tileSpacingX)
                         + -discardOrigin.forward * (row * tileSpacingZ);
        Vector3 newPos = discardOrigin.position + offset;

        // 기본적으로 discardOrigin.rotation 사용
        Quaternion finalRotation = discardOrigin.rotation;

        // E 플레이어인 경우, x:-90, y:+180 적용
        if (seat == PlayerSeat.E)
        {
            // 기존 회전에 추가 회전
            finalRotation = discardOrigin.rotation * Quaternion.Euler(-90f, 180f, 0f);
            // 만약 discardOrigin.rotation을 무시하고 완전히 고정 각도(-90, 180, 0)로 하고 싶다면
            // finalRotation = Quaternion.Euler(-90f, 180f, 0f);
        }

        // 타일 생성
        GameObject discardObj = Instantiate(discardTilePrefab, newPos, finalRotation);

        discardCounts[seat]++;
    }


    private Transform GetDiscardPosition(PlayerSeat seat)
    {
        switch (seat)
        {
            case PlayerSeat.E: return discardPosE;
            case PlayerSeat.S: return discardPosS;
            case PlayerSeat.W: return discardPosW;
            case PlayerSeat.N: return discardPosN;
        }
        return null;
    }
}
