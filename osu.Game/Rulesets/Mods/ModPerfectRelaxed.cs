// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModPerfectRelaxed : Mod, IApplicableToHitObject, IApplicableToBeatmap
    {
        public override string Name => "Perfect Relaxed";
        public override string Acronym => "PFR";
        public override IconUsage? Icon => OsuIcon.ModPerfect;
        public override ModType Type => ModType.DifficultyIncrease;
        public override double ScoreMultiplier => 1;
        public override LocalisableString Description => "300 or miss.";
        public override bool Ranked => false;

        public void ApplyToHitObject(HitObject ho) {
/*
			ho.HitWindows = new HitWindowsPerfect();
			ho.HitWindows.SetDifficulty(6);
*/
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
		{
			BeatmapDifficulty bd = beatmap.Difficulty;

			foreach(var ho in beatmap.HitObjects){
				ho.HitWindows = new HitWindowsPerfect(bd);

				foreach(var nested_ho in ho.NestedHitObjects) {
					nested_ho.HitWindows = new HitWindowsPerfect(bd);
				}
			}
		}
    }

	class HitWindowsPerfect : HitWindows {
		public HitWindowsPerfect(BeatmapDifficulty bd) {
			this.SetDifficulty(bd.OverallDifficulty);
		}

        public override bool IsHitResultAllowed(HitResult result) {
			return result == HitResult.Great || result == HitResult.Miss;
		}
	}
}

