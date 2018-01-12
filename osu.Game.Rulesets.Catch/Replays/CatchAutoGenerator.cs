// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Users;

namespace osu.Game.Rulesets.Catch.Replays
{
    internal class CatchAutoGenerator : AutoGenerator<CatchHitObject>
    {
        public const double RELEASE_DELAY = 20;

        public CatchAutoGenerator(Beatmap<CatchHitObject> beatmap)
            : base(beatmap)
        {
            Replay = new Replay { User = new User { Username = @"Autoplay" } };
        }

        protected Replay Replay;

        public override Replay Generate()
        {
            // Todo: Realistically this shouldn't be needed, but the first frame is skipped with the way replays are currently handled
            Replay.Frames.Add(new CatchReplayFrame(-100000, 0));

            foreach (var obj in Beatmap.HitObjects)
            {
                switch (obj)
                {
                    case Fruit _:
                        Replay.Frames.Add(new CatchReplayFrame(obj.StartTime, obj.X));
                        break;
                }

                foreach (var nestedObj in obj.NestedHitObjects.Cast<CatchHitObject>())
                {
                    switch (nestedObj)
                    {
                        case BananaShower.Banana _:
                        case TinyDroplet _:
                        case Droplet _:
                            Replay.Frames.Add(new CatchReplayFrame(nestedObj.StartTime, nestedObj.X));
                            break;
                    }
                }
            }

            return Replay;
        }
    }
}
