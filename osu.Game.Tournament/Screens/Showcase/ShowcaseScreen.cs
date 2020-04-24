// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Tournament.Components;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class ShowcaseScreen : BeatmapInfoScreen, IProvideVideo
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new TournamentLogo(),
                new TourneyVideo("showcase")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    // chroma key area for stable gameplay
                    Name = "chroma",
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Height = 695,
                    Width = 1366,
                    Colour = new Color4(0, 255, 0, 255),
                }
            });
        }
    }
}
