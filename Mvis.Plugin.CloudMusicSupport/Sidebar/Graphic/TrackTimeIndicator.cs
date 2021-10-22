using System;
using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Plugins;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar.Graphic
{
    public class TrackTimeIndicator : CompositeDrawable
    {
        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        [Resolved]
        private LyricSidebarSectionContainer sidebarPage { get; set; }

        private readonly Bindable<double> globalOffset = new Bindable<double>();

        private OsuSpriteText timer;
        private OsuSpriteText offsetText;
        private FillFlowContainer fillFlow;

        public TrackTimeIndicator()
        {
            AutoSizeAxes = Axes.Y;
            Width = 200;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider provider)
        {
            var config = (LyricConfigManager)Dependencies.Get<LLinPluginManager>()
                                                         .GetConfigManager(sidebarPage.Plugin);

            BorderColour = provider.Content2;

            InternalChild = fillFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    timer = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 25, weight: FontWeight.Bold),
                        Colour = Color4.Black,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    offsetText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Medium, size: 16),
                        Colour = Color4.Black,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    }
                }
            };

            config.BindWith(LyricSettings.LyricOffset, globalOffset);
        }

        private double totalOffset => globalOffset.Value + mvisScreen?.CurrentTrack.CurrentTime ?? 0;

        protected override void Update()
        {
            timer.Text = $"{(totalOffset >= 0 ? "" : "-")}"
                         + $"{toTimeSpanText(totalOffset)}";

            offsetText.Text = $"{toTimeSpanText(mvisScreen.CurrentTrack.CurrentTime)}"
                              + $"{(globalOffset.Value >= 0 ? "+" : "-")}"
                              + $"{toTimeSpanText(globalOffset.Value)}";

            Width = Math.Max(fillFlow.DrawWidth, 200);
        }

        private string toTimeSpanText(double ms) => $"{TimeSpan.FromMilliseconds(ms):mm\\:ss\\.fff}";
    }
}
