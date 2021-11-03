// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class PreviewButton : OsuClickableContainer
    {
        private readonly Box background, progress;
        private readonly PlayButton playButton;

        private PreviewTrack preview => playButton.Preview;

        public IBindable<bool> Playing => playButton.Playing;

        public APIBeatmapSet BeatmapSet
        {
            get => playButton.BeatmapSet;
            set => playButton.BeatmapSet = value;
        }

        public PreviewButton()
        {
            Height = 42;

            Children = new Drawable[]
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
                playButton = new PlayButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(18),
                },
            };

            Action = () => playButton.TriggerClick();
            Playing.ValueChanged += playing => progress.FadeTo(playing.NewValue ? 1 : 0, 100);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            progress.Colour = colours.Yellow;
            background.Colour = colourProvider.Background6;
        }

        protected override void Update()
        {
            base.Update();

            if (Playing.Value && preview != null)
            {
                // prevent negative (potential infinite) width if a track without length was loaded
                progress.Width = preview.Length > 0 ? (float)(preview.CurrentTime / preview.Length) : 0f;
            }
            else
                progress.Width = 0;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeTo(0.75f, 80);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeTo(0.5f, 80);
            base.OnHoverLost(e);
        }
    }
}
