// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Replays;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModAutoplay : Mod, IApplicableFailOverride, ICreateReplayData
    {
        public override string Name => "Autoplay";
        public override string Acronym => "AT";
        public override IconUsage? Icon => OsuIcon.ModAuto;
        public override ModType Type => ModType.Automation;
        public override string Description => "Watch a perfect automated play through the song.";
        public override double ScoreMultiplier => 1;

        public bool PerformFail() => false;

        public bool RestartOnFail => false;

        public override bool UserPlayable => false;
        public override bool ValidForMultiplayer => false;
        public override bool ValidForMultiplayerAsFreeMod => false;

        public override Type[] IncompatibleMods => new[] { typeof(ModCinema), typeof(ModRelax), typeof(ModFailCondition), typeof(ModNoFail) };

        public override bool HasImplementation => GetType().GenericTypeArguments.Length == 0;

        [Obsolete("Override CreateReplayData(IBeatmap, IReadOnlyList<Mod>) instead")] // Can be removed 20220929
        public virtual Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score { Replay = new Replay() };

        public virtual ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
#pragma warning disable CS0618
            var replayScore = CreateReplayScore(beatmap, mods);
#pragma warning restore CS0618

            return new ModReplayData(replayScore.Replay, new ModCreatedUser { Username = replayScore.ScoreInfo.User.Username });
        }
    }
}
