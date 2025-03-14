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

    public float tileSpacingX = 25f;
    public float tileSpacingY = 25f;
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

        // 행/열 계산
        int row = index / maxTilesPerRow;
        int col = index % maxTilesPerRow;

        // 예: 오른쪽(col) & 위아래(row) 방향으로 오프셋
        Vector3 offset = discardOrigin.right * (col * tileSpacingX)
                         + -discardOrigin.up * (row * tileSpacingY);
        Vector3 newPos = discardOrigin.position + offset;

        // 타일 생성
        GameObject discardObj = Instantiate(discardTilePrefab, newPos, discardOrigin.rotation);
        // discardObj.GetComponent<DiscardTileController>()?.SetTileData(tileData);

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
