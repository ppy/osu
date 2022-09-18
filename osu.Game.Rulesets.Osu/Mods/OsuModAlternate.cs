// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAlternate : InputBlockingMod
    {
        public override string Name => @"Alternate";
        public override string Acronym => @"AL";
        public override LocalisableString Description => @"Don't use the same key twice in a row!";
        public override IconUsage? Icon => FontAwesome.Solid.Keyboard;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModSingleTap) }).ToArray();

        [SettingSource("Notes per second for alternating", "The minimal number of notes per second that you want to be forced to alternate.")]
        public IBindable<int> AlternateNotesPerSecond { get; } = new NotesPerSecondSetting();

        protected override bool CheckValidNewAction(OsuAction action) => LastAcceptedAction != action || NotesPerSecond < AlternateNotesPerSecond.Value;
    }
}
