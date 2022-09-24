// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModEasy : ModEasyWithExtraLives
    {
        public override LocalisableString Description => @"更大的水果,更少的扣血,更低的准确率要求,并且拥有额外生命!";
    }
}
