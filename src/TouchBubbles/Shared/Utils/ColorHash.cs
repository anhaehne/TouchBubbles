using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;


// source: https://github.com/zenozeng/color-hash
namespace TouchBubbles.Shared.Utils
{
    public static class ColorHash
    {
        private static readonly double[] _saturationValues = new[] { 0.35, 0.5, 0.65 };
        private static readonly double[] _luminanceValues = new[] { 0.35, 0.4, 0.5 };

        public static (int Hue, double Saturation, double Luminance) HSL(string str)
        {
            var hash = GetHash(str);

            var hue = (int)(hash % 359);

            hash = hash / 360;
            var saturation = _saturationValues[(int) (hash % _saturationValues.Length)];

            hash = hash / _saturationValues.Length;

            var x = hash % _luminanceValues.Length;
            var luminance = _luminanceValues[(int)(hash % _luminanceValues.Length)];

            return (hue, saturation, luminance);
        }

        public static string HEX(string str)
        {
            var hsl = HSL(str);
            var color = HslToColor(hsl.Hue, hsl.Saturation, hsl.Luminance);

            var r = color.R;
            var g = color.G;
            var b = color.B;

            var hex = ColorToHex(color);

            return hex;
        }

        private static string ColorToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private static Color HslToColor(int hue, double saturation, double luminance)
        {
            var hueTemp = hue / 360d;

            var q = luminance < 0.5 ? luminance * (1 + saturation) : luminance + saturation - luminance * saturation;
            var p = 2 * luminance - q;

            var r = ToColor(hueTemp + (1 / 3d));
            var g = ToColor(hueTemp);
            var b = ToColor(hueTemp - 1 / 3d);

            return Color.FromArgb(r, g, b);

            int ToColor(double color)
            {
                if (color < 0)
                {
                    color++;
                }
                if (color > 1)
                {
                    color--;
                }
                if (color < 1d / 6)
                {
                    color = p + (q - p) * 6 * color;
                }
                else if (color < 0.5)
                {
                    color = q;
                }
                else if (color < 2d / 3)
                {
                    color = p + (q - p) * 6 * (2d / 3 - color);
                }
                else
                {
                    color = p;
                }
                return Convert.ToInt32(Math.Round(color * 255));
            }
        }

        private static long GetHash(string str)
        {
            var hasher = SHA256.Create();
            var hashed = hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            return Math.Abs(BitConverter.ToInt32(hashed, 0));
        }
    }
}
