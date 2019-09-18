// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables.Pieces
{
    /// <summary>
    /// Represents the static hit markers of notes.
    /// </summary>
    internal class NotePiece : Container, IHasAccentColour
    {
        public const float NOTE_HEIGHT = 12;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        private readonly Box colouredBox;

        public NotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = NOTE_HEIGHT;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                colouredBox = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = NOTE_HEIGHT / 2,
                    Alpha = 0.1f
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(dir =>
            {
                colouredBox.Anchor = colouredBox.Origin = dir.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
            }, true);
        }

        private Color4 accentColour;

        public Color4 AccentColour
        {
            get => accentColour;
            set
            {
                if (accentColour == value)
                    return;

                accentColour = value;

                colouredBox.Colour = AccentColour.Lighten(0.9f);
            }
        }
    }
}
