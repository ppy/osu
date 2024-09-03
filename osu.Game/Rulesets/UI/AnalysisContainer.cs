// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Replays;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.UI
{
    public abstract partial class AnalysisContainer : Container
    {
        protected Replay Replay;
        protected Playfield Playfield;

        public AnalysisSettings AnalysisSettings;

        public AnalysisContainer(Replay replay, Playfield playfield)
        {
            Replay = replay;
            Playfield = playfield;

            AnalysisSettings = CreateAnalysisSettings();
        }

        protected abstract AnalysisSettings CreateAnalysisSettings();
    }
}
