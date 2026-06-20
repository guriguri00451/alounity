using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Alounity.QR;

namespace Alounity.QR
{
    public class QRCodeDisplay : MonoBehaviour
    {
        [SerializeField] RawImage qrImage;
        [SerializeField] TextMeshProUGUI roomIdText;
        [SerializeField] GameObject rootPanel;

        Texture2D currentTexture;

        void Start()
        {
            if (SocketIOManager.Instance != null)
            {
                SocketIOManager.Instance.OnRoomCreated += ShowQRCode;
                // 既にルーム作成済みなら即表示
                if (!string.IsNullOrEmpty(SocketIOManager.Instance.RoomId))
                {
                    ShowQRCode();
                }
            }
        }

        void OnDisable()
        {
            if (SocketIOManager.Instance != null)
            {
                SocketIOManager.Instance.OnRoomCreated -= ShowQRCode;
            }
        }

        void ShowQRCode()
        {
            string roomId = SocketIOManager.Instance.RoomId;
            if (string.IsNullOrEmpty(roomId)) return;

            var uri = SocketIOManager.Instance.ServerUri;
            if (uri == null) return;

            string url = $"{uri.Scheme}://{uri.Host}:{uri.Port}/room/{roomId}";
            Debug.Log($"[QRCode] 生成URL: {url}");

            if (currentTexture != null)
            {
                Destroy(currentTexture);
            }

            currentTexture = QRCodeGenerator.Generate(url);
            if (currentTexture != null && qrImage != null)
            {
                qrImage.texture = currentTexture;
            }

            if (roomIdText != null)
            {
                roomIdText.text = roomId;
            }

            if (rootPanel != null)
            {
                rootPanel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (rootPanel != null)
            {
                rootPanel.SetActive(false);
            }
        }
    }
}
