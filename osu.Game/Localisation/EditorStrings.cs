// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Editor";

        /// <summary>
        /// "Beatmap editor"
        /// </summary>
        public static LocalisableString BeatmapEditor => new TranslatableString(getKey(@"beatmap_editor"), @"Beatmap editor");

        /// <summary>
        /// "Waveform opacity"
        /// </summary>
        public static LocalisableString WaveformOpacity => new TranslatableString(getKey(@"waveform_opacity"), @"Waveform opacity");

        /// <summary>
        /// "Timeline objects opacity"
        /// </summary>
        public static LocalisableString TimelineObjectsOpacity => new TranslatableString(getKey(@"timeline_objects_opacity"), @"Timeline objects opacity");

        /// <summary>
        /// "Show hit markers"
        /// </summary>
        public static LocalisableString ShowHitMarkers => new TranslatableString(getKey(@"show_hit_markers"), @"Show hit markers");

        /// <summary>
        /// "Automatically seek after placing objects"
        /// </summary>
        public static LocalisableString AutoSeekOnPlacement => new TranslatableString(getKey(@"auto_seek_on_placement"), @"Automatically seek after placing objects");

        /// <summary>
        /// "Timing"
        /// </summary>
        public static LocalisableString Timing => new TranslatableString(getKey(@"timing"), @"Timing");

        /// <summary>
        /// "Set preview point to current time"
        /// </summary>
        public static LocalisableString SetPreviewPointToCurrent => new TranslatableString(getKey(@"set_preview_point_to_current"), @"Set preview point to current time");

        /// <summary>
        /// "Move already placed objects when changing timing"
        /// </summary>
        public static LocalisableString AdjustExistingObjectsOnTimingChanges => new TranslatableString(getKey(@"adjust_existing_objects_on_timing_changes"), @"Move already placed objects when changing timing");

        /// <summary>
        /// "For editing (.olz)"
        /// </summary>
        public static LocalisableString ExportForEditing => new TranslatableString(getKey(@"export_for_editing"), @"For editing (.olz)");

        /// <summary>
        /// "For compatibility (.osz)"
        /// </summary>
        public static LocalisableString ExportForCompatibility => new TranslatableString(getKey(@"export_for_compatibility"), @"For compatibility (.osz)");

        /// <summary>
        /// "Create new difficulty"
        /// </summary>
        public static LocalisableString CreateNewDifficulty => new TranslatableString(getKey(@"create_new_difficulty"), @"Create new difficulty");

        /// <summary>
        /// "Change difficulty"
        /// </summary>
        public static LocalisableString ChangeDifficulty => new TranslatableString(getKey(@"change_difficulty"), @"Change difficulty");

        /// <summary>
        /// "Delete difficulty"
        /// </summary>
        public static LocalisableString DeleteDifficulty => new TranslatableString(getKey(@"delete_difficulty"), @"Delete difficulty");

        /// <summary>
        /// "Edit externally"
        /// </summary>
        public static LocalisableString EditExternally => new TranslatableString(getKey(@"edit_externally"), @"Edit externally");

        /// <summary>
        /// "Submit beatmap"
        /// </summary>
        public static LocalisableString SubmitBeatmap => new TranslatableString(getKey(@"submit_beatmap"), @"Submit beatmap");

        /// <summary>
        /// "setup"
        /// </summary>
        public static LocalisableString SetupScreen => new TranslatableString(getKey(@"setup_screen"), @"setup");

        /// <summary>
        /// "compose"
        /// </summary>
        public static LocalisableString ComposeScreen => new TranslatableString(getKey(@"compose_screen"), @"compose");

        /// <summary>
        /// "design"
        /// </summary>
        public static LocalisableString DesignScreen => new TranslatableString(getKey(@"design_screen"), @"design");

        /// <summary>
        /// "timing"
        /// </summary>
        public static LocalisableString TimingScreen => new TranslatableString(getKey(@"timing_screen"), @"timing");

        /// <summary>
        /// "verify"
        /// </summary>
        public static LocalisableString VerifyScreen => new TranslatableString(getKey(@"verify_screen"), @"verify");

        /// <summary>
        /// "Playback speed"
        /// </summary>
        public static LocalisableString PlaybackSpeed => new TranslatableString(getKey(@"playback_speed"), @"Playback speed");

        /// <summary>
        /// "Test!"
        /// </summary>
        public static LocalisableString TestBeatmap => new TranslatableString(getKey(@"test_beatmap"), @"Test!");

        /// <summary>
        /// "{0:0}&#176;"
        /// </summary>
        public static LocalisableString RotationUnsnapped(float newRotation) => new TranslatableString(getKey(@"rotation_unsnapped"), @"{0:0}°", newRotation);

        /// <summary>
        /// "{0:0}&#176; (snapped)"
        /// </summary>
        public static LocalisableString RotationSnapped(float newRotation) => new TranslatableString(getKey(@"rotation_snapped"), @"{0:0}° (snapped)", newRotation);

        /// <summary>
        /// "Limit distance snap placement to current time"
        /// </summary>
        public static LocalisableString LimitedDistanceSnap => new TranslatableString(getKey(@"limited_distance_snap_grid"), @"Limit distance snap placement to current time");

        /// <summary>
        /// "Contract sidebars when not hovered"
        /// </summary>
        public static LocalisableString ContractSidebars => new TranslatableString(getKey(@"contract_sidebars"), @"Contract sidebars when not hovered");

        /// <summary>
        /// "Must be in edit mode to handle editor links"
        /// </summary>
        public static LocalisableString MustBeInEditorToHandleLinks => new TranslatableString(getKey(@"must_be_in_editor_to_handle_links"), @"Must be in edit mode to handle editor links");

        /// <summary>
        /// "Failed to parse editor link"
        /// </summary>
        public static LocalisableString FailedToParseEditorLink => new TranslatableString(getKey(@"failed_to_parse_edtior_link"), @"Failed to parse editor link");

        /// <summary>
        /// "Timeline"
        /// </summary>
        public static LocalisableString Timeline => new TranslatableString(getKey(@"timeline"), @"Timeline");

        /// <summary>
        /// "Show timing changes"
        /// </summary>
        public static LocalisableString TimelineShowTimingChanges => new TranslatableString(getKey(@"timeline_show_timing_changes"), @"Show timing changes");

        /// <summary>
        /// "Finish editing and import changes"
        /// </summary>
        public static LocalisableString FinishEditingExternally => new TranslatableString(getKey(@"Finish editing and import changes"), @"Finish editing and import changes");

        /// <summary>
        /// "Show breaks"
        /// </summary>
        public static LocalisableString TimelineShowBreaks => new TranslatableString(getKey(@"timeline_show_breaks"), @"Show breaks");

        /// <summary>
        /// "Show ticks"
        /// </summary>
        public static LocalisableString TimelineShowTicks => new TranslatableString(getKey(@"timeline_show_ticks"), @"Show ticks");

        /// <summary>
        /// "Bookmarks"
        /// </summary>
        public static LocalisableString Bookmarks => new TranslatableString(getKey(@"bookmarks"), @"Bookmarks");

        /// <summary>
        /// "Add bookmark"
        /// </summary>
        public static LocalisableString AddBookmark => new TranslatableString(getKey(@"add_bookmark"), @"Add bookmark");

        /// <summary>
        /// "Remove closest bookmark"
        /// </summary>
        public static LocalisableString RemoveClosestBookmark => new TranslatableString(getKey(@"remove_closest_bookmark"), @"Remove closest bookmark");

        /// <summary>
        /// "Seek to previous bookmark"
        /// </summary>
        public static LocalisableString SeekToPreviousBookmark => new TranslatableString(getKey(@"seek_to_previous_bookmark"), @"Seek to previous bookmark");

        /// <summary>
        /// "Seek to next bookmark"
        /// </summary>
        public static LocalisableString SeekToNextBookmark => new TranslatableString(getKey(@"seek_to_next_bookmark"), @"Seek to next bookmark");

        /// <summary>
        /// "Reset bookmarks"
        /// </summary>
        public static LocalisableString ResetBookmarks => new TranslatableString(getKey(@"reset_bookmarks"), @"Reset bookmarks");

        /// <summary>
        /// "Open beatmap info page"
        /// </summary>
        public static LocalisableString OpenInfoPage => new TranslatableString(getKey(@"open_info_page"), @"Open beatmap info page");

        /// <summary>
        /// "Open beatmap discussion page"
        /// </summary>
        public static LocalisableString OpenDiscussionPage => new TranslatableString(getKey(@"open_discussion_page"), @"Open beatmap discussion page");

        /// <summary>
        /// "Current difficulty"
        /// </summary>
        public static LocalisableString CheckCurrentDifficulty => new TranslatableString(getKey(@"check_current_difficulty"), @"Current difficulty");

        /// <summary>
        /// "Entire beatmap set"
        /// </summary>
        public static LocalisableString CheckEntireBeatmapSet => new TranslatableString(getKey(@"check_entire_beatmap_set"), @"Entire beatmap set");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
