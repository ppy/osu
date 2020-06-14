using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.PurePlayer.Components
{
    public class SongProgressBar : Container
    {
        protected const float idle_alpha = 0.5f;
        public ProgressBar progressBar;

        [Resolved]
        private OsuColour colour { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                progressBar = new HoverableProgressBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillColour = colour.Yellow,
                    BackgroundColour = colour.GrayC.Opacity(0.5f),
                    Alpha = idle_alpha,
                },
            };
        }

        private class HoverableProgressBar : ProgressBar
        {
            protected override bool OnHover(HoverEvent e)
            {
                this.FadeTo(1, 500, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                this.FadeTo(idle_alpha, 500, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}