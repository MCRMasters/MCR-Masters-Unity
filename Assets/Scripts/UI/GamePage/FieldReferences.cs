// Assets/Scripts/UI/FieldReferences.cs
using UnityEngine;
using TMPro;

namespace MCRGame.UI
{
    /// <summary>
    /// 3DField Prefab 안의 주요 UI/Transform 참조들을
    /// Inspector 에서 할당해 두고, 런타임에 한 번에 바인딩하기 위한 스크립트.
    /// </summary>
    public class FieldReferences : MonoBehaviour
    {
        [Header("Wind Texts (SELF→SHIMO→TOI→KAMI)")]
        public TextMeshProUGUI[] WindTexts;        

        [Header("Turn Images (SELF→SHIMO→TOI→KAMI)")]
        public UnityEngine.UI.Image[] TurnImages;

        [Header("Score Texts (SELF→SHIMO→TOI→KAMI)")]
        public TextMeshProUGUI[] ScoreTexts;

        [Header("Round & Left-Tile Texts")]
        public TextMeshProUGUI RoundText;
        public TextMeshProUGUI LeftTileText;

        [Header("3D Hand Fields (SELF→SHIMO→TOI→KAMI)")]
        public Hand3DField[] Hand3DFields;

        [Header("Discard Positions (SELF→SHIMO→TOI→KAMI)")]
        public Transform[] DiscardPositions;

        [Header("CallBlock Origins (SELF→SHIMO→TOI→KAMI)")]
        public Transform[] CallBlockOrigins;
    }
}
