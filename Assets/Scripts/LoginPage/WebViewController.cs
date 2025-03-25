using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebViewController : MonoBehaviour
{
    public string Url;                              // 기본 URL (원한다면 Inspector에서 설정)
    public int LeftMargin, RightMargin, TopMargin, BottomMargin;

    [SerializeField]
    private WebViewObject webViewObject;

    private Coroutine _loadCoroutine;

    /// <summary>
    /// 버튼 클릭 등으로 WebView에 URL을 로드하고 싶을 때 호출
    /// </summary>
    public void OpenUrl(string url)
    {
        // 이미 로드 중이면 중단
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
        }
        _loadCoroutine = StartCoroutine(LoadWebView(url));
        SetVisibility(true);
    }

    /// <summary>
    /// WebView를 표시/숨기기
    /// </summary>
    public void SetVisibility(bool visibility)
    {
        if (webViewObject != null)
            webViewObject.SetVisibility(visibility);
    }

    /// <summary>
    /// WebView 표시 상태 반환
    /// </summary>
    public bool GetVisibility()
    {
        return webViewObject != null && webViewObject.GetVisibility();
    }

    /// <summary>
    /// 지정된 URL을 로드하는 코루틴
    /// </summary>
    private IEnumerator LoadWebView(string url)
    {
        webViewObject.Init(
            cb: (msg) => { Debug.Log("CallFromJS: " + msg); },
            err: (msg) => { Debug.LogError("CallOnError: " + msg); },
            httpErr: (msg) => { Debug.LogError("CallOnHttpError: " + msg); },
            started: (msg) => { Debug.Log("CallOnStarted: " + msg); },
            hooked: (msg) => { Debug.Log("CallOnHooked: " + msg); },
            cookies: (msg) => { Debug.Log("CallOnCookies: " + msg); },
            ld: (msg) => { Debug.Log("CallOnLoaded: " + msg); }
        );

        // WebView 여백, 줌 등 설정
        webViewObject.SetMargins(LeftMargin, TopMargin, RightMargin, BottomMargin);
        webViewObject.SetTextZoom(100);

#if !UNITY_WEBPLAYER && !UNITY_WEBGL
        if (url.StartsWith("http"))
        {
            webViewObject.LoadURL(url.Replace(" ", "%20"));
        }
        else
        {
            webViewObject.LoadURL("StreamingAssets/" + url.Replace(" ", "%20"));
        }
#else
        if (url.StartsWith("http"))
        {
            webViewObject.LoadURL(url.Replace(" ", "%20"));
        }
        else
        {
            webViewObject.LoadURL("StreamingAssets/" + url.Replace(" ", "%20"));
        }
#endif
        yield break;
    }

    private void OnDisable()
    {
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
        }
    }
}
