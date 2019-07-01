// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Screens
{
    public interface IOsuScreen : IScreen
    {
        /// <summary>
        /// Whether the beatmap or ruleset should be allowed to be changed by the user or game.
        /// Used to mark exclusive areas where this is strongly prohibited, like gameplay.
        /// </summary>
        bool DisallowExternalBeatmapRulesetChanges { get; }

        /// <summary>
        /// Whether the user can exit this this <see cref="IOsuScreen"/> by pressing the back button.
        /// </summary>
        bool AllowBackButton { get; }

        /// <summary>
        /// Whether a top-level component should be allowed to exit the current screen to, for example,
        /// complete an import. Note that this can be overridden by a user if they specifically request.
        /// </summary>
        bool AllowExternalScreenChange { get; }

        /// <summary>
        /// Whether this <see cref="OsuScreen"/> allows the cursor to be displayed.
        /// </summary>
        bool CursorVisible { get; }

        /// <summary>
        /// Whether all overlays should be hidden when this screen is entered or resumed.
        /// </summary>
        bool HideOverlaysOnEnter { get; }

        /// <summary>
        /// Whether overlays should be able to be opened once this screen is entered or resumed.
        /// </summary>
        OverlayActivation InitialOverlayActivationMode { get; }

        /// <summary>
        /// The amount of parallax to be applied while this screen is displayed.
        /// </summary>
        float BackgroundParallaxAmount { get; }

        Bindable<WorkingBeatmap> Beatmap { get; }

        Bindable<RulesetInfo> Ruleset { get; }
    }
}
