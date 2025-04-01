using System.Collections;
using UnityEngine;

namespace MCRGame.Net
{
    public class WebViewController : MonoBehaviour
    {
        public string Url; // 기본 URL (Inspector에서 필요 시 설정)
        public int LeftMargin, RightMargin, TopMargin, BottomMargin; // 여백 설정

        [SerializeField]
        private WebViewObject webViewObject; // Inspector에서 WebViewObject를 연결

        // 토큰을 받으면 호출될 콜백 (TokenResponse를 전달)
        public System.Action<TokenResponse> OnTokenReceived;

        private Coroutine _loadCoroutine;

        /// <summary>
        /// 외부에서 URL을 로드하고 웹뷰를 열기 위해 호출합니다.
        /// </summary>
        public void OpenUrl(string url)
        {
            if (_loadCoroutine != null)
                StopCoroutine(_loadCoroutine);
            _loadCoroutine = StartCoroutine(LoadWebView(url));
            SetVisibility(true);
        }

        /// <summary>
        /// 웹뷰 표시 여부를 설정합니다.
        /// </summary>
        public void SetVisibility(bool visibility)
        {
            if (webViewObject != null)
                webViewObject.SetVisibility(visibility);
        }

        /// <summary>
        /// 지정된 URL을 로드하는 코루틴입니다.
        /// </summary>
        private IEnumerator LoadWebView(string url)
        {
            webViewObject.Init(
                cb: (msg) =>
                {
                    Debug.Log("[WebViewController] CallFromJS: " + msg);
                    // 콜백 메시지가 JSON 형식이라면 파싱 시도
                    try
                    {
                        TokenResponse token = JsonUtility.FromJson<TokenResponse>(msg);
                        if (!string.IsNullOrEmpty(token.access_token))
                        {
                            Debug.Log($"[WebViewController] AccessToken = {token.access_token}");
                            Debug.Log($"[WebViewController] RefreshToken = {token.refresh_token}");
                            OnTokenReceived?.Invoke(token);
                            SetVisibility(false);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("[WebViewController] JSON 파싱 실패: " + e);
                    }
                },
                err: (msg) =>
                {
                    Debug.LogError("[WebViewController] CallOnError: " + msg);
                },
                httpErr: (msg) =>
                {
                    Debug.LogError("[WebViewController] CallOnHttpError: " + msg);
                },
                started: (msg) =>
                {
                    Debug.Log("[WebViewController] CallOnStarted: " + msg);
                },
                hooked: (msg) =>
                {
                    Debug.Log("[WebViewController] CallOnHooked: " + msg);
                },
                cookies: (msg) =>
                {
                    Debug.Log("[WebViewController] CallOnCookies: " + msg);
                },
                ld: (msg) =>
                {
                    Debug.Log("[WebViewController] CallOnLoaded: " + msg);
                    // 페이지 로드 완료 시, JavaScript를 평가하여 document.body.innerText(= JSON 문자열) 전달
                    string js = @"
                    (function() {
                        var content = document.body.innerText;
                        Unity.call(content);
                    })();
                ";
                    webViewObject.EvaluateJS(js);
                }
            );

            webViewObject.SetMargins(LeftMargin, TopMargin, RightMargin, BottomMargin);
            webViewObject.SetTextZoom(100);
            webViewObject.LoadURL(url.Replace(" ", "%20"));
            yield break;
        }

        private void OnDisable()
        {
            if (_loadCoroutine != null)
                StopCoroutine(_loadCoroutine);
        }
    }
}