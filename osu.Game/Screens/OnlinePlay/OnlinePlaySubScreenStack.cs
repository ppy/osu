// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Screens;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class OnlinePlaySubScreenStack : OsuScreenStack
    {
        protected override void ScreenChanged(IScreen prev, IScreen? next)
        {
            base.ScreenChanged(prev, next);

            // because this is a screen stack within a screen stack, let's manually handle disabled changes to simplify things.
            var osuScreen = next as OsuScreen;

            Debug.Assert(osuScreen != null);

            bool disallowChanges = osuScreen.DisallowExternalBeatmapRulesetChanges;

            osuScreen.Beatmap.Disabled = disallowChanges;
            osuScreen.Ruleset.Disabled = disallowChanges;
            osuScreen.Mods.Disabled = disallowChanges;
        }
    }
}
