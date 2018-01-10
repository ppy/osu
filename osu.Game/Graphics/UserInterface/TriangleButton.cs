// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A button with moving triangles in the background.
    /// </summary>
    public class TriangleButton : OsuButton, IFilterable
    {
        private Box hover;

        protected Triangles Triangles;

        public TriangleButton()
        {
            Height = 40;
        }

        protected override SpriteText CreateText() => new OsuSpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = @"Exo2.0-Bold",
        };

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.BlueDark;

            Content.Masking = true;
            Content.CornerRadius = 5;

            AddRange(new Drawable[]
            {
                Triangles = new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourDark = colours.BlueDarker,
                    ColourLight = colours.Blue,
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingMode.Additive,
                    Colour = Color4.White.Opacity(0.1f),
                    Alpha = 0,
                },
            });

            Enabled.ValueChanged += enabled_ValueChanged;
            Enabled.TriggerChange();
        }

        private void enabled_ValueChanged(bool enabled)
        {
            this.FadeColour(enabled ? Color4.White : Color4.Gray, 200, Easing.OutQuint);
        }

        protected override bool OnHover(InputState state)
        {
            hover.FadeIn(200);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            hover.FadeOut(200);
            base.OnHoverLost(state);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            Content.ScaleTo(0.9f, 4000, Easing.OutQuint);
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            return base.OnMouseUp(state, args);
        }

        public IEnumerable<string> FilterTerms => new[] { Text };

        public bool MatchingFilter
        {
            set
            {
                this.FadeTo(value ? 1 : 0);
            }
        }
    }
}
