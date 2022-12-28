using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

#nullable disable

namespace osu.Game.Overlays.Toolbar
{
    public partial class LegacyMfClockDisplay : ClockDisplay
    {
        private readonly OsuSpriteText spriteText;
        private readonly FillFlowContainer tooltipContainer;
        private readonly OsuSpriteText mainTooltip;
        private readonly OsuSpriteText subTooltip;

        private LocalisableString tooltipMain
        {
            get => mainTooltip.Text;
            set => mainTooltip.Text = value;
        }

        private LocalisableString tooltipSub
        {
            get => subTooltip.Text;
            set => subTooltip.Text = value;
        }

        public LegacyMfClockDisplay()
        {
            AutoSizeAxes = Axes.X;
            Height = 40; //Workaround: 悬浮时显示Tooltip

            InternalChildren = new Drawable[]
            {
                spriteText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                },
                tooltipContainer = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Alpha = 0,
                    Y = 45,
                    Children = new Drawable[]
                    {
                        mainTooltip = new OsuSpriteText
                        {
                            Shadow = true,
                            Font = OsuFont.GetFont(size: 22, weight: FontWeight.Bold),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        },
                        subTooltip = new OsuSpriteText
                        {
                            Shadow = true,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight
                        }
                    }
                }
            };
        }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        private int beatmapCountReal;

        private int beatmapCount
        {
            get => beatmapCountReal;
            set
            {
                beatmapCountReal = value;
                Schedule(updateBeatmapTooltip);
            }
        }

        private void updateBeatmapTooltip() =>
            tooltipSub = $"你共有{beatmapCount}张谱面!";

        [BackgroundDependencyLoader]
        private void load()
        {
            beatmapCount = beatmapManager.GetAllUsableBeatmapSets().Count;
            beatmapManager.OnBeatmapAdded += c => beatmapCount += c;
            beatmapManager.OnBeatmapHide += c => beatmapCount -= c;
        }

        protected override void LoadComplete()
        {
            updateBeatmapTooltip();
            base.LoadComplete();
        }

        protected override void UpdateDisplay(DateTimeOffset now)
        {
            spriteText.Text = $"{now:yyyy/MM/dd tth:mm:ss}";

            tooltipMain = $"osu!已经运行了 {new TimeSpan(TimeSpan.TicksPerSecond * (int)(Clock.CurrentTime / 1000)):c}";
        }

        protected override bool OnHover(HoverEvent e)
        {
            tooltipContainer.FadeIn(300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            tooltipContainer.FadeOut(300, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
