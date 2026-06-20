using UnityEngine;
using QRCodeEncoderLibrary;

namespace Alounity.QR
{
    public static class QRCodeGenerator
    {
        public static Texture2D Generate(string text, int pixelsPerModule = 10)
        {
            var encoder = new QREncoder();
            var qrCode = encoder.Encode(text);

            if (qrCode == null || qrCode.Length == 0)
            {
                Debug.LogError("[QRCode] エンコードに失敗しました");
                return null;
            }

            int size = qrCode.GetLength(0);
            int textureSize = size * pixelsPerModule;

            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;

            Color black = Color.black;
            Color white = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color color = qrCode[y, x] ? black : white;

                    for (int py = 0; py < pixelsPerModule; py++)
                    {
                        for (int px = 0; px < pixelsPerModule; px++)
                        {
                            texture.SetPixel(x * pixelsPerModule + px, y * pixelsPerModule + py, color);
                        }
                    }
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
