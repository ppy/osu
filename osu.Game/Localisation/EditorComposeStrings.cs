// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorComposeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorCompose";

        /// <summary>
        /// "New combo"
        /// </summary>
        public static LocalisableString NewComboButtonTitle => new TranslatableString(getKey(@"new_combo_button_title"), @"New combo");

        /// <summary>
        /// "Grid snap"
        /// </summary>
        public static LocalisableString GridSnapButtonTitle => new TranslatableString(getKey(@"grid_snap_button_title"), @"Grid snap");

        /// <summary>
        /// "Distance Snap"
        /// </summary>
        public static LocalisableString DistanceSnapButtonTitle => new TranslatableString(getKey(@"distance_snap_button_title"), @"Distance Snap");

        /// <summary>
        /// "Normal"
        /// </summary>
        public static LocalisableString BankNormalText => new TranslatableString(getKey(@"bank_normal_text"), @"Normal");

        /// <summary>
        /// "Addition"
        /// </summary>
        public static LocalisableString BankAdditionText => new TranslatableString(getKey(@"bank_addition_text"), @"Addition");

        /// <summary>
        /// "Distance spacing"
        /// </summary>
        public static LocalisableString SnappingExpandedLabelText => new TranslatableString(getKey(@"snapping_expanded_label_text"), @"Distance spacing");

        /// <summary>
        /// "X offset"
        /// </summary>
        public static LocalisableString GridXOffset => new TranslatableString(getKey(@"grid_x_offset"), @"X offset");

        /// <summary>
        /// "Y offset"
        /// </summary>
        public static LocalisableString GridYOffset => new TranslatableString(getKey(@"grid_y_offset"), @"Y offset");

        /// <summary>
        /// "Spacing"
        /// </summary>
        public static LocalisableString GridSpacing => new TranslatableString(getKey(@"grid_spacing"), @"Spacing");

        /// <summary>
        /// "Rotation"
        /// </summary>
        public static LocalisableString GridRotation => new TranslatableString(getKey(@"grid_rotation"), @"Rotation");

        /// <summary>
        /// "Move"
        /// </summary>
        public static LocalisableString TransformMove => new TranslatableString(getKey(@"transform_move"), @"Move");

        /// <summary>
        /// "Rotate"
        /// </summary>
        public static LocalisableString TransformRotate => new TranslatableString(getKey(@"transform_rotate"), @"Rotate");

        /// <summary>
        /// "Scale"
        /// </summary>
        public static LocalisableString TransformScale => new TranslatableString(getKey(@"transform_scale"), @"Scale");

        /// <summary>
        /// "Polygon"
        /// </summary>
        public static LocalisableString GeneratePolygon => new TranslatableString(getKey(@"generate_polygon"), @"Polygon");

        /// <summary>
        /// "Distance snap"
        /// </summary>
        public static LocalisableString GenerateDistanceSnap => new TranslatableString(getKey(@"generate_distance_snap"), @"Distance snap");

        /// <summary>
        /// "Offset angle"
        /// </summary>
        public static LocalisableString GenerateOffsetAngle => new TranslatableString(getKey(@"generate_offset_angle"), @"Offset angle");

        /// <summary>
        /// "Repeats"
        /// </summary>
        public static LocalisableString GenerateRepeatCount => new TranslatableString(getKey(@"generate_repeat_count"), @"Repeats");

        /// <summary>
        /// "Vertices"
        /// </summary>
        public static LocalisableString GeneratePoint => new TranslatableString(getKey(@"generate_point"), @"Vertices");

        /// <summary>
        /// "Create"
        /// </summary>
        public static LocalisableString GenerateCreate => new TranslatableString(getKey(@"generate_create"), @"Create");

        /// <summary>
        /// "Control point spacing"
        /// </summary>
        public static LocalisableString FreehandSliderTolerance => new TranslatableString(getKey(@"freehand_slider_display_tolerance"), @"Control point spacing");

        /// <summary>
        /// "Corner bias"
        /// </summary>
        public static LocalisableString FreehandSliderCornerThreshold => new TranslatableString(getKey(@"freehand_slider_corner_threshold"), @"Corner bias");

        /// <summary>
        /// "Perfect curve bias"
        /// </summary>
        public static LocalisableString FreehandSliderCircleThreshold => new TranslatableString(getKey(@"freehand_slider_circle_threshold"), @"Perfect curve bias");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
