using System.Collections.Generic;

namespace MarkingDesigner.Helpers
{
    public static class StringExtensions
    {
        // @0-@9 を 0x90-0x99 に変換しつつバイト列化
        public static byte[] ToMarkingBytes(this string text)
        {
            if (string.IsNullOrEmpty(text)) return new byte[0];
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '@' && i + 1 < text.Length && char.IsDigit(text[i + 1]))
                {
                    int val = text[i + 1] - '0';
                    bytes.Add((byte)(0x90 + val));
                    i++;
                }
                else if (c < 128) bytes.Add((byte)c);
                else bytes.Add((byte)'?');
            }
            return bytes.ToArray();
        }
    }
}