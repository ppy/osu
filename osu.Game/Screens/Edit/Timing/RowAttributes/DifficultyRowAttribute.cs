// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public class DifficultyRowAttribute : RowAttribute
    {
        private readonly BindableNumber<double> speedMultiplier;

        private OsuSpriteText text;

        public DifficultyRowAttribute(DifficultyControlPoint difficulty)
            : base(difficulty, "difficulty")
        {
            speedMultiplier = difficulty.SpeedMultiplierBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.AddRange(new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = 40,
                    Font = OsuFont.GetFont(size: EditorTable.TEXT_SIZE, weight: FontWeight.Regular),
                },
                new AttributeProgressBar(Point)
                {
                    Current = speedMultiplier,
                }
            });

            speedMultiplier.BindValueChanged(_ => updateText(), true);
        }

        private void updateText() => text.Text = $"{speedMultiplier.Value:n2}x";
    }
}
