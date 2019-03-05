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

        private void unbindAllDependencies(IScreen prev, IScreen next)
        {
            var beatmap = (prev as OsuScreen)?.Beatmap;
            var ruleset = (prev as OsuScreen)?.Ruleset;

            (beatmap as LeasedBindable<WorkingBeatmap>)?.UnbindAll();
            (ruleset as LeasedBindable<RulesetInfo>)?.UnbindAll();
        }
    }
}
