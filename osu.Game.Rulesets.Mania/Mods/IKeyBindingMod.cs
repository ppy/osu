// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Mods
{
    public interface IKeyBindingMod
    {
        /// <summary>
        /// The keybinding variant which this <see cref="IKeyBindingMod"/> requires.
        /// </summary>
        PlayfieldType Variant { get; }
    }
}
