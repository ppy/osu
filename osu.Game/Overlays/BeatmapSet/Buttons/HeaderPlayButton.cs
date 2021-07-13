// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Overlays.BeatmapListing.Panels;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class HeaderPlayButton : PlayButton
    {
        private Box background, progress;

        public HeaderPlayButton()
        {
            Height = 42;
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Child = progress = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0f,
                        Alpha = 0f,
                    },
                },
                base.CreateContent().With(c =>
                {
                    c.Anchor = Anchor.Centre;
                    c.Origin = Anchor.Centre;
                    c.RelativeSizeAxes = Axes.None;
                    c.Size = new Vector2(18);
                })
            }
        };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            progress.Colour = colours.Yellow;
            background.Colour = colourProvider.Background6;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playing.BindValueChanged(playing =>
            {
                progress.FadeTo(playing.NewValue ? 1 : 0, 100);
            }, true);

            ShouldDisplay.BindValueChanged(_ => updateDisplay(), true);
        }

        protected override void Update()
        {
            base.Update();

            if (Playing.Value && Preview != null)
            {
                // prevent negative (potential infinite) width if a track without length was loaded
                progress.Width = Preview.Length > 0 ? (float)(Preview.CurrentTime / Preview.Length) : 0f;
            }
            else
                progress.Width = 0;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
            base.OnHoverLost(e);
        }

        private void updateDisplay() => background.FadeTo(IsHovered && !ShouldDisplay.Value ? 0.75f : 0.5f, 80);
    }
}
