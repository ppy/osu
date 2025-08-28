// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : InputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override IconUsage? Icon => OsuIcon.ModSingleTap;
        public override LocalisableString Description => @"You must only use one key!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAlternate) }).ToArray();

        protected override bool CheckValidNewAction(OsuAction action) => LastAcceptedAction == null || LastAcceptedAction == action;
    }
}
