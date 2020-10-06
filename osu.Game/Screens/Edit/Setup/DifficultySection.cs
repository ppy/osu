// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DifficultySection : SetupSection
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = "Difficulty settings"
                },
                new LabelledSlider()
            };
        }
    }
}
