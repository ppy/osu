// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD.JudgementCounter
{
    public struct JudgementCounterInfo
    {
        public HitResult Type { get; set; }

        public BindableInt ResultCount { get; set; }
    }
}
