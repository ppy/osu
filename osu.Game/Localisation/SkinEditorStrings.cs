// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        /// <summary>
        /// "Currently editing"
        /// </summary>
        public static LocalisableString CurrentlyEditing => new TranslatableString(getKey(@"currently_editing"), @"Currently editing");

        /// <summary>
        /// "All layout elements for layers in the current screen will be reset to defaults."
        /// </summary>
        public static LocalisableString RevertToDefaultDescription => new TranslatableString(getKey(@"revert_to_default_description"), @"All layout elements for layers in the current screen will be reset to defaults.");

        /// <summary>
        /// "Closest"
        /// </summary>
        public static LocalisableString Closest => new TranslatableString(getKey(@"closest"), @"Closest");

        /// <summary>
        /// "Anchor"
        /// </summary>
        public static LocalisableString Anchor => new TranslatableString(getKey(@"anchor"), @"Anchor");

        /// <summary>
        /// "Origin"
        /// </summary>
        public static LocalisableString Origin => new TranslatableString(getKey(@"origin"), @"Origin");

        /// <summary>
        /// "Reset position"
        /// </summary>
        public static LocalisableString ResetPosition => new TranslatableString(getKey(@"reset_position"), @"Reset position");

        /// <summary>
        /// "Reset rotation"
        /// </summary>
        public static LocalisableString ResetRotation => new TranslatableString(getKey(@"reset_rotation"), @"Reset rotation");

        /// <summary>
        /// "Reset scale"
        /// </summary>
        public static LocalisableString ResetScale => new TranslatableString(getKey(@"reset_scale"), @"Reset scale");

        /// <summary>
        /// "Bring to front"
        /// </summary>
        public static LocalisableString BringToFront => new TranslatableString(getKey(@"bring_to_front"), @"Bring to front");

        /// <summary>
        /// "Send to back"
        /// </summary>
        public static LocalisableString SendToBack => new TranslatableString(getKey(@"send_to_back"), @"Send to back");

        /// <summary>
        /// "Current working layer"
        /// </summary>
        public static LocalisableString CurrentWorkingLayer => new TranslatableString(getKey(@"current_working_layer"), @"Current working layer");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
