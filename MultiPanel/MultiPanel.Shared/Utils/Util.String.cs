namespace MultiPanel.Shared.Utils;

public static class Util
{
    public static class String
    {
        /// <summary>
        ///     生成安全的随机密码
        /// </summary>
        /// <param name="length">密码长度</param>
        /// <param name="includeSpecialChars">是否包含特殊字符</param>
        /// <returns>随机密码</returns>
        public static string GenerateRandomPassword(int length = 16, bool includeSpecialChars = true)
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnpqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var chars = upper + lower + digits;
            if (includeSpecialChars) chars += special;

            var random = new Random();
            var password = new char[length];

            // 确保至少包含每种类型字符
            password[0] = upper[random.Next(upper.Length)];
            password[1] = lower[random.Next(lower.Length)];
            password[2] = digits[random.Next(digits.Length)];

            if (includeSpecialChars)
            {
                password[3] = special[random.Next(special.Length)];

                // 填充剩余位置
                for (var i = 4; i < length; i++) password[i] = chars[random.Next(chars.Length)];
            }
            else
            {
                // 填充剩余位置
                for (var i = 3; i < length; i++) password[i] = chars[random.Next(chars.Length)];
            }

            // 随机打乱字符顺序
            for (var i = 0; i < length; i++)
            {
                var j = random.Next(i, length);
                (password[i], password[j]) = (password[j], password[i]);
            }

            return new string(password);
        }
    }
}