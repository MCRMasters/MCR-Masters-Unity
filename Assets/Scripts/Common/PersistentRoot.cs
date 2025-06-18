using System.Collections.Generic;
using UnityEngine;

namespace MCRGame.Common
{
    /// <summary>
    /// 루트 오브젝트를 씬 전환 시에도 파괴되지 않게 만든다.
    /// 같은 이름의 객체가 이미 살아 있으면 자신을 파괴해 중복을 방지한다.
    /// </summary>
    public class PersistentRoot : MonoBehaviour
    {
        private static readonly HashSet<string> Preserved = new();

        private void Awake()
        {
            // (1) 이미 같은 이름으로 보존된 오브젝트가 있으면 중복 제거
            if (Preserved.Contains(gameObject.name))
            {
                Destroy(gameObject);
                return;
            }

            // (2) 처음 발견된 루트 → DontDestroyOnLoad
            Preserved.Add(gameObject.name);
            DontDestroyOnLoad(gameObject);
        }
    }
}