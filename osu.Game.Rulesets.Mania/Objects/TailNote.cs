// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class TailNote : Note
    {
        public TailNote(IBeatmap beatmap) : base(beatmap)
        {
        }

        public override Judgement CreateJudgement() => new ManiaJudgement();
    }
}
