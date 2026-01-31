// ReSharper disable once CheckNamespace

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
}