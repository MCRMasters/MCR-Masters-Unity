using UnityEngine;

namespace MCRGame.Tester
{
    /// <summary>
    /// Inspector에서 Prefab을 할당하고, OnGUI로 입력한 위치에
    /// 간단히 이펙트를 스폰해 보는 툴입니다.
    /// Editor 전용 API 없이 Runtime에서도 동작합니다.
    /// </summary>
    public class EffectTester : MonoBehaviour
    {
        [Header("Effect Spawn Test")]
        [Tooltip("스폰할 이펙트 Prefab (예: water splash)")]
        public GameObject effectPrefab;
        [Tooltip("몇 초 뒤 자동 삭제할지 (0이면 삭제 안 함)")]
        public float effectLifetime = 3f;

        // Spawn 위치 입력용
        private float spawnX = 0f;
        private float spawnY = 0f;
        private float spawnZ = 0f;

        void OnGUI()
        {
            // GUI 영역 설정
            GUILayout.BeginArea(new Rect(10, 10, 260, 180), "Effect Spawn", GUI.skin.window);

            // Prefab 이름 표시
            GUILayout.Label("Prefab: " + (effectPrefab ? effectPrefab.name : "None"), GUILayout.Height(20));

            GUILayout.Space(5);

            // X, Y, Z 입력 필드
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", GUILayout.Width(20));
            string xs = GUILayout.TextField(spawnX.ToString("F2"), GUILayout.Width(60));
            float.TryParse(xs, out spawnX);
            GUILayout.Label("Y:", GUILayout.Width(20));
            string ys = GUILayout.TextField(spawnY.ToString("F2"), GUILayout.Width(60));
            float.TryParse(ys, out spawnY);
            GUILayout.Label("Z:", GUILayout.Width(20));
            string zs = GUILayout.TextField(spawnZ.ToString("F2"), GUILayout.Width(60));
            float.TryParse(zs, out spawnZ);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // Lifetime 입력 필드
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lifetime:", GUILayout.Width(60));
            string ls = GUILayout.TextField(effectLifetime.ToString("F2"), GUILayout.Width(60));
            float.TryParse(ls, out effectLifetime);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Spawn 버튼
            if (effectPrefab == null)
            {
                GUILayout.Label("Effect Prefab을 Inspector에서 할당하세요.", GUI.skin.box);
            }
            else if (GUILayout.Button("Spawn Effect", GUILayout.Height(30)))
            {
                SpawnEffect(new Vector3(spawnX, spawnY, spawnZ));
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// 지정한 월드 위치에 Prefab을 인스턴스화하고,
        /// effectLifetime 후에 자동으로 Destroy합니다.
        /// </summary>
        void SpawnEffect(Vector3 worldPos)
        {
            GameObject go = Instantiate(effectPrefab, worldPos, Quaternion.identity);
            if (effectLifetime > 0f)
                Destroy(go, effectLifetime);
        }
    }
}
