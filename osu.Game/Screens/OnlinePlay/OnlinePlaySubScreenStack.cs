// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay
{
    public class OnlinePlaySubScreenStack : OsuScreenStack
    {
        public OnlinePlaySubScreenStack()
        {
            AddInternal(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.9f), Color4.Black.Opacity(0.6f))
            });
        }

        protected override void ScreenChanged(IScreen prev, IScreen next)
        {
            base.ScreenChanged(prev, next);

            // because this is a screen stack within a screen stack, let's manually handle disabled changes to simplify things.
            var osuScreen = ((OsuScreen)next);

            bool disallowChanges = osuScreen.DisallowExternalBeatmapRulesetChanges;

            osuScreen.Beatmap.Disabled = disallowChanges;
            osuScreen.Ruleset.Disabled = disallowChanges;
            osuScreen.Mods.Disabled = disallowChanges;
        }
    }
}
