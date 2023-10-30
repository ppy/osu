// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public class ModTouchDevice : Mod, IApplicableMod
    {
        public sealed override string Name => "Touch Device";
        public sealed override string Acronym => "TD";
        public sealed override LocalisableString Description => "Automatically applied to plays on devices with a touchscreen.";
        public sealed override double ScoreMultiplier => 1;
        public sealed override ModType Type => ModType.System;
        public sealed override bool AlwaysValidForSubmission => true;
    }
}
