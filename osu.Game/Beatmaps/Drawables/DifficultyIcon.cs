// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

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
                    // the null coalesce here is only present to make unit tests work (ruleset dlls aren't copied correctly for testing at the moment)
                    Icon = beatmap.Ruleset?.CreateInstance().CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.fa_question_circle_o }
                }
            };
        }
    }
}
