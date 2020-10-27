// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    public class FrameStableClock : GameplayClock
    {
        public GameplayClock ParentGameplayClock;

        public override IEnumerable<Bindable<double>> NonGameplayAdjustments => ParentGameplayClock?.NonGameplayAdjustments ?? Enumerable.Empty<Bindable<double>>();

        public FrameStableClock(FramedClock underlyingClock)
            : base(underlyingClock)
        {
        }

        public override bool ShouldDisableSamplePlayback =>
            // handle the case where playback is catching up to real-time.
            base.ShouldDisableSamplePlayback || (ParentGameplayClock != null && Math.Abs(CurrentTime - ParentGameplayClock.CurrentTime) > 200);
    }
}
