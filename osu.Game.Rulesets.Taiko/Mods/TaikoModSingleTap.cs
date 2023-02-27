// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModSingleTap : TaikoInputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override LocalisableString Description => @"One key for dons, one key for kats.";

        protected override bool CheckValidNewAction(TaikoAction action) => LastAcceptedDonAction == null || LastAcceptedDonAction == action || LastAcceptedKatAction == null || LastAcceptedKatAction == action;
    }
}
