using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Screens.LLin.Plugins.Types
{
    public interface IFunctionProvider
    {
        /// <summary>
        /// 控制器按钮大小，设置后交由控制条负责处理
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// 由外部设置的行动，具体是否调用视Active()中是否实现而定。
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// 控制器图标，设置后交由控制条负责处理
        /// </summary>
        public IconUsage Icon { get; set; }

        /// <summary>
        /// 控制器标题（名称），设置后交由控制条负责处理
        /// </summary>
        public LocalisableString Title { get; set; }

        /// <summary>
        /// 控制器描述，设置后交由控制条负责处理
        /// </summary>
        public LocalisableString Description { get; set; }

        /// <summary>
        /// 控制器类型
        /// </summary>
        public FunctionType Type { get; set; }

        /// <summary>
        /// 激活此控制器，用于执行动作
        /// </summary>
        public void Active();

        public string ToString() => $"{Title} - {Description}";
    }

    public interface IToggleableFunctionProvider : IFunctionProvider
    {
        /// <summary>
        /// 可切换的值
        /// </summary>
        public BindableBool Bindable { get; set; }
    }

    public interface IPluginFunctionProvider : IFunctionProvider
    {
        /// <summary>
        /// 源插件侧边栏页面
        /// </summary>
        public PluginSidebarPage SourcePage { get; set; }
    }

    public enum FunctionType
    {
        Base,
        Audio,
        Plugin,
        Misc,
        ProgressDisplay
    }
}
