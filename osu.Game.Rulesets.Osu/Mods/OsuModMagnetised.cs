// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModMagnetised : OsuEaseHitObjectPositionsMod
    {
        public override string Name => "Magnetised";
        public override string Acronym => "MG";
        public override IconUsage? Icon => FontAwesome.Solid.Magnet;
        public override string Description => "No need to chase the circles – your cursor is a magnet!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModRelax), typeof(OsuModRepel) }).ToArray();

        protected override Vector2 DestinationVector => CursorPosition;

        [SettingSource("Attraction strength", "How strong the pull is.", 0)]
        public BindableFloat AttractionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        public OsuModMagnetised()
        {
            EasementStrength.BindTo(AttractionStrength);
        }
    }
}
