// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : InputBlockingMod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't use the same key twice in a row!";
        public override IconUsage? Icon => OsuIcon.ModAlternate;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModSingleTap) }).ToArray();

        protected override bool CheckValidNewAction(OsuAction action) => LastAcceptedAction != action;
    }
}
