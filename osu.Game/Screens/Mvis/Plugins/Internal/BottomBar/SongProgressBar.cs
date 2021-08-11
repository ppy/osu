using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Misc;

namespace osu.Game.Screens.Mvis.BottomBar
{
    public class SongProgressBar : ProgressBar
    {
        private Indicator indicator;
        private Indicator songProgressIndicator;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        private const float idle_alpha = 0.5f;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.BottomLeft;
            Anchor = Anchor.BottomLeft;
            RelativeSizeAxes = Axes.X;
            Height = 5;
            Alpha = idle_alpha;

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                FillColour = colourProvider.Highlight1;
                BackgroundColour = colourProvider.Light4.Opacity(0.5f);
            }, true);

            AddRange(new[]
            {
                songProgressIndicator = new Indicator
                {
                    Text = "歌曲进度"
                },
                indicator = new Indicator
                {
                    Text = "光标位置",
                    IsMouseIndicator = true
                }
            });
        }

        protected override void LoadComplete()
        {
            mvisScreen.OnIdle += () =>
            {
                if (IsHovered) showIndicators();
                UpdateValue(fill.Width);
            };
            mvisScreen.OnResumeFromIdle += hideIndicators;

            base.LoadComplete();
        }

        protected override bool OnHover(HoverEvent e)
        {
            this.ResizeHeightTo(7, 300, Easing.OutQuint).FadeTo(1, 300, Easing.OutQuint);
            showIndicators();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            this.ResizeHeightTo(5, 300, Easing.OutQuint).FadeTo(idle_alpha, 300, Easing.OutQuint);
            hideIndicators();
            base.OnHoverLost(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            indicator.MoveToX(
                getFinalPosX(indicator, ToLocalSpace(e.ScreenSpaceMousePosition).X - (indicator.Width / 2)),
                300, Easing.OutQuint);

            return base.OnMouseMove(e);
        }

        protected override void UpdateValue(float value)
        {
            fill.Width = value;

            if (mvisScreen.OverlaysHidden)
                songProgressIndicator.MoveToX(
                    getFinalPosX(songProgressIndicator, (value * UsableWidth) - (songProgressIndicator.Width / 2)),
                    300, Easing.OutQuint);
        }

        private bool indicatorsOverlaps;

        private bool overlap
        {
            set
            {
                if (value == indicatorsOverlaps) return;

                if (value)
                    songProgressIndicator.MoveToY(-(indicator.Height + 5), 300, Easing.OutQuint);
                else
                    songProgressIndicator.MoveToY(0, 300, Easing.OutQuint);

                indicatorsOverlaps = value;
            }
        }

        protected override void Update()
        {
            if (mvisScreen.OverlaysHidden)
            {
                var indicatorX = indicator.X - 5;
                var indicatorEnd = indicatorX + indicator.Width + 5;
                var songX = songProgressIndicator.X - 5;
                var songEnd = songX + songProgressIndicator.Width + 5;

                overlap = (indicatorX >= songX && indicatorX <= songEnd)
                          || (indicatorEnd >= songX && indicatorEnd <= songEnd);
            }

            base.Update();
        }

        private float getFinalPosX(Indicator target, float xPos)
        {
            //DrawWidth: 总宽度, target.Width: 指示器宽度, 5: 右侧Margin
            var rightMargin = DrawWidth - target.Width - 5;

            if (xPos > rightMargin) return rightMargin;
            //5: 左侧Margin
            if (xPos < 5) return 5;

            return xPos;
        }

        private void showIndicators()
        {
            if (!mvisScreen.OverlaysHidden) return;

            indicator.Show();
            songProgressIndicator.Show();
        }

        private void hideIndicators()
        {
            indicator.Hide();
            songProgressIndicator.Hide();
        }

        public SongProgressBar(bool allowSeek = true)
            : base(allowSeek)
        {
            fill.RelativeSizeAxes = Axes.Both;
        }
    }
}
