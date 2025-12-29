namespace NetUtility;

public static partial class Extension
{
    private static int ParseRgbShort(string str)
    {
        var r = Convert.ToInt32(str[0].ToString() + str[0], 16);
        var g = Convert.ToInt32(str[1].ToString() + str[1], 16);
        var b = Convert.ToInt32(str[2].ToString() + str[2], 16);
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }

    private static int ParseArgbShort(string str)
    {
        var a = Convert.ToInt32(str[0].ToString() + str[0], 16);
        var r = Convert.ToInt32(str[1].ToString() + str[1], 16);
        var g = Convert.ToInt32(str[2].ToString() + str[2], 16);
        var b = Convert.ToInt32(str[3].ToString() + str[3], 16);
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    private static int ParseRgb(string str)
    {
        var rgb = Convert.ToInt32(str, 16);
        return unchecked((int)0xFF000000) | rgb;
    }

    private static int ParseArgb(string str)
    {
        return Convert.ToInt32(str, 16);
    }

    extension(string color)
    {
        /// <summary>
        ///     颜色字符串转换为 ARGB 整数
        ///     支持格式：#RGB, #ARGB, #RRGGBB, #AARRGGBB
        /// </summary>
        public int ColorToArgbInt()
        {
            if (string.IsNullOrEmpty(color))
                throw new ArgumentException("颜色字符串不能为空");

            Color.FromName(color).ToArgb();
            var cleanStr = color.StartsWith("#") ? color[1..] : color;

            return cleanStr.Length switch
            {
                3 => ParseRgbShort(cleanStr),
                4 => ParseArgbShort(cleanStr),
                6 => ParseRgb(cleanStr),
                8 => ParseArgb(cleanStr),
                _ => throw new ArgumentException($"不支持的格式: {color}")
            };
        }

        public Color StringToColor()
        {
            return color.StartsWith("#") ? Color.FromArgb(color.ColorToArgbInt()) : Color.FromName(color);
        }

        public string StringToHexString()
        {
            return color.StartsWith("#") ? color : Color.FromName(color).ToArgbString();
        }
    }

    extension(Color color)
    {
        public string ToArgbString()
        {
            var (r, g, b, a) = color.ToArgb().GetColorComponents();
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }
    }

    extension(int argb)
    {
        /// <summary>
        ///     获取颜色分量
        /// </summary>
        public (int alpha, int red, int green, int blue) GetColorComponents()
        {
            return (
                (argb >> 24) & 0xFF,
                (argb >> 16) & 0xFF,
                (argb >> 8) & 0xFF,
                argb & 0xFF
            );
        }
    }

    extension(Rgba32 pixel1)
    {
        /// <summary>
        ///     计算两个像素的颜色相似度（欧几里得距离）
        /// </summary>
        /// <param name="pixel2">第二个像素</param>
        /// <returns>相似度分数（0-1，0表示完全相同）</returns>
        public float CalcEuclideanDifference(Rgba32 pixel2)
        {
            var rDiff = pixel1.R - pixel2.R;
            var gDiff = pixel1.G - pixel2.G;
            var bDiff = pixel1.B - pixel2.B;
            var aDiff = pixel1.B - pixel2.B;
            // 计算距离（考虑Alpha通道）
            var distance = (float)Math.Sqrt(
                rDiff * rDiff +
                gDiff * gDiff +
                bDiff * bDiff +
                aDiff * aDiff * 0.5f // Alpha通道权重较低
            );

            // 最大可能距离（考虑RGBA范围）
            var maxDistance = (float)Math.Sqrt(255 * 255 * 3 + 255 * 255 * 0.5f);

            // 转换为相似度（1 - 标准化距离）
            return Math.Max(0, 1 - distance / maxDistance);
        }

        /// <summary>
        ///     计算两个像素的CIEDE2000色差（更符合人眼感知）
        /// </summary>
        public float CalcCiede2000Difference(Rgba32 pixel2)
        {
            // 将RGB转换为Lab颜色空间（需要安装SixLabors.ImageSharp.ColorSpaces）
            var lab1 = pixel1.RgbToLab();
            var lab2 = pixel2.RgbToLab();

            // 计算Delta E（简化的CIEDE2000）
            var deltaL = lab1.L - lab2.L;
            var deltaA = lab1.A - lab2.A;
            var deltaB = lab1.B - lab2.B;

            return (float)Math.Sqrt(deltaL * deltaL + deltaA * deltaA + deltaB * deltaB);
        }

        /// <summary>
        ///     简化的RGB转Lab转换
        /// </summary>
        public (float L, float A, float B) RgbToLab()
        {
            // 将RGB转换为0-1范围
            var r = pixel1.R / 255f;
            var g = pixel1.G / 255f;
            var b = pixel1.B / 255f;

            // Gamma校正
            r = r > 0.04045f ? (float)Math.Pow((r + 0.055f) / 1.055f, 2.4f) : r / 12.92f;
            g = g > 0.04045f ? (float)Math.Pow((g + 0.055f) / 1.055f, 2.4f) : g / 12.92f;
            b = b > 0.04045f ? (float)Math.Pow((b + 0.055f) / 1.055f, 2.4f) : b / 12.92f;

            // 转换为XYZ颜色空间
            var x = r * 0.4124f + g * 0.3576f + b * 0.1805f;
            var y = r * 0.2126f + g * 0.7152f + b * 0.0722f;
            var z = r * 0.0193f + g * 0.1192f + b * 0.9505f;

            // D65白点参考
            x /= 0.95047f;
            y /= 1.0f;
            z /= 1.08883f;

            // 转换为Lab
            x = x > 0.008856f ? (float)Math.Pow(x, 1.0 / 3.0) : 7.787f * x + 16.0f / 116.0f;
            y = y > 0.008856f ? (float)Math.Pow(y, 1.0 / 3.0) : 7.787f * y + 16.0f / 116.0f;
            z = z > 0.008856f ? (float)Math.Pow(z, 1.0 / 3.0) : 7.787f * z + 16.0f / 116.0f;

            var L = 116.0f * y - 16.0f;
            var A = 500.0f * (x - y);
            var B = 200.0f * (y - z);

            return (L, A, B);
        }
    }
}