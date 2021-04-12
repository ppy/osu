using Mvis.Plugin.Example.Config;
using Mvis.Plugin.Example.UI;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;
using osuTK.Graphics;

namespace Mvis.Plugin.Example
{
    public class ExamplePlugin : MvisPlugin
    {
        private OsuSpriteText text1;
        private OsuSpriteText text2;
        private OsuSpriteText text3;
        private OsuSpriteText text4;

        public override TargetLayer Target => TargetLayer.Foreground;

        //只会在Flags中有HasConfig的情况下被调用
        public override IPluginConfigManager CreateConfigManager(Storage storage)
            => new ExamplePluginConfigManager(storage);

        //只会在Flags中有HasConfig的情况下被调用
        public override PluginSettingsSubSection CreateSettingsSubSection()
            => new ExampleSettings(this);

        public ExamplePlugin()
        {
            Name = "示例插件";
            Description = "我是描述!";
            Author = "MATRIX-夜翎";

            Flags.AddRange(new[]
            {
                PluginFlags.CanDisable,
                PluginFlags.CanUnload,
                PluginFlags.HasConfig //声明该插件有配置相关的功能
            });

            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

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
        private Bindable<string> bindableString;
        private Bindable<ExampleEnum> bindableEnum;

        protected override bool PostInit()
        {
            Logger.Log($"插件{Name}开始加载前的准备!");

            //从MvisPluginManager获取自己的配置管理器
            var config = (ExamplePluginConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(this);
            bindableString = config.GetBindable<string>(ExamplePluginSettings.KeyString);
            bindableFloat = config.GetBindable<float>(ExamplePluginSettings.KeyFloat);
            bindableEnum = config.GetBindable<ExampleEnum>(ExamplePluginSettings.keyEnum);
            return true;
        }

        protected override bool OnContentLoaded(Drawable content)
        {
            bindableString.BindValueChanged(onBindableStringValueChanged);
            bindableEnum.BindValueChanged(onBindableEnumValueChanged);

            //osu.Framework/Bindables/Bindable.cs#L106:
            //public void BindValueChanged(Action<ValueChangedEvent<T>> onChange, bool runOnceImmediately = false)
            bindableFloat.BindValueChanged(onBindableFloatValueChanged, true);

            Logger.Log($"插件{Name}已加载!");
            return true;
        }

        private void onBindableStringValueChanged(ValueChangedEvent<string> v)
        {
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

            //如果新值没有描述，则设置文本为ToString的结果
            text4.Text = v.NewValue.GetDescription() ?? v.NewValue.ToString();
        }

        public override bool Disable()
        {
            setText("插件被禁用了oAo!");

            return base.Disable();
        }

        public override bool Enable()
        {
            setText("插件被启用了oAo!");

            return base.Enable();
        }

        private void setText(string text)
        {
            if (text1 != null)
                text1.Text = text;
        }
    }
}
