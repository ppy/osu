// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Replays;

namespace osu.Game.Rulesets.UI
{
    public partial class AnalysisContainer : Container
    {
        protected Replay Replay;

        public AnalysisContainer(Replay replay)
        {
            Replay = replay;
        }
    }
}
