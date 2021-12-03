using System;
using osu.Framework.Graphics.Colour;
using osu.Framework.Localisation;

namespace osu.Game.Screens.LLin.Plugins
{
    public class DialogOption
    {
        /// <summary>
        /// 选项Action
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// 选项文本
        /// </summary>
        public LocalisableString Text { get; set; }

        /// <summary>
        /// 选项颜色
        /// <remarks>可能是背景，也可能时前景，具体看LLin实现方</remarks>
        /// </summary>
        public ColourInfo Color { get; set; }

        /// <summary>
        /// 选项类型
        /// 详见 <see cref="OptionType"/>
        /// </summary>
        public OptionType Type = OptionType.Common;
    }

    public enum OptionType
    {
        Common, //普通
        Confirm, //确认
        Cancel //取消
    }
}
