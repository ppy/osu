// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Represents a particular area or part of a game screen whose layout can be customised using the skin editor.
    /// </summary>
    public enum GlobalSkinnableContainers
    {
        [LocalisableDescription(typeof(SkinEditorStrings), nameof(SkinEditorStrings.HUD))]
        MainHUDComponents,

        [LocalisableDescription(typeof(SkinEditorStrings), nameof(SkinEditorStrings.SongSelect))]
        SongSelect,

        [LocalisableDescription(typeof(SkinEditorStrings), nameof(SkinEditorStrings.Playfield))]
        Playfield
    }
}
