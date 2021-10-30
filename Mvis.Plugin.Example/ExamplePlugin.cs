using Mvis.Plugin.Example.Config;
using Mvis.Plugin.Example.DBus;
using Mvis.Plugin.Example.Sidebar;
using Mvis.Plugin.Example.UI;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Config;
using osuTK.Graphics;

namespace Mvis.Plugin.Example
{
    public class ExamplePlugin : LLinPlugin
    {
        private OsuSpriteText text1;
        private OsuSpriteText text2;
        private OsuSpriteText text3;
        private OsuSpriteText text4;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.TargetLayer"/>
        /// </summary>
        public override TargetLayer Target => TargetLayer.Foreground;

        /// <summary>
        /// 插件兼容性版本，截至2021.04.16，MvisPluginManager的兼容版本为1。<br/>
        /// 加载任何大于或小于MvisPluginManager兼容版本的插件都会弹出警告。
        /// </summary>
        public override int Version => 1;

        //指定插件的配置管理器
        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new ExamplePluginConfigManager(storage);

        //指定插件的设置界面
        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new ExampleSettings(this);

        //指定插件的侧边栏页面
        public override PluginSidebarPage CreateSidebarPage()
            => new ExampleSidebarPage(this);

        public ExamplePlugin()
        {
            Name = "示例插件";
            Description = "我是描述!";
            Author = "MATRIX-夜翎";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            //注册DBus物件
            if (RuntimeInfo.OS == RuntimeInfo.Platform.Linux)
                PluginManager.RegisterDBusObject(new ExampleDBusObject());
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.CreateContent()"/>
        /// </summary>
        protected override Drawable CreateContent() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "你发现了这段由插件生成的文字!",
                    Font = OsuFont.GetFont(size: 30)
                },
                new OsuSpriteText
                {
                    Text = "因为Target属性是TargetLayer.Foreground，所以这段文字会显示在前景!"
                },
                new OsuSpriteText
                {
                    Text = "你可以在一个dll里塞多个插件, 只要你提供对应的MvisPluginProvider!"
                },
                text1 = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 30)
                },
                text2 = new OsuSpriteText(),
                text3 = new OsuSpriteText(),
                text4 = new OsuSpriteText()
            }
        };

        private Bindable<float> bindableFloat;
        private Bindable<ExampleEnum> bindableEnum;
        public Bindable<string> BindableString;

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.PostInit()"/>
        /// </summary>
        protected override bool PostInit()
        {
            Logger.Log($"插件{Name}开始加载前的准备!");

            //从MvisPluginManager获取自己的配置管理器
            var config = (ExamplePluginConfigManager)Dependencies.Get<LLinPluginManager>().GetConfigManager(this);

            //从配置管理器获取bindable，你也可以用config.BindWith()来将某一配置和现有的Bindable绑定。
            BindableString = config.GetBindable<string>(ExamplePluginSettings.KeyString);
            bindableFloat = config.GetBindable<float>(ExamplePluginSettings.KeyFloat);
            bindableEnum = config.GetBindable<ExampleEnum>(ExamplePluginSettings.keyEnum);
            return true;
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.OnContentLoaded(Drawable)"/>
        /// </summary>
        protected override bool OnContentLoaded(Drawable content)
        {
            //当bindableString的值改变时，调用onBindableStringValueChanged。
            BindableString.BindValueChanged(onBindableStringValueChanged);

            //当bindableEnum的值改变时，调用onBindableEnumValueChanged。
            bindableEnum.BindValueChanged(onBindableEnumValueChanged);

            //osu.Framework/Bindables/Bindable.cs#L106:
            //public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false)
            //当bindableFloat的值改变时，调用onBindableFloatValueChanged。
            bindableFloat.BindValueChanged(onBindableFloatValueChanged, true);

            //在这里设置可以避免bindable值发生改变时因插件内容未被加载导致的NullReferenceException

            Logger.Log($"插件{Name}已加载!");
            return true;
        }

        private void onBindableStringValueChanged(ValueChangedEvent<string> v)
        {
            //设置text2的文本
            text2.Text = "BindableString: " + v.NewValue;
        }

        private void onBindableFloatValueChanged(ValueChangedEvent<float> v)
        {
            //你也可以这样设置文本
            text3.Text = $"BindableFloat: {v.NewValue}";
        }

        private void onBindableEnumValueChanged(ValueChangedEvent<ExampleEnum> v)
        {
            switch (v.NewValue)
            {
                case ExampleEnum.Key1:
                    this.FadeColour(Color4.Gold);
                    break;

                case ExampleEnum.Key2:
                    this.FadeColour(Color4.White, 300, Easing.OutQuint);
                    break;
            }

            //设置text4的文本
            text4.Text = v.NewValue.GetDescription();
        }

        /// <summary>
        /// 设置text1的文本
        /// </summary>
        /// <param name="text"></param>
        private void setText(string text)
        {
            //setText被调用时text1可能尚未被加载
            //故添加null检测
            if (text1 != null)
                text1.Text = text;
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.Disable()"/>
        /// </summary>
        public override bool Disable()
        {
            setText("插件被禁用了oAo!");

            return base.Disable();
        }

        /// <summary>
        /// 请参阅 <see cref="LLinPlugin.Enable()"/>
        /// </summary>
        public override bool Enable()
        {
            setText("插件被启用了oAo!");

            return base.Enable();
        }
    }
}
