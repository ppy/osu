// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModCipher : Mod
    {
        public override string Name => "Cipher";
        public override string Acronym => "CIP";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => OsuIcon.PlayStyleKeyboard;
        public override double ScoreMultiplier => 1;

        [SettingSource("Plaintext", "Enter text to encode", SettingControlType = typeof(SettingsTextBox))]
        public Bindable<string> Plaintext { get; } = new Bindable<string>("Test String");

    }
}
