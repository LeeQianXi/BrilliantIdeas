namespace NetUtility;

public static partial class Utility
{
    public static partial class Text
    {
        /// <summary>
        ///     字符辅助器接口。
        /// </summary>
        public interface ITextHelper
        {
            /// <summary>
            ///     获取格式化字符串。
            /// </summary>
            /// <typeparam name="T">字符串参数的类型。</typeparam>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg">字符串参数。</param>
            /// <returns>格式化后的字符串。</returns>
            string Format<T>(string format, T arg);

            /// <summary>
            ///     获取格式化字符串。
            /// </summary>
            /// <typeparam name="T1">字符串参数 1 的类型。</typeparam>
            /// <typeparam name="T2">字符串参数 2 的类型。</typeparam>
            /// <param name="format">字符串格式。</param>
            /// <param name="arg1">字符串参数 1。</param>
            /// <param name="arg2">字符串参数 2。</param>
            /// <returns>格式化后的字符串。</returns>
            string Format<T1, T2>(string format, T1 arg1, T2 arg2);

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
            string Format<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

            /// <summary>
            ///     获取格式化字符串。
            /// </summary>
            /// <param name="format">字符串格式。</param>
            /// <param name="args">字符串参数</param>
            /// <returns>格式化后的字符串。</returns>
            string Format(string format, params object[] args);
        }
    }
}