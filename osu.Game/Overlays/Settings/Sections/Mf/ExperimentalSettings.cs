using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class ExperimentalSettings : SettingsSubsection
    {
        protected override string Header => "实验性功能";

        private readonly Bindable<string> customWindowIconPath = new Bindable<string>();
        private TextFlowContainer textFlow;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(MConfigManager mConfig, OsuGame game, CustomStorage customStorage)
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
                textFlow = new TextFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 15 },
                    Margin = new MarginPadding { Bottom = 8 }
                }
            };

            if (RuntimeInfo.IsDesktop)
            {
                bool isSdlBackend = host.Window is SDL2DesktopWindow;
                Bindable<bool> fadeOutWindowBindable;
                Bindable<bool> fadeInWindowBindable;

                Add(new ExperimentalSettingsSetupContainer("自定义窗口图标", MSetting.CustomWindowIconPath));

                Add(new SettingsCheckbox
                {
                    LabelText = "退出时淡出窗口",
                    TooltipText = isSdlBackend ? string.Empty : "仅当窗口后端为SDL2时可用",
                    Current = fadeOutWindowBindable = mConfig.GetBindable<bool>(MSetting.FadeOutWindowWhenExiting),
                });

                Add(new SettingsCheckbox
                {
                    LabelText = "启动时淡入窗口",
                    TooltipText = isSdlBackend ? string.Empty : "仅当窗口后端为SDL2时可用",
                    Current = fadeInWindowBindable = mConfig.GetBindable<bool>(MSetting.FadeInWindowWhenEntering),
                });

                Add(new SettingsCheckbox
                {
                    LabelText = "使用系统光标",
                    Current = mConfig.GetBindable<bool>(MSetting.UseSystemCursor),
                });

                fadeOutWindowBindable.Disabled = !isSdlBackend;
                fadeInWindowBindable.Disabled = !isSdlBackend;
            }

            if (customStorage.ActiveFonts.Count > 0)
            {
                textFlow.AddParagraph("已加载的字体：",
                    f => f.Font = new FontUsage(family: "Noto-CJK-Basic"));

                foreach (var font in customStorage.ActiveFonts)
                {
                    textFlow.AddParagraph($"{font.Author} - {font.Name}", f =>
                    {
                        f.Font = OsuFont.Default;
                        f.UseLegacyUnicode = true;
                    });
                }
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
