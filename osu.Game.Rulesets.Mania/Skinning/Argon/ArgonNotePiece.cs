// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;
using static osu.Game.Rulesets.Mania.Skinning.Argon.ArgonSnapColouring;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    internal partial class ArgonNotePiece : CompositeDrawable
    {
        public const float NOTE_HEIGHT = 42;
        public const float NOTE_ACCENT_RATIO = 0.82f;
        public const float CORNER_RADIUS = 3.4f;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();

        private readonly Box colouredBox;

        public ArgonNotePiece()
        {
            RelativeSizeAxes = Axes.X;
            Height = NOTE_HEIGHT;

            CornerRadius = CORNER_RADIUS;
            Masking = true;

            InternalChildren = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Colour4.Black)
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = NOTE_ACCENT_RATIO,
                    Masking = true,
                    CornerRadius = CORNER_RADIUS,
                    Children = new Drawable[]
                    {
                        colouredBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                },
                new Circle
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = CORNER_RADIUS * 2,
                },
                CreateIcon(),
            };
        }

        protected virtual Drawable CreateIcon() => new SpriteIcon
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Y = 4,
            // TODO: replace with a non-squashed version.
            // The 0.7f height scale should be removed.
            Icon = FontAwesome.Solid.AngleDown,
            Size = new Vector2(20),
            Scale = new Vector2(1, 0.7f)
        };

        [BackgroundDependencyLoader(true)]
        private void load(IScrollingInfo scrollingInfo, DrawableHitObject? drawableObject)
        {
            direction.BindTo(scrollingInfo.Direction);
            direction.BindValueChanged(onDirectionChanged, true);

            if (drawableObject != null)
            {
                accentColour.BindTo(drawableObject.AccentColour);
                accentColour.BindValueChanged(_ => updateNoteAccent(), true);

                if (drawableObject is DrawableManiaHitObject maniaHitObject)
                {
                    snapDivisor.BindTo(maniaHitObject.SnapDivisor);
                    snapDivisor.BindValueChanged(_ => updateNoteAccent(), true);
                }
            }
        }

        [Resolved]
        private OsuColour? colours { get; set; }

        private readonly IBindable<int> snapDivisor = new Bindable<int>();

        private void updateNoteAccent()
        {
            Color4 colour = snapDivisor.Value == 0
                ? accentColour.Value
                : SnapColourFor(snapDivisor.Value, colours);

            colouredBox.Colour = ColourInfo.GradientVertical(
                colour.Lighten(0.1f),
                colour
            );
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            colouredBox.Anchor = colouredBox.Origin = direction.NewValue == ScrollingDirection.Up
                ? Anchor.TopCentre
                : Anchor.BottomCentre;

            Scale = new Vector2(1, direction.NewValue == ScrollingDirection.Up ? -1 : 1);
        }
    }
}
