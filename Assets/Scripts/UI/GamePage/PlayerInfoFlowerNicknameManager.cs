using UnityEngine;
using UnityEngine.UI;


namespace MCRGame.UI
{
    /// <summary>
    /// 4명(동, 남, 서, 북) 플레이어의 
    /// - 프로필 이미지 (Profile Image)
    /// - 닉네임 (Nickname)
    /// - 화패 수 (Flower Tile Count)
    /// 를 목업 데이터로 표시하는 스크립트.
    ///
    /// 실제 서버 연동 전 테스트용.
    /// </summary>
    public class PlayerInfoFlowerNicknameManager : MonoBehaviour
    {
        [Header("동(E) 플레이어 UI")]
        [SerializeField] private Image profileImageE;
        [SerializeField] private Text nicknameE;
        [SerializeField] private Text flowerTileCountE;

        [Header("남(S) 플레이어 UI")]
        [SerializeField] private Image profileImageS;
        [SerializeField] private Text nicknameS;
        [SerializeField] private Text flowerTileCountS;

        [Header("서(W) 플레이어 UI")]
        [SerializeField] private Image profileImageW;
        [SerializeField] private Text nicknameW;
        [SerializeField] private Text flowerTileCountW;

        [Header("북(N) 플레이어 UI")]
        [SerializeField] private Image profileImageN;
        [SerializeField] private Text nicknameN;
        [SerializeField] private Text flowerTileCountN;

        [Header("목업 데이터")]
        [Tooltip("프로필에 사용할 수 있는 이미지들 (7개 정도)")]
        [SerializeField] private Sprite[] possibleProfileImages;

        [Tooltip("닉네임으로 사용할 과일 이름들 (6개 정도)")]
        [SerializeField]
        private string[] fruitNames =
            { "Apple", "Banana", "Cherry", "Grape", "Mango", "Peach" };

        private void Start()
        {
            // 실제 서버가 없으므로, 목업 데이터로 4명 UI를 채움
            SetPlayerE();
            SetPlayerS();
            SetPlayerW();
            SetPlayerN();
        }

        /// <summary>
        /// 동(E) 플레이어
        /// </summary>
        private void SetPlayerE()
        {
            if (profileImageE != null)
                profileImageE.sprite = GetRandomProfileImage();

            if (nicknameE != null)
                nicknameE.text = GetRandomFruitName();

            if (flowerTileCountE != null)
                flowerTileCountE.text = Random.Range(0, 9).ToString();
        }

        /// <summary>
        /// 남(S) 플레이어
        /// </summary>
        private void SetPlayerS()
        {
            if (profileImageS != null)
                profileImageS.sprite = GetRandomProfileImage();

            if (nicknameS != null)
                nicknameS.text = GetRandomFruitName();

            if (flowerTileCountS != null)
                flowerTileCountS.text = Random.Range(0, 9).ToString();
        }

        /// <summary>
        /// 서(W) 플레이어
        /// </summary>
        private void SetPlayerW()
        {
            if (profileImageW != null)
                profileImageW.sprite = GetRandomProfileImage();

            if (nicknameW != null)
                nicknameW.text = GetRandomFruitName();

            if (flowerTileCountW != null)
                flowerTileCountW.text = Random.Range(0, 9).ToString();
        }

        /// <summary>
        /// 북(N) 플레이어
        /// </summary>
        private void SetPlayerN()
        {
            if (profileImageN != null)
                profileImageN.sprite = GetRandomProfileImage();

            if (nicknameN != null)
                nicknameN.text = GetRandomFruitName();

            if (flowerTileCountN != null)
                flowerTileCountN.text = Random.Range(0, 9).ToString();
        }

        /// <summary>
        /// 등록된 프로필 이미지 중 랜덤 1개 반환
        /// </summary>
        private Sprite GetRandomProfileImage()
        {
            if (possibleProfileImages == null || possibleProfileImages.Length == 0)
            {
                Debug.LogWarning("possibleProfileImages가 비어있습니다.");
                return null;
            }
            int randIndex = Random.Range(0, possibleProfileImages.Length);
            return possibleProfileImages[randIndex];
        }

        /// <summary>
        /// 등록된 과일 이름 중 랜덤 1개 반환
        /// </summary>
        private string GetRandomFruitName()
        {
            if (fruitNames == null || fruitNames.Length == 0)
            {
                Debug.LogWarning("fruitNames가 비어있습니다.");
                return "NoName";
            }
            int randIndex = Random.Range(0, fruitNames.Length);
            return fruitNames[randIndex];
        }
    }
}