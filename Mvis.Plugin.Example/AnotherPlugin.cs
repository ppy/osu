using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins;

namespace Mvis.Plugin.Example
{
    public class AnotherPlugin : LLinPlugin
    {
        private OsuSpriteText text1;

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.BottomCentre,
            Origin = Anchor.BottomCentre,
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
                    Text = "因为Target属性是TargetLayer.Background，所以这段文字会显示在背景!"
                },
                new OsuSpriteText
                {
                    Text = "所有显示在背景的插件都会被应用背景暗化!"
                },
                text1 = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 30)
                }
            }
        };

        protected override bool OnContentLoaded(Drawable content)
        {
            Logger.Log($"插件{Name}已加载!");
            return true;
        }

        protected override bool PostInit()
        {
            Logger.Log($"插件{Name}开始加载前的准备!");
            return true;
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

        public override TargetLayer Target => TargetLayer.Background;
        public override int Version => 1;

        public AnotherPlugin()
        {
            Name = "另一个示例插件";
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
    }
}
