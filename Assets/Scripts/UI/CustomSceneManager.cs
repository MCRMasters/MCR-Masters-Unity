using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager.LoadScene 사용
using System.Collections.Generic;
using UnityEngine.UI;

namespace MCRGame.UI
{
    public class CustomSceneManager : MonoBehaviour
    {
        [Header("씬 전환용 버튼 설정들")]
        [SerializeField]
        private SceneButtonData[] sceneButtons;

        private void Start()
        {
            // 배열에 등록된 각 버튼에 이벤트를 연결
            foreach (var data in sceneButtons)
            {
                if (data.button != null && !string.IsNullOrEmpty(data.sceneName))
                {
                    data.button.onClick.AddListener(() => OnSceneButtonClicked(data.sceneName));
                }
            }
        }

        /// <summary>
        /// 버튼이 클릭되었을 때, 해당 씬으로 이동
        /// </summary>
        private void OnSceneButtonClicked(string sceneName)
        {
            Debug.Log($"씬 '{sceneName}'로 전환합니다.");
            SceneManager.LoadScene(sceneName);
        }


        /// <summary>
        /// 버튼과 이동할 씬 이름을 묶어서 관리하기 위한 데이터 클래스
        /// </summary>
        [System.Serializable]
        public class SceneButtonData
        {
            public Button button;       // Inspector에서 연결할 버튼
            public string sceneName;    // 이동할 씬 이름
        }

    }
}