// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{

    public abstract class ModAlternate<TObject, TAction> : ModInputBlocker<TObject, TAction>
        where TObject : HitObject
        where TAction : struct
    {
        public override string Name => "Alternate";
        public override string Acronym => "AL";
        public override string Description => @"Don't use the same key twice in a row!";
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModSingleTap<TObject, TAction>) };
    }
}
