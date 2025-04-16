using UnityEngine;
using MCRGame.UI;

namespace MCRGame.Tester
{
    public class DrawTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject drawImageObject;

        public void OnDrawButtonClicked()
        {
            drawImageObject.SetActive(true); // 반드시 먼저 켜줘야 Update가 작동함
            drawImageObject.GetComponent<DrawEffect>().PlayDrawEffect();
        }
    }
}