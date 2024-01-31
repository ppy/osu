// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CommonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Common";

        /// <summary>
        /// "Back"
        /// </summary>
        public static LocalisableString Back => new TranslatableString(getKey(@"back"), @"Back");

        /// <summary>
        /// "Next"
        /// </summary>
        public static LocalisableString Next => new TranslatableString(getKey(@"next"), @"Next");

        /// <summary>
        /// "Finish"
        /// </summary>
        public static LocalisableString Finish => new TranslatableString(getKey(@"finish"), @"Finish");

        /// <summary>
        /// "Enabled"
        /// </summary>
        public static LocalisableString Enabled => new TranslatableString(getKey(@"enabled"), @"Enabled");

        /// <summary>
        /// "Disabled"
        /// </summary>
        public static LocalisableString Disabled => new TranslatableString(getKey(@"disabled"), @"Disabled");

        /// <summary>
        /// "Default"
        /// </summary>
        public static LocalisableString Default => new TranslatableString(getKey(@"default"), @"Default");

        /// <summary>
        /// "Export"
        /// </summary>
        public static LocalisableString Export => new TranslatableString(getKey(@"export"), @"Export");

        /// <summary>
        /// "Width"
        /// </summary>
        public static LocalisableString Width => new TranslatableString(getKey(@"width"), @"Width");

        /// <summary>
        /// "Height"
        /// </summary>
        public static LocalisableString Height => new TranslatableString(getKey(@"height"), @"Height");

        /// <summary>
        /// "Downloading..."
        /// </summary>
        public static LocalisableString Downloading => new TranslatableString(getKey(@"downloading"), @"Downloading...");

        /// <summary>
        /// "Importing..."
        /// </summary>
        public static LocalisableString Importing => new TranslatableString(getKey(@"importing"), @"Importing...");

        /// <summary>
        /// "Deselect All"
        /// </summary>
        public static LocalisableString DeselectAll => new TranslatableString(getKey(@"deselect_all"), @"Deselect All");

        /// <summary>
        /// "Select All"
        /// </summary>
        public static LocalisableString SelectAll => new TranslatableString(getKey(@"select_all"), @"Select All");

        /// <summary>
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"beatmaps"), @"Beatmaps");

        /// <summary>
        /// "Scores"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"scores"), @"Scores");

        /// <summary>
        /// "Skins"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"skins"), @"Skins");

        /// <summary>
        /// "Collections"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Collections");

        /// <summary>
        /// "Mod presets"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"Mod presets");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString Name => new TranslatableString(getKey(@"name"), @"Name");

        /// <summary>
        /// "Description"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), @"Description");

        /// <summary>
        /// "File"
        /// </summary>
        public static LocalisableString MenuBarFile => new TranslatableString(getKey(@"menu_bar_file"), @"File");

        /// <summary>
        /// "Edit"
        /// </summary>
        public static LocalisableString MenuBarEdit => new TranslatableString(getKey(@"menu_bar_edit"), @"Edit");

        /// <summary>
        /// "View"
        /// </summary>
        public static LocalisableString MenuBarView => new TranslatableString(getKey(@"menu_bar_view"), @"View");

        /// <summary>
        /// "Undo"
        /// </summary>
        public static LocalisableString Undo => new TranslatableString(getKey(@"undo"), @"Undo");

        /// <summary>
        /// "Redo"
        /// </summary>
        public static LocalisableString Redo => new TranslatableString(getKey(@"redo"), @"Redo");

        /// <summary>
        /// "Cut"
        /// </summary>
        public static LocalisableString Cut => new TranslatableString(getKey(@"cut"), @"Cut");

        /// <summary>
        /// "Copy"
        /// </summary>
        public static LocalisableString Copy => new TranslatableString(getKey(@"copy"), @"Copy");

        /// <summary>
        /// "Paste"
        /// </summary>
        public static LocalisableString Paste => new TranslatableString(getKey(@"paste"), @"Paste");

        /// <summary>
        /// "Clone"
        /// </summary>
        public static LocalisableString Clone => new TranslatableString(getKey(@"clone"), @"Clone");

        /// <summary>
        /// "Exit"
        /// </summary>
        public static LocalisableString Exit => new TranslatableString(getKey(@"exit"), @"Exit");

        /// <summary>
        /// "Caps lock is active"
        /// </summary>
        public static LocalisableString CapsLockIsActive => new TranslatableString(getKey(@"caps_lock_is_active"), @"Caps lock is active");

        /// <summary>
        /// "Revert to default"
        /// </summary>
        public static LocalisableString RevertToDefault => new TranslatableString(getKey(@"revert_to_default"), @"Revert to default");

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString General => new TranslatableString(getKey(@"general"), @"General");

        /// <summary>
        /// "Do you wish to save your work?"
        /// </summary>
        public static LocalisableString DoYouWishToSave => new TranslatableString(getKey(@"do_you_wish_to_save"), @"Do you wish to save your work?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}