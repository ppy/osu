using System;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Plugins;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class TrackTimeIndicator : CompositeDrawable
    {
        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        [Resolved]
        private LyricSidebarPage sidebarPage { get; set; }

        private readonly Bindable<double> globalOffset = new Bindable<double>();

        private OsuSpriteText timer;
        private OsuSpriteText offsetText;

        public TrackTimeIndicator()
        {
            Masking = true;
            CornerRadius = 5;
            AutoSizeAxes = Axes.Y;
            Width = 200;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            Margin = new MarginPadding(10);
            BorderThickness = 3;
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider provider)
        {
            var config = (LyricConfigManager)Dependencies.Get<MvisPluginManager>()
                                                         .GetConfigManager(sidebarPage.Plugin);

            BorderColour = provider.Content2;

            Box bgBox;
            InternalChildren = new Drawable[]
            {
                bgBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = provider.Highlight1
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        offsetText = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 16),
                            Colour = Color4.Black,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Margin = new MarginPadding { Horizontal = 10, Top = 10 }
                        },
                        timer = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 25, weight: FontWeight.Bold),
                            Colour = Color4.Black,
                            Margin = new MarginPadding { Horizontal = 10, Bottom = 10 },
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        }
                    }
                }
            };

            config.BindWith(LyricSettings.LyricOffset, globalOffset);

            provider.HueColour.BindValueChanged(_ =>
            {
                BorderColour = provider.Content2;
                bgBox.Colour = provider.Highlight1;
            }, true);
        }

        private double totalOffset => globalOffset.Value + mvisScreen?.CurrentTrack.CurrentTime ?? 0;

        protected override void Update()
        {
            timer.Text = $"{(totalOffset >= 0 ? "" : "-")}"
                         + $"{toTimeSpanText(totalOffset)}";

            offsetText.Text = $"{toTimeSpanText(mvisScreen.CurrentTrack.CurrentTime)}"
                              + $"{(globalOffset.Value >= 0 ? "+" : "-")}"
                              + $"{toTimeSpanText(globalOffset.Value)}";

            Width = Math.Min(Parent.DrawWidth * 0.45f, 200);
        }

        private string toTimeSpanText(double ms) => $"{TimeSpan.FromMilliseconds(ms):mm\\:ss\\.fff}";
    }
}
