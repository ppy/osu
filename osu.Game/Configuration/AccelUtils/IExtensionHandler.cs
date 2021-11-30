using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace osu.Game.Configuration.AccelUtils
{
    public interface IExtensionHandler
    {
        /// <summary>
        /// 处理器要处理的扩展名
        /// </summary>
        public string ExtensionName { get; }

        /// <summary>
        /// 处理器支持的属性
        /// </summary>
        public string[] SupportedProperties { get; }

        /// <summary>
        /// 处理器名称
        /// </summary>
        public string HandlerName { get; }

        /// <summary>
        /// 将输入的值转换为特定的输出
        /// </summary>
        /// <param name="name">属性名称</param>
        /// <param name="value">属性值</param>
        /// <param name="errors">错误列表</param>
        /// <returns>属性是否支持</returns>
        public bool Process(string name, [NotNull] ref object value, [NotNull] ref IList<string> errors);
    }
}
