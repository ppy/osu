using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class LocaleSettings : SettingsSubsection
    {
        private OsuTextBox textBox;
        protected override string Header => "语言环境(Locale)";

        [Resolved]
        private FrameworkConfigManager config { get; set; }

        [Resolved]
        private MConfigManager mConfig { get; set; }

        [Resolved]
        private OsuGame game { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = 15 },
                    Child = textBox = new OsuTextBox
                    {
                        PlaceholderText = "无locale设置",
                        RelativeSizeAxes = Axes.X,
                        CommitOnFocusLost = true
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            textBox.Text = config.Get<string>(FrameworkSetting.Locale);
            textBox.OnCommit += setLocale;
        }

        private void setLocale(TextBox sender, bool newtext)
        {
            config.SetValue(FrameworkSetting.Locale, sender.Text);
        }
    }
}
