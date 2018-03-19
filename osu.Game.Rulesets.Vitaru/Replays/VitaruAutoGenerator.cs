using osu.Game.Beatmaps;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Vitaru.Objects;
using osu.Game.Users;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Vitaru.Replays
{
    /*
    public class VitaruAutoGenerator : AutoGenerator<VitaruHitObject>
    {
        public VitaruAutoGenerator(Beatmap<VitaruHitObject> beatmap) : base(beatmap)
        {
            Replay = new Replay
            {
                User = new User
                {
                    Username = @"Autoplay",
                }
            };
        }

        protected Replay Replay;
        protected List<ReplayFrame> Frames => Replay.Frames;

        public override Replay Generate()
        {
            Frames.Add(new ReplayFrame(-100000, null, null, ReplayButtonState.None));
            Frames.Add(new ReplayFrame(Beatmap.HitObjects[0].StartTime - 1000, null, null, ReplayButtonState.None));

            for (int i = 0; i < Beatmap.HitObjects.Count; i++)
            {
                VitaruHitObject h = Beatmap.HitObjects[i];

                IHasEndTime endTimeData = h as IHasEndTime;

                double endTime = endTimeData?.EndTime ?? h.StartTime;

                Frames.Add(new ReplayFrame(endTime, null, null, ReplayButtonState.None));
            }

            return Replay;
        }
    }
    */
}
