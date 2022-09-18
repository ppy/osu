// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : InputBlockingMod
    {
        public override string Name => @"Single Tap";
        public override string Acronym => @"SG";
        public override LocalisableString Description => @"You must only use one key!";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAlternate) }).ToArray();

        [SettingSource("Notes per second for single tap", "The maximum number of notes per second that you want to be forced to single tap.")]
        public IBindable<int> SingleTapNotesPerSecond { get; } = new NotesPerSecondSetting();

        protected override bool CheckValidNewAction(OsuAction action) =>
            LastAcceptedAction == null || LastAcceptedAction == action || (!SingleTapNotesPerSecond.IsDefault && NotesPerSecond > SingleTapNotesPerSecond.Value);
    }
}
