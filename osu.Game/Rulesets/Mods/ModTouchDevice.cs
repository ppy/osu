// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Localisation;

namespace osu.Game.Rulesets.Mods
{
    public class ModTouchDevice : Mod, IApplicableMod
    {
        public sealed override string Name => "Touch Device";
        public sealed override string Acronym => "TD";
        public sealed override IconUsage? Icon => OsuIcon.ModTouchDevice;
        public sealed override LocalisableString Description => ModSelectOverlayStrings.ModTouchDeviceDescription;
        public sealed override double ScoreMultiplier => 1;
        public sealed override ModType Type => ModType.System;
        public sealed override bool ValidForMultiplayer => false;
        public sealed override bool ValidForMultiplayerAsFreeMod => false;
        public sealed override bool AlwaysValidForSubmission => true;
        public override Type[] IncompatibleMods => new[] { typeof(ICreateReplayData) };
    }
}
