using M.Resources.Fonts;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Game.Configuration;
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
        private readonly Bindable<Font> currentFont = new Bindable<Font> { Default = default_font };

        private static readonly Font default_font = new FakeFont
        {
            Name = "Torus",
            Author = "Paulo Goode",
            Homepage = "https://paulogoode.com/torus/",
            FamilyName = "Torus"
        };

        private TextFlowContainer textFlow;
        private SettingsDropdown<Font> dropDown;

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load(MConfigManager mConfig, OsuGame game, CustomStore customStorage)
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
                textFlow = new TextFlowContainerWithTooltip
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 15 },
                    Margin = new MarginPadding { Top = 8 },
                    TooltipText = "字体将从下往上应用, 先加载的字体将被后加载的字体覆盖"
                },
                dropDown = new DefaultFontSettingsDropDown
                {
                    LabelText = "默认字体"
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

            var fonts = customStorage.ActiveFonts;
            fonts.Insert(0, default_font);
            fonts.Add(new FakeFont
            {
                Name = "Noto fonts",
                Author = "Google",
                Homepage = "https://www.google.com/get/noto/",
                FamilyName = "Noto-CJK-Compatibility"
            });

            dropDown.Items = fonts;
            currentFont.Value = fonts.Find(f => f.FamilyName == mConfig.Get<string>(MSetting.CurrentFont));
            currentFont.BindValueChanged(v => mConfig.Set(MSetting.CurrentFont, v.NewValue.FamilyName));
            dropDown.Current = currentFont;

            //如果字体数>0，则显示已加载的字体
            if (customStorage.ActiveFonts.Count > 0)
            {
                textFlow.AddParagraph("已加载的字体：",
                    f => f.Font = new FontUsage("Noto-CJK-Basic"));

                foreach (var font in customStorage.ActiveFonts)
                {
                    textFlow.AddParagraph($"{font.Author} - ", f =>
                    {
                        f.Font = new FontUsage("Noto-CJK-Basic", 18);
                        f.UseLegacyUnicode = true;
                    });

                    textFlow.AddText(font.Name, f =>
                    {
                        f.Font = new FontUsage($"{font.FamilyName}-Regular", 18);
                        f.UseLegacyUnicode = true;
                    });
                }
            }

            mConfig.BindWith(MSetting.CustomWindowIconPath, customWindowIconPath);
            customWindowIconPath.BindValueChanged(v => game?.SetWindowIcon(v.NewValue));
        }

        private class TextFlowContainerWithTooltip : TextFlowContainer, IHasTooltip
        {
            public string TooltipText { get; set; }
        }

        private class DefaultFontSettingsDropDown : SettingsDropdown<Font>
        {
            protected override OsuDropdown<Font> CreateDropdown() => new FontDropdownControl();

            private class FontDropdownControl : DropdownControl
            {
                protected override string GenerateItemText(Font font) => $"{font.Author} - {font.Name}({font.FamilyName})";
            }
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

        public class FakeFont : Font
        {
            public FakeFont()
            {
                Name = "Torus";
                Author = "Paulo Goode";
                Homepage = "https://paulogoode.com/torus/";
                FamilyName = "Torus";
            }
        }
    }
}
