// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class HitObjectPointPiece : CircularContainer
    {
        protected OsuSpriteText Label { get; private set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AutoSizeAxes = Axes.Both;

            Color4 colour = GetRepresentingColour(colours);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = 16,
                    Masking = true,
                    CornerRadius = 8,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colour,
                            RelativeSizeAxes = Axes.Both,
                        },
                        Label = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(5),
                            Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                            Colour = colours.B5,
                        }
                    }
                },
            };
        }

        protected virtual Color4 GetRepresentingColour(OsuColour colours)
        {
            return colours.Yellow;
        }
    }
}
