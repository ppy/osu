// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Footer;
using osu.Game.Users;

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
        /// Whether the user can exit this <see cref="IOsuScreen"/>.
        /// </summary>
        /// <remarks>
        /// When overriden to <c>false</c>,
        /// the user is blocked from exiting the screen via the <see cref="GlobalAction.Back"/> action,
        /// and the back button is hidden from this screen by the initial state of <see cref="BackButtonVisibility"/> being set to hidden.
        /// </remarks>
        bool AllowUserExit { get; }

        /// <summary>
        /// Whether a footer (and a back button) should be displayed underneath the screen.
        /// </summary>
        /// <remarks>
        /// Temporarily, the footer's own back button is shown regardless of whether <see cref="BackButtonVisibility"/> is set to hidden.
        /// This will be corrected as the footer becomes used more commonly.
        /// </remarks>
        bool ShowFooter { get; }

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
        /// Whether the menu cursor should be hidden when non-mouse input is received.
        /// </summary>
        bool HideMenuCursorOnNonMouseInput { get; }

        /// <summary>
        /// On mobile phones, this specifies whether this <see cref="OsuScreen"/> requires the device to be in portrait orientation.
        /// Tablet devices are unaffected by this property.
        /// </summary>
        /// <remarks>
        /// By default, all screens in the game display in landscape orientation on phones.
        /// Setting this to <c>true</c> will display this screen in portrait orientation instead,
        /// and switch back to landscape when transitioning back to a regular non-portrait screen.
        /// </remarks>
        bool RequiresPortraitOrientation { get; }

        /// <summary>
        /// Whether overlays should be able to be opened when this screen is current.
        /// </summary>
        IBindable<OverlayActivation> OverlayActivationMode { get; }

        /// <summary>
        /// Whether the back button should be displayed in this screen.
        /// </summary>
        IBindable<bool> BackButtonVisibility { get; }

        /// <summary>
        /// The current <see cref="UserActivity"/> for this screen.
        /// </summary>
        Bindable<UserActivity> Activity { get; }

        /// <summary>
        /// The amount of parallax to be applied while this screen is displayed.
        /// </summary>
        float BackgroundParallaxAmount { get; }

        Bindable<WorkingBeatmap> Beatmap { get; }

        Bindable<RulesetInfo> Ruleset { get; }

        /// <summary>
        /// A list of footer buttons to be added to the game footer, or empty to display no buttons.
        /// </summary>
        IReadOnlyList<ScreenFooterButton> CreateFooterButtons();

        /// <summary>
        /// Whether mod track adjustments should be applied on entering this screen.
        /// A <see langword="null"/> value means that the parent screen's value of this setting will be used.
        /// </summary>
        bool? ApplyModTrackAdjustments { get; }

        /// <summary>
        /// Whether control of the global track should be allowed via the music controller / now playing overlay.
        /// A <see langword="null"/> value means that the parent screen's value of this setting will be used.
        /// </summary>
        bool? AllowGlobalTrackControl { get; }

        /// <summary>
        /// Invoked when the back button has been pressed to close any overlays before exiting this <see cref="IOsuScreen"/>.
        /// </summary>
        /// <remarks>
        /// If this <see cref="IOsuScreen"/> has not yet finished loading, the exit will occur immediately without this method being invoked.
        /// <para>
        /// Return <c>true</c> to block this <see cref="IOsuScreen"/> from being exited after closing an overlay.
        /// Return <c>false</c> if this <see cref="IOsuScreen"/> should continue exiting.
        /// </para>
        /// </remarks>
        bool OnBackButton();
    }
}
