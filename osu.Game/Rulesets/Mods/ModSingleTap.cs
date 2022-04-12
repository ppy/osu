// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModSingleTap<TObject, TAction> : ModInputBlocker<TObject, TAction>
        where TObject : HitObject
        where TAction : struct
    {
        public override string Name => "Single Tap";
        public override string Acronym => "SL";
        public override string Description => @"Alternate tapping!";
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModAlternate<TObject, TAction>) };
    }
}
