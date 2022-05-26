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
    internal class OsuModRepel : OsuEaseHitObjectPositionsMod
    {
        public override string Name => "Repel";
        public override string Acronym => "RP";
        public override IconUsage? Icon => FontAwesome.Solid.ExpandArrowsAlt;
        public override string Description => "Run away!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(OsuModMagnetised)).ToArray();

        [SettingSource("Repulsion strength", "How strong the repulsion is.", 0)]
        public BindableFloat RepulsionStrength { get; } = new BindableFloat(0.5f)
        {
            Precision = 0.05f,
            MinValue = 0.05f,
            MaxValue = 1.0f,
        };

        protected override Vector2 DestinationVector => new Vector2(
            2 * WorkingHitObject.X - CursorPosition.X,
            2 * WorkingHitObject.Y - CursorPosition.Y
        );

        public OsuModRepel()
        {
            EasementStrength.BindTo(RepulsionStrength);
        }
    }
}
