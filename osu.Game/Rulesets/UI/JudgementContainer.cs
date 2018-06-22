// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
