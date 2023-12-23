// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// This provides the ability to change the offset while in gameplay.
    /// Eventually this should be replaced with all settings from PlayerLoader being accessible from the game.
    /// </summary>
    internal partial class GameplayOffsetControl : VisibilityContainer
    {
        protected override bool StartHidden => true;

        public override bool PropagateNonPositionalInputSubTree => true;

        private BeatmapOffsetControl offsetControl = null!;

        private OsuTextFlowContainer text = null!;

        private ScheduledDelegate? hideOp;

        public GameplayOffsetControl()
        {
            AutoSizeAxes = Axes.Y;
            Width = SettingsToolboxGroup.CONTAINER_WIDTH;

            Masking = true;
            CornerRadius = 5;

            // Allow BeatmapOffsetControl to handle keyboard input.
            AlwaysPresent = true;

            Anchor = Anchor.CentreRight;
            Origin = Anchor.CentreRight;

            X = 100;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.8f,
                    Colour = colourProvider?.Background4 ?? Color4.Black,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Spacing = new Vector2(5),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        offsetControl = new BeatmapOffsetControl(),
                        text = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(weight: FontWeight.SemiBold))
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                        }
                    }
                },
            };

            offsetControl.Current.BindValueChanged(val =>
            {
                text.Text = BeatmapOffsetControl.GetOffsetExplanatoryText(val.NewValue);
                Show();

                hideOp?.Cancel();
                hideOp = Scheduler.AddDelayed(Hide, 500);
            });
        }

        protected override void PopIn()
        {
            this.FadeIn(500, Easing.OutQuint)
                .MoveToX(0, 500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.InQuint)
                .MoveToX(100, 500, Easing.InQuint);
        }
    }
}
