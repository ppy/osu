﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SliderRepeat : SliderEndCircle
    {
        public SliderRepeat(Slider slider)
            : base(slider)
        {
        }

        protected override HitObject CreateInstance() => new SliderRepeat(null!);
    }
}
