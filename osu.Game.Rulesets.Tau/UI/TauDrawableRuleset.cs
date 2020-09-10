using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Tau.Objects;
using osu.Game.Rulesets.Tau.Objects.Drawables;
using osu.Game.Rulesets.Tau.Replays;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Tau.UI
{
    [Cached]
    public class DrawableTauRuleset : DrawableRuleset<TauHitObject>
    {
        public DrawableTauRuleset(TauRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new TauPlayfield(Beatmap.BeatmapInfo.BaseDifficulty);

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new TauFramedReplayInputHandler(replay);

        public override DrawableHitObject<TauHitObject> CreateDrawableRepresentation(TauHitObject h)
        {
            switch (h)
            {
                case HardBeat _:
                    return new DrawableHardBeat(h);

                case Beat beat:
                    return new DrawableBeat(beat);

                default:
                    return null;
            }
        }

        protected override PassThroughInputManager CreateInputManager() => new TauInputManager(Ruleset?.RulesetInfo);

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new TauPlayfieldAdjustmentContainer();

        protected override ReplayRecorder CreateReplayRecorder(Replay replay) => new TauReplayRecorder(replay);
    }
}
