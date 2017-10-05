// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using OpenTK;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultyIcon : DifficultyColouredContainer
    {
        private readonly BeatmapInfo beatmap;

        public DifficultyIcon(BeatmapInfo beatmap) : base(beatmap)
        {
            this.beatmap = beatmap;
            Size = new Vector2(20);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = AccentColour,
                    Icon = FontAwesome.fa_circle
                },
                new ConstrainedIconContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Icon = beatmap.Ruleset.CreateInstance().CreateIcon()
                }
            };
        }
    }
}
