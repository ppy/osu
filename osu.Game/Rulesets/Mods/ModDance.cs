using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Replays;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDance : Mod, ICreateReplayData, IApplicableFailOverride
    {
        public override Type[] IncompatibleMods => new[]
        {
            typeof(ModRelax),
            typeof(ModFailCondition),
            typeof(ModNoFail),
            typeof(ModAutoplay)
        };

        [SettingSource("保存Dance回放")]
        public Bindable<bool> SaveScore { get; } = new BindableBool();

        public override bool UserPlayable => false;

        public override ModType Type => ModType.Automation;

        public override bool RequiresConfiguration => true;

        //Copied from ModAutoplay.cs
        [Obsolete("Use the mod-supporting override")] // can be removed 20210731
        public virtual Score CreateReplayScore(IBeatmap beatmap) => new Score { Replay = new Replay() };

#pragma warning disable 618
        public virtual Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => CreateReplayScore(beatmap);
#pragma warning restore 618
        //Copy end

        public ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            var score = CreateReplayScore(beatmap, mods);
            return new ModReplayData(score.Replay, new ModCreatedUser{ Username = score.ScoreInfo.User.Username });
        }

        public bool PerformFail() => false;

        public bool RestartOnFail => false;
    }
}
