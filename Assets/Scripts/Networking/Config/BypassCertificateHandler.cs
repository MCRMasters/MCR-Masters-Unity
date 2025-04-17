using UnityEngine.Networking;

namespace MCRGame.Net
{
    public class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 항상 true를 리턴하여 인증서 검증을 건너뜀 (테스트 용으로만 사용!)
            return true;
        }
    }

}