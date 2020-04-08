// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Skinning
{
    internal static class CatchSkinExtensions
    {
        [NotNull]
        public static IBindable<Color4> GetHyperDashFruitColour(this ISkin skin)
            => skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDashFruit) ??
               skin.GetConfig<CatchSkinColour, Color4>(CatchSkinColour.HyperDash) ??
               new Bindable<Color4>(Catcher.DEFAULT_HYPER_DASH_COLOUR);
    }
}
