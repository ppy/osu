// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModEasy : ModEasyWithExtraLives
    {
        public override LocalisableString Description => @"更大的圆圈,更少的扣血,更低的准确率要求,并且拥有额外生命!";
    }
}
