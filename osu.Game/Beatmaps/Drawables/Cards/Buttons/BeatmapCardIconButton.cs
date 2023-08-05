// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public abstract partial class BeatmapCardIconButton : OsuClickableContainer
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

        protected override Container<Drawable> Content => content;

        private readonly Container content;
        private readonly Box hover;

        protected BeatmapCardIconButton()
        {
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;

            base.Content.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = BeatmapCard.CORNER_RADIUS,
                Scale = new Vector2(0.8f),
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                    },
                    Icon = new SpriteIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Scale = new Vector2(1.2f),
                    },
                }
            });

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
            bool isHovered = IsHovered && Enabled.Value;

            hover.FadeTo(isHovered ? 1f : 0f, 500, Easing.OutQuint);
            content.ScaleTo(isHovered ? 1 : 0.8f, 500, Easing.OutQuint);
            Icon.FadeColour(isHovered ? HoverColour : IdleColour, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
