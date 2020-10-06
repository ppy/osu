// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class DifficultyPointPiece : CompositeDrawable
    {
        private readonly DifficultyControlPoint difficultyPoint;

        private OsuSpriteText speedMultiplierText;
        private readonly BindableNumber<double> speedMultiplier;

        public DifficultyPointPiece(DifficultyControlPoint difficultyPoint)
        {
            this.difficultyPoint = difficultyPoint;
            speedMultiplier = difficultyPoint.SpeedMultiplierBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Color4 colour = difficultyPoint.GetRepresentingColour(colours);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colour,
                    Width = 2,
                    RelativeSizeAxes = Axes.Y,
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colour,
                            RelativeSizeAxes = Axes.Both,
                        },
                        speedMultiplierText = new OsuSpriteText
                        {
                            Font = OsuFont.Default.With(weight: FontWeight.Bold),
                            Colour = Color4.White,
                        }
                    }
                },
            };

            speedMultiplier.BindValueChanged(multiplier => speedMultiplierText.Text = $"{multiplier.NewValue:n2}x", true);
        }
    }
}
