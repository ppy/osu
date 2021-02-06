using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class ExperimentalSettings : SettingsSubsection
    {
        protected override string Header => "实验性功能";

        private readonly Bindable<string> customWindowIconPath = new Bindable<string>();

        [BackgroundDependencyLoader]
        private void load(MConfigManager mConfig, OsuGame game)
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 15 },
                    Child = new OsuSpriteText
                    {
                        Text = "注意! 这些设置可能会很有帮助, 但调整不好可能会影响整个游戏的稳定性!",
                        RelativeSizeAxes = Axes.X,
                        Colour = Color4.Gold
                    },
                },
                new SettingsCheckbox
                {
                    LabelText = "使用自定义开屏页背景",
                    Current = mConfig.GetBindable<bool>(MSetting.UseCustomGreetingPicture)
                },
            };

            if (RuntimeInfo.IsDesktop)
            {
                Add(new ExperimentalSettingsSetupContainer("自定义窗口图标", MSetting.CustomWindowIconPath));
            }

            mConfig.BindWith(MSetting.CustomWindowIconPath, customWindowIconPath);
            customWindowIconPath.BindValueChanged(v => game?.SetWindowIcon(v.NewValue));
        }

        private class ExperimentalSettingsSetupContainer : FillFlowContainer
        {
            [Resolved]
            private MConfigManager mConfg { get; set; }

            private readonly OsuTextBox textBox;
            private readonly MSetting lookup;

            public ExperimentalSettingsSetupContainer(string description, MSetting lookup)
            {
                this.lookup = lookup;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 15 };
                Spacing = new Vector2(3);

                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = description,
                        RelativeSizeAxes = Axes.X,
                    },
                    textBox = new OsuTextBox
                    {
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                string text = mConfg.Get<string>(lookup);
                textBox.Text = text ?? "没有赋值";
                textBox.OnCommit += applySetting;
            }

            private void applySetting(TextBox sender, bool newtext)
            {
                mConfg.Set(lookup, sender.Text);
            }
        }
    }
}
