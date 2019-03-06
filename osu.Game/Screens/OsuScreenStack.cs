// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Screens
{
    public class OsuScreenStack : ScreenStack
    {
        public OsuScreenStack()
        {
            ScreenExited += unbindAllDependencies;
        }

        public OsuScreenStack(IScreen baseScreen)
            : base(baseScreen)
        {
            ScreenExited += unbindAllDependencies;
        }


        /// <summary> Return all child dependency bindables created by the exiting screen </summary>
        /// <remarks>
        /// While all bindables will eventually be returned by disposal logic,
        /// Bindables that are created by every OsuScreen, namely ones created in <see cref="OsuScreenDependencies"/>, will not be returned in time.
        /// We need to return them manually after OnExiting runs to ensure a new instance of the same screen can use these bindables immediately.
        /// </remarks>>
        private void unbindAllDependencies(IScreen prev, IScreen next)
        {
            var beatmap = (prev as OsuScreen)?.Beatmap;
            var ruleset = (prev as OsuScreen)?.Ruleset;

            (beatmap as LeasedBindable<WorkingBeatmap>)?.UnbindAll();
            (ruleset as LeasedBindable<RulesetInfo>)?.UnbindAll();
        }
    }
}
