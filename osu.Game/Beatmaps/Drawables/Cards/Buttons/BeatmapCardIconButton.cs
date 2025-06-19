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

        protected SpriteIcon Icon { get; private set; } = null!;

        private Container content = null!;
        private Container hover = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.Both;

            Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Scale = new Vector2(0.8f),
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {
                    hover = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        CornerRadius = BeatmapCard.CORNER_RADIUS,
                        Masking = true,
                        Colour = Color4.White.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                        Child = new Box { RelativeSizeAxes = Axes.Both, }
                    },
                    Icon = new SpriteIcon
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Size = new Vector2(14),
                    },
                }
            });

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
            content.ScaleTo(isHovered ? 0.9f : 0.8f, 500, Easing.OutQuint);
            Icon.FadeColour(isHovered ? HoverColour : IdleColour, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
        }

        protected void SetLoading(bool isLoading)
        {
            Icon.FadeTo(isLoading ? 0.2f : 1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            Enabled.Value = !isLoading;
        }
    }
}
