using UnityEngine;

namespace MCRGame.Tester
{
    /// <summary>
    /// GUI를 통해 다양한 이펙트를 스폰하는 툴입니다.
    /// 여기서는 물기둥(prefab) 이펙트를 화면에서 지정한 위치에 띄울 수 있게 합니다.
    /// </summary>
    public class WaterColumnTester : MonoBehaviour
    {
        [Header("Water Column Effect")]
        [Tooltip("스폰할 물기둥 이펙트 Prefab")]
        public GameObject waterColumnPrefab;
        [Tooltip("자동 삭제 시간(초), 0이면 삭제하지 않음)")]
        public float effectLifetime = 5f;

        // GUI를 통해 입력할 위치
        private float spawnX = 0f;
        private float spawnY = 0f;
        private float spawnZ = 0f;

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 240), "Water Column Tester", GUI.skin.window);

            GUILayout.Label("Water Column Prefab:");
            GUILayout.Label(waterColumnPrefab ? waterColumnPrefab.name : "None", GUI.skin.box);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnX.ToString("F2"), GUILayout.Width(60)), out spawnX);
            GUILayout.Label("Y:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnY.ToString("F2"), GUILayout.Width(60)), out spawnY);
            GUILayout.Label("Z:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnZ.ToString("F2"), GUILayout.Width(60)), out spawnZ);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lifetime:", GUILayout.Width(60));
            float.TryParse(GUILayout.TextField(effectLifetime.ToString("F2"), GUILayout.Width(60)), out effectLifetime);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (waterColumnPrefab == null)
            {
                GUILayout.Label("Prefab을 할당하세요.", GUI.skin.box);
            }
            else if (GUILayout.Button("Spawn Water Column", GUILayout.Height(30)))
            {
                SpawnWaterColumn(new Vector3(spawnX, spawnY, spawnZ));
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// 물기둥 이펙트를 지정 위치에 스폰합니다.
        /// </summary>
        void SpawnWaterColumn(Vector3 worldPos)
        {
            GameObject go = Instantiate(waterColumnPrefab, worldPos, Quaternion.identity);
            if (effectLifetime > 0f)
                Destroy(go, effectLifetime);
        }
    }
}
