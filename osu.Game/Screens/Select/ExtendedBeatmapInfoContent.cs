// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class ExtendedBeatmapInfoContent : Container
    {
        public ExtendedBeatmapInfoContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                // TODO: add metadata column
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new ObjectCountContent
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                        new DifficultySettingsContent
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                    }
                },
                // TODO: add online stats column
            };
        }
    }
}
