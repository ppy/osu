﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SkinEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinEditor";

        /// <summary>
        /// "Skin editor"
        /// </summary>
        public static LocalisableString SkinEditor => new TranslatableString(getKey(@"skin_editor"), @"Skin editor");

        /// <summary>
        /// "Components"
        /// </summary>
        public static LocalisableString Components => new TranslatableString(getKey(@"components"), @"Components");

        /// <summary>
        /// "Scene library"
        /// </summary>
        public static LocalisableString SceneLibrary => new TranslatableString(getKey(@"scene_library"), @"Scene library");

        /// <summary>
        /// "Song Select"
        /// </summary>
        public static LocalisableString SongSelect => new TranslatableString(getKey(@"song_select"), @"Song Select");

        /// <summary>
        /// "Gameplay"
        /// </summary>
        public static LocalisableString Gameplay => new TranslatableString(getKey(@"gameplay"), @"Gameplay");

        /// <summary>
        /// "Settings ({0})"
        /// </summary>
        public static LocalisableString Settings(string arg0) => new TranslatableString(getKey(@"settings"), @"Settings ({0})", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
