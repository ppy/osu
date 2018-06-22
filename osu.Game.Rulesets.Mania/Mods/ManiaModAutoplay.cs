// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModAutoplay : ModAutoplay<ManiaHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<ManiaHitObject> beatmap)
        {
            return new Score
            {
                User = new User { Username = "osu!topus!" },
                Replay = new ManiaAutoGenerator((ManiaBeatmap)beatmap).Generate(),
            };
        }
    }
}
