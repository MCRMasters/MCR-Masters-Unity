using UnityEngine;
using MCRGame.Effect;

namespace MCRGame.Tester
{
    public class WaterColumnEffectTester : MonoBehaviour
    {
        [Header("Water Column Effect Prefab")]
        [Tooltip("테스트할 WaterColumnEffect 프리팹")]
        public GameObject waterColumnPrefab;

        [Header("Spawn Position")]
        [Tooltip("스폰할 위치 X, Y, Z")]
        public float spawnX = 0f;
        public float spawnY = 0f;
        public float spawnZ = 0f;

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 180), "Water Column Tester", GUI.skin.window);

            // 프리팹 표시
            GUILayout.Label("Prefab:");
            GUILayout.Label(waterColumnPrefab ? waterColumnPrefab.name : "None", GUI.skin.box);

            GUILayout.Space(5);

            // 스폰 좌표 입력 필드
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnX.ToString("F2"), GUILayout.Width(60)), out spawnX);
            GUILayout.Label("Y:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnY.ToString("F2"), GUILayout.Width(60)), out spawnY);
            GUILayout.Label("Z:", GUILayout.Width(20));
            float.TryParse(GUILayout.TextField(spawnZ.ToString("F2"), GUILayout.Width(60)), out spawnZ);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Spawn 버튼
            if (waterColumnPrefab == null)
            {
                GUILayout.Label("Assign the prefab in Inspector.", GUI.skin.box);
            }
            else if (GUILayout.Button("Spawn & Play Effect", GUILayout.Height(30)))
            {
                SpawnAndPlayEffect();
            }

            GUILayout.EndArea();
        }

        private void SpawnAndPlayEffect()
        {
            Vector3 pos = new Vector3(spawnX, spawnY, spawnZ);
            GameObject go = Instantiate(waterColumnPrefab, pos, Quaternion.identity);
            var effect = go.GetComponent<WaterColumnEffect>();
            if (effect != null)
            {
                // PlayEffect() 호출로 DOTween 시퀀스 시작
                effect.PlayEffect();
            }
            else
            {
                Debug.LogError("WaterColumnEffect 컴포넌트를 찾을 수 없습니다!");
            }
        }
    }
}
