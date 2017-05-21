// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{

    internal class DifficultyIcon : DifficultyColouredContainer
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
            Children = new[]
            {
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = Size.X,
                    Colour = AccentColour,
                    Icon = FontAwesome.fa_circle
                },
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextSize = Size.X,
                    Colour = Color4.White,
                    Icon = beatmap.Ruleset.CreateInstance().Icon
                }
            };
        }
    }
}
