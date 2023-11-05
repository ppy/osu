// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Input.Bindings
{
    public partial class GlobalActionContainer : DatabasedKeyBindingContainer<GlobalAction>, IHandleGlobalKeyboardInput, IKeyBindingHandler<GlobalAction>
    {
        protected override bool Prioritised => true;

        private readonly IKeyBindingHandler<GlobalAction>? handler;

        public GlobalActionContainer(OsuGameBase? game)
            : base(matchingMode: KeyCombinationMatchingMode.Modifiers)
        {
            if (game is IKeyBindingHandler<GlobalAction> h)
                handler = h;
        }

        /// <summary>
        /// All default key bindings across all categories, ordered with highest priority first.
        /// </summary>
        /// <remarks>
        /// IMPORTANT: Take care when changing order of the items in the enumerable.
        /// It is used to decide the order of precedence, with the earlier items having higher precedence.
        /// </remarks>
        public override IEnumerable<IKeyBinding> DefaultKeyBindings => globalKeyBindings
                                                                       .Concat(editorKeyBindings)
                                                                       .Concat(inGameKeyBindings)
                                                                       .Concat(replayKeyBindings)
                                                                       .Concat(songSelectKeyBindings)
                                                                       .Concat(audioControlKeyBindings)
                                                                       // Overlay bindings may conflict with more local cases like the editor so they are checked last.
                                                                       // It has generally been agreed on that local screens like the editor should have priority,
                                                                       // based on such usages potentially requiring a lot more key bindings that may be "shared" with global ones.
                                                                       .Concat(overlayKeyBindings);

        public static IEnumerable<KeyBinding> GetDefaultBindingsFor(GlobalActionCategory category)
        {
            switch (category)
            {
                case GlobalActionCategory.General:
                    return globalKeyBindings;

                case GlobalActionCategory.Editor:
                    return editorKeyBindings;

                case GlobalActionCategory.InGame:
                    return inGameKeyBindings;

                case GlobalActionCategory.Replay:
                    return replayKeyBindings;

                case GlobalActionCategory.SongSelect:
                    return songSelectKeyBindings;

                case GlobalActionCategory.AudioControl:
                    return audioControlKeyBindings;

                case GlobalActionCategory.Overlays:
                    return overlayKeyBindings;

                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, $"Unexpected {nameof(GlobalActionCategory)}");
            }
        }

        public static IEnumerable<GlobalAction> GetGlobalActionsFor(GlobalActionCategory category)
            => GetDefaultBindingsFor(category).Select(binding => binding.Action).Cast<GlobalAction>().Distinct();

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e) => handler?.OnPressed(e) == true;

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) => handler?.OnReleased(e);

        private static IEnumerable<KeyBinding> globalKeyBindings => new[]
        {
            new KeyBinding(InputKey.Up, GlobalAction.SelectPrevious),
            new KeyBinding(InputKey.Down, GlobalAction.SelectNext),

            new KeyBinding(InputKey.Left, GlobalAction.SelectPreviousGroup),
            new KeyBinding(InputKey.Right, GlobalAction.SelectNextGroup),

            new KeyBinding(InputKey.Space, GlobalAction.Select),
            new KeyBinding(InputKey.Enter, GlobalAction.Select),
            new KeyBinding(InputKey.KeypadEnter, GlobalAction.Select),

            new KeyBinding(InputKey.Escape, GlobalAction.Back),
            new KeyBinding(InputKey.ExtraMouseButton1, GlobalAction.Back),

            new KeyBinding(new[] { InputKey.Alt, InputKey.Home }, GlobalAction.Home),

            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.F }, GlobalAction.ToggleFPSDisplay),
            new KeyBinding(new[] { InputKey.Control, InputKey.T }, GlobalAction.ToggleToolbar),
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.S }, GlobalAction.ToggleSkinEditor),
            new KeyBinding(new[] { InputKey.Control, InputKey.P }, GlobalAction.ToggleProfile),

            new KeyBinding(new[] { InputKey.Control, InputKey.Alt, InputKey.R }, GlobalAction.ResetInputSettings),

            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.R }, GlobalAction.RandomSkin),

            new KeyBinding(InputKey.F10, GlobalAction.ToggleGameplayMouseButtons),
            new KeyBinding(InputKey.F12, GlobalAction.TakeScreenshot),
        };

        private static IEnumerable<KeyBinding> overlayKeyBindings => new[]
        {
            new KeyBinding(InputKey.F8, GlobalAction.ToggleChat),
            new KeyBinding(InputKey.F6, GlobalAction.ToggleNowPlaying),
            new KeyBinding(InputKey.F9, GlobalAction.ToggleSocial),
            new KeyBinding(new[] { InputKey.Control, InputKey.B }, GlobalAction.ToggleBeatmapListing),
            new KeyBinding(new[] { InputKey.Control, InputKey.O }, GlobalAction.ToggleSettings),
            new KeyBinding(new[] { InputKey.Control, InputKey.N }, GlobalAction.ToggleNotifications),
        };

        private static IEnumerable<KeyBinding> editorKeyBindings => new[]
        {
            new KeyBinding(new[] { InputKey.F1 }, GlobalAction.EditorComposeMode),
            new KeyBinding(new[] { InputKey.F2 }, GlobalAction.EditorDesignMode),
            new KeyBinding(new[] { InputKey.F3 }, GlobalAction.EditorTimingMode),
            new KeyBinding(new[] { InputKey.F4 }, GlobalAction.EditorSetupMode),
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.A }, GlobalAction.EditorVerifyMode),
            new KeyBinding(new[] { InputKey.Control, InputKey.D }, GlobalAction.EditorCloneSelection),
            new KeyBinding(new[] { InputKey.J }, GlobalAction.EditorNudgeLeft),
            new KeyBinding(new[] { InputKey.K }, GlobalAction.EditorNudgeRight),
            new KeyBinding(new[] { InputKey.G }, GlobalAction.EditorCycleGridDisplayMode),
            new KeyBinding(new[] { InputKey.F5 }, GlobalAction.EditorTestGameplay),
            new KeyBinding(new[] { InputKey.T }, GlobalAction.EditorTapForBPM),
            new KeyBinding(new[] { InputKey.Control, InputKey.H }, GlobalAction.EditorFlipHorizontally),
            new KeyBinding(new[] { InputKey.Control, InputKey.J }, GlobalAction.EditorFlipVertically),
            new KeyBinding(new[] { InputKey.Control, InputKey.Alt, InputKey.MouseWheelDown }, GlobalAction.EditorDecreaseDistanceSpacing),
            new KeyBinding(new[] { InputKey.Control, InputKey.Alt, InputKey.MouseWheelUp }, GlobalAction.EditorIncreaseDistanceSpacing),
            // Framework automatically converts wheel up/down to left/right when shift is held.
            // See https://github.com/ppy/osu-framework/blob/master/osu.Framework/Input/StateChanges/MouseScrollRelativeInput.cs#L37-L38.
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.MouseWheelRight }, GlobalAction.EditorCyclePreviousBeatSnapDivisor),
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.MouseWheelLeft }, GlobalAction.EditorCycleNextBeatSnapDivisor),
            new KeyBinding(new[] { InputKey.Control, InputKey.R }, GlobalAction.EditorToggleRotateControl),
        };

        private static IEnumerable<KeyBinding> inGameKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.ExtraMouseButton2, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.Tilde, GlobalAction.QuickRetry),
            new KeyBinding(new[] { InputKey.Control, InputKey.R }, GlobalAction.QuickRetry),
            new KeyBinding(new[] { InputKey.Control, InputKey.Tilde }, GlobalAction.QuickExit),
            new KeyBinding(new[] { InputKey.F3 }, GlobalAction.DecreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.F4 }, GlobalAction.IncreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.Shift, InputKey.Tab }, GlobalAction.ToggleInGameInterface),
            new KeyBinding(InputKey.Tab, GlobalAction.ToggleInGameLeaderboard),
            new KeyBinding(InputKey.MouseMiddle, GlobalAction.PauseGameplay),
            new KeyBinding(InputKey.Control, GlobalAction.HoldForHUD),
            new KeyBinding(InputKey.Enter, GlobalAction.ToggleChatFocus),
            new KeyBinding(InputKey.F1, GlobalAction.SaveReplay),
            new KeyBinding(InputKey.F2, GlobalAction.ExportReplay),
        };

        private static IEnumerable<KeyBinding> replayKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.TogglePauseReplay),
            new KeyBinding(InputKey.MouseMiddle, GlobalAction.TogglePauseReplay),
            new KeyBinding(InputKey.Left, GlobalAction.SeekReplayBackward),
            new KeyBinding(InputKey.Right, GlobalAction.SeekReplayForward),
            new KeyBinding(new[] { InputKey.Control, InputKey.H }, GlobalAction.ToggleReplaySettings),
        };

        private static IEnumerable<KeyBinding> songSelectKeyBindings => new[]
        {
            new KeyBinding(InputKey.F1, GlobalAction.ToggleModSelection),
            new KeyBinding(InputKey.F2, GlobalAction.SelectNextRandom),
            new KeyBinding(new[] { InputKey.Shift, InputKey.F2 }, GlobalAction.SelectPreviousRandom),
            new KeyBinding(InputKey.F3, GlobalAction.ToggleBeatmapOptions),
            new KeyBinding(InputKey.BackSpace, GlobalAction.DeselectAllMods),
        };

        private static IEnumerable<KeyBinding> audioControlKeyBindings => new[]
        {
            new KeyBinding(new[] { InputKey.Alt, InputKey.Up }, GlobalAction.IncreaseVolume),
            new KeyBinding(new[] { InputKey.Alt, InputKey.Down }, GlobalAction.DecreaseVolume),

            new KeyBinding(new[] { InputKey.Alt, InputKey.Left }, GlobalAction.PreviousVolumeMeter),
            new KeyBinding(new[] { InputKey.Alt, InputKey.Right }, GlobalAction.NextVolumeMeter),

            new KeyBinding(new[] { InputKey.Control, InputKey.F4 }, GlobalAction.ToggleMute),

            new KeyBinding(InputKey.TrackPrevious, GlobalAction.MusicPrev),
            new KeyBinding(InputKey.F1, GlobalAction.MusicPrev),
            new KeyBinding(InputKey.TrackNext, GlobalAction.MusicNext),
            new KeyBinding(InputKey.F5, GlobalAction.MusicNext),
            new KeyBinding(InputKey.PlayPause, GlobalAction.MusicPlay),
            new KeyBinding(InputKey.F3, GlobalAction.MusicPlay)
        };
    }

    public enum GlobalAction
    {
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleChat))]
        ToggleChat,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSocial))]
        ToggleSocial,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ResetInputSettings))]
        ResetInputSettings,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleToolbar))]
        ToggleToolbar,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSettings))]
        ToggleSettings,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleBeatmapListing))]
        ToggleBeatmapListing,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.IncreaseVolume))]
        IncreaseVolume,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.DecreaseVolume))]
        DecreaseVolume,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleMute))]
        ToggleMute,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SkipCutscene))]
        SkipCutscene,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.QuickRetry))]
        QuickRetry,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.TakeScreenshot))]
        TakeScreenshot,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleGameplayClicksTaps))]
        ToggleGameplayMouseButtons,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Back))]
        Back,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.IncreaseScrollSpeed))]
        IncreaseScrollSpeed,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.DecreaseScrollSpeed))]
        DecreaseScrollSpeed,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Select))]
        Select,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.QuickExit))]
        QuickExit,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicNext))]
        MusicNext,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicPrev))]
        MusicPrev,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicPlay))]
        MusicPlay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleNowPlaying))]
        ToggleNowPlaying,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectPrevious))]
        SelectPrevious,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectNext))]
        SelectNext,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Home))]
        Home,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleNotifications))]
        ToggleNotifications,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.PauseGameplay))]
        PauseGameplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorSetupMode))]
        EditorSetupMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorComposeMode))]
        EditorComposeMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorDesignMode))]
        EditorDesignMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorTimingMode))]
        EditorTimingMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.HoldForHUD))]
        HoldForHUD,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.RandomSkin))]
        RandomSkin,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.TogglePauseReplay))]
        TogglePauseReplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleInGameInterface))]
        ToggleInGameInterface,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleModSelection))]
        ToggleModSelection,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectNextRandom))]
        SelectNextRandom,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectPreviousRandom))]
        SelectPreviousRandom,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleBeatmapOptions))]
        ToggleBeatmapOptions,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorVerifyMode))]
        EditorVerifyMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorNudgeLeft))]
        EditorNudgeLeft,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorNudgeRight))]
        EditorNudgeRight,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSkinEditor))]
        ToggleSkinEditor,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.PreviousVolumeMeter))]
        PreviousVolumeMeter,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.NextVolumeMeter))]
        NextVolumeMeter,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SeekReplayForward))]
        SeekReplayForward,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SeekReplayBackward))]
        SeekReplayBackward,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleChatFocus))]
        ToggleChatFocus,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorCycleGridDisplayMode))]
        EditorCycleGridDisplayMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorTestGameplay))]
        EditorTestGameplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorFlipHorizontally))]
        EditorFlipHorizontally,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorFlipVertically))]
        EditorFlipVertically,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorIncreaseDistanceSpacing))]
        EditorIncreaseDistanceSpacing,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorDecreaseDistanceSpacing))]
        EditorDecreaseDistanceSpacing,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectPreviousGroup))]
        SelectPreviousGroup,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectNextGroup))]
        SelectNextGroup,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.DeselectAllMods))]
        DeselectAllMods,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorTapForBPM))]
        EditorTapForBPM,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleFPSCounter))]
        ToggleFPSDisplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleProfile))]
        ToggleProfile,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorCloneSelection))]
        EditorCloneSelection,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorCyclePreviousBeatSnapDivisor))]
        EditorCyclePreviousBeatSnapDivisor,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorCycleNextBeatSnapDivisor))]
        EditorCycleNextBeatSnapDivisor,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SaveReplay))]
        SaveReplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ExportReplay))]
        ExportReplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleReplaySettings))]
        ToggleReplaySettings,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleInGameLeaderboard))]
        ToggleInGameLeaderboard,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorToggleRotateControl))]
        EditorToggleRotateControl,
    }

    public enum GlobalActionCategory
    {
        General,
        Editor,
        InGame,
        Replay,
        SongSelect,
        AudioControl,
        Overlays
    }
}
