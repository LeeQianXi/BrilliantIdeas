using NetUtility.Module;

namespace NetUtility;

public static partial class Utility
{
    /// <summary>
    ///     字符相关的实用函数。
    /// </summary>
    public static partial class Text
    {
        private static ITextHelper? _textHelper;

        /// <summary>
        ///     设置字符辅助器。
        /// </summary>
        /// <param name="textHelper">要设置的字符辅助器。</param>
        public static void SetTextHelper(ITextHelper textHelper)
        {
            _textHelper = textHelper;
        }

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <typeparam name="T">字符串参数的类型。</typeparam>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg">字符串参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format<T>(string format, T arg)
        {
            if (format == null) throw new NetUtilityException("Format is invalid.");

            return _textHelper == null ? string.Format(format, arg) : _textHelper.Format(format, arg);
        }

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <typeparam name="T1">字符串参数 1 的类型。</typeparam>
        /// <typeparam name="T2">字符串参数 2 的类型。</typeparam>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg1">字符串参数 1。</param>
        /// <param name="arg2">字符串参数 2。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            if (format == null) throw new NetUtilityException("Format is invalid.");

            return _textHelper == null ? string.Format(format, arg1, arg2) : _textHelper.Format(format, arg1, arg2);
        }

        /// <summary>
        ///     获取格式化字符串。
        /// </summary>
        /// <typeparam name="T1">字符串参数 1 的类型。</typeparam>
        /// <typeparam name="T2">字符串参数 2 的类型。</typeparam>
        /// <typeparam name="T3">字符串参数 3 的类型。</typeparam>
        /// <param name="format">字符串格式。</param>
        /// <param name="arg1">字符串参数 1。</param>
        /// <param name="arg2">字符串参数 2。</param>
        /// <param name="arg3">字符串参数 3。</param>
        /// <returns>格式化后的字符串。</returns>
        public static string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            if (format == null) throw new NetUtilityException("Format is invalid.");

            return _textHelper == null
                ? string.Format(format, arg1, arg2, arg3)
                : _textHelper.Format(format, arg1, arg2, arg3);
        }

        public static string Format(string format, params object[] args)
        {
            if (format == null) throw new NetUtilityException("Format is invalid.");

            return _textHelper == null ? string.Format(format, args) : _textHelper.Format(format, args);
        }
    }
}