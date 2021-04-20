using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Misc;

namespace osu.Game.Screens.Mvis.BottomBar
{
    public class SongProgressBar : ProgressBar
    {
        private Drawable indicator;
        private Indicator songProgressIndicator;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        private InputManager inputManager;

        private const float idle_alpha = 0.5f;

        [BackgroundDependencyLoader]
        private void load()
        {
            Origin = Anchor.BottomCentre;
            Anchor = Anchor.BottomCentre;
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
            inputManager = GetContainingInputManager();

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
                calcFinalPosX(ToLocalSpace(e.ScreenSpaceMousePosition).X - indicator.Width / 2),
                300, Easing.OutQuint);

            return base.OnMouseMove(e);
        }

        protected override void UpdateValue(float value)
        {
            fill.Width = value;

            if (mvisScreen.OverlaysHidden)
            {
                songProgressIndicator.MoveToX(
                    calcFinalPosX(value * UsableWidth - songProgressIndicator.Width / 2),
                    300, Easing.OutQuint);
            }
        }

        private float calcFinalPosX(float xPos)
        {
            if (xPos > DrawWidth - 75) return DrawWidth - 75;
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
