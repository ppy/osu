// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SpinnerMasks.Components
{
    public class SpinnerPiece : CompositeDrawable
    {
        private readonly Spinner spinner;
        private readonly CircularContainer circle;

        public SpinnerPiece(Spinner spinner)
        {
            this.spinner = spinner;

            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            Size = new Vector2(1.3f);

            RingPiece ring;
            InternalChildren = new Drawable[]
            {
                circle = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Alpha = 0.5f,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                },
                ring = new RingPiece
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };

            ring.Scale = new Vector2(spinner.Scale);

            spinner.PositionChanged += _ => updatePosition();
            spinner.StackHeightChanged += _ => updatePosition();
            spinner.ScaleChanged += _ => ring.Scale = new Vector2(spinner.Scale);

            updatePosition();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        private void updatePosition() => Position = spinner.Position;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => circle.ReceivePositionalInputAt(screenSpacePos);
    }
}
