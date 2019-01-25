﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Taiko.Replays
{
    public class TaikoReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public List<TaikoAction> Actions = new List<TaikoAction>();

        public TaikoReplayFrame()
        {
        }

        public TaikoReplayFrame(double time, params TaikoAction[] actions)
            : base(time)
        {
            Actions.AddRange(actions);
        }

        public void ConvertFrom(LegacyReplayFrame legacyFrame, IBeatmap beatmap)
        {
            if (legacyFrame.MouseRight1) Actions.Add(TaikoAction.LeftRim);
            if (legacyFrame.MouseRight2) Actions.Add(TaikoAction.RightRim);
            if (legacyFrame.MouseLeft1) Actions.Add(TaikoAction.LeftCentre);
            if (legacyFrame.MouseLeft2) Actions.Add(TaikoAction.RightCentre);
        }
    }
}
