// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public abstract class BeatmapCardIconButton : OsuClickableContainer
    {
        private Colour4 idleColour;

        public Colour4 IdleColour
        {
            get => idleColour;
            set
            {
                idleColour = value;
                if (IsLoaded)
                    updateState();
            }
        }

        private Colour4 hoverColour;

        public Colour4 HoverColour
        {
            get => hoverColour;
            set
            {
                hoverColour = value;
                if (IsLoaded)
                    updateState();
            }
        }

        private float iconSize;

        public float IconSize
        {
            get => iconSize;
            set
            {
                iconSize = value;
                Icon.Size = new Vector2(iconSize);
            }
        }

        protected readonly SpriteIcon Icon;

        protected BeatmapCardIconButton()
        {
            Anchor = Origin = Anchor.Centre;

            Child = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Children = new Drawable[]
                {
                    Icon = new SpriteIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre
                    }
                }
            };

            Size = new Vector2(24);
            IconSize = 12;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            IdleColour = colourProvider.Light1;
            HoverColour = colourProvider.Content1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Enabled.BindValueChanged(_ => updateState(), true);
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        private void updateState()
        {
            Content.FadeColour(IsHovered && Enabled.Value ? HoverColour : IdleColour, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
