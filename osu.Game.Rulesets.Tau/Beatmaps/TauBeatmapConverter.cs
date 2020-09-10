using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Beatmaps
{
    public class TauBeatmapConverter : BeatmapConverter<TauHitObject>
    {
        public override bool CanConvert() => true;

        public TauBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        protected override IEnumerable<TauHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            var position = ((IHasPosition)original).Position;
            var comboData = original as IHasCombo;
            bool isHard = (original is IHasPathWithRepeats tmp ? tmp.NodeSamples[0] : original.Samples).Any(s => s.Name == HitSampleInfo.HIT_FINISH);

            switch (original)
            {
                default:
                    if (isHard)
                        return new HardBeat
                        {
                            Samples = original is IHasPathWithRepeats curve ? curve.NodeSamples[0] : original.Samples,
                            StartTime = original.StartTime,
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                        }.Yield();
                    else
                        return new Beat
                        {
                            Samples = original is IHasPathWithRepeats curve ? curve.NodeSamples[0] : original.Samples,
                            StartTime = original.StartTime,
                            Angle = position.GetHitObjectAngle(),
                            NewCombo = comboData?.NewCombo ?? false,
                            ComboOffset = comboData?.ComboOffset ?? 0,
                        }.Yield();
            }
        }

        protected override Beatmap<TauHitObject> CreateBeatmap() => new TauBeatmap();
    }
}
