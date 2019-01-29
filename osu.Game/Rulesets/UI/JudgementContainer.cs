// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.UI
{
    public class JudgementContainer<T> : Container<T>
        where T : DrawableJudgement
    {
        public override void Add(T judgement)
        {
            if (judgement == null) throw new ArgumentNullException(nameof(judgement));

            // remove any existing judgements for the judged object.
            // this can be the case when rewinding.
            RemoveAll(c => c.JudgedObject == judgement.JudgedObject);

            base.Add(judgement);
        }
    }
}
