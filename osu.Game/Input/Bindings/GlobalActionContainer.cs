// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Input.Bindings
{
    public class GlobalActionContainer : DatabasedKeyBindingContainer<GlobalAction>, IHandleGlobalKeyboardInput
    {
        private readonly Drawable handler;
        private InputManager parentInputManager;

        public GlobalActionContainer(OsuGameBase game)
            : base(matchingMode: KeyCombinationMatchingMode.Modifiers)
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            parentInputManager = GetContainingInputManager();
        }

        public override IEnumerable<IKeyBinding> DefaultKeyBindings => GlobalKeyBindings
                                                                       .Concat(EditorKeyBindings)
                                                                       .Concat(InGameKeyBindings)
                                                                       .Concat(SongSelectKeyBindings)
                                                                       .Concat(AudioControlKeyBindings);

        public IEnumerable<KeyBinding> GlobalKeyBindings => new[]
        {
            new KeyBinding(InputKey.F6, GlobalAction.ToggleNowPlaying),
            new KeyBinding(InputKey.F8, GlobalAction.ToggleChat),
            new KeyBinding(InputKey.F9, GlobalAction.ToggleSocial),
            new KeyBinding(InputKey.F10, GlobalAction.ToggleGameplayMouseButtons),
            new KeyBinding(InputKey.F12, GlobalAction.TakeScreenshot),

            new KeyBinding(new[] { InputKey.Control, InputKey.Alt, InputKey.R }, GlobalAction.ResetInputSettings),
            new KeyBinding(new[] { InputKey.Control, InputKey.T }, GlobalAction.ToggleToolbar),
            new KeyBinding(new[] { InputKey.Control, InputKey.O }, GlobalAction.ToggleSettings),
            new KeyBinding(new[] { InputKey.Control, InputKey.D }, GlobalAction.ToggleBeatmapListing),
            new KeyBinding(new[] { InputKey.Control, InputKey.N }, GlobalAction.ToggleNotifications),
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.S }, GlobalAction.ToggleSkinEditor),

            new KeyBinding(InputKey.Escape, GlobalAction.Back),
            new KeyBinding(InputKey.ExtraMouseButton1, GlobalAction.Back),

            new KeyBinding(new[] { InputKey.Alt, InputKey.Home }, GlobalAction.Home),

            new KeyBinding(InputKey.Up, GlobalAction.SelectPrevious),
            new KeyBinding(InputKey.Down, GlobalAction.SelectNext),

            new KeyBinding(InputKey.Space, GlobalAction.Select),
            new KeyBinding(InputKey.Enter, GlobalAction.Select),
            new KeyBinding(InputKey.KeypadEnter, GlobalAction.Select),

            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.R }, GlobalAction.RandomSkin),
        };

        public IEnumerable<KeyBinding> EditorKeyBindings => new[]
        {
            new KeyBinding(new[] { InputKey.F1 }, GlobalAction.EditorComposeMode),
            new KeyBinding(new[] { InputKey.F2 }, GlobalAction.EditorDesignMode),
            new KeyBinding(new[] { InputKey.F3 }, GlobalAction.EditorTimingMode),
            new KeyBinding(new[] { InputKey.F4 }, GlobalAction.EditorSetupMode),
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.A }, GlobalAction.EditorVerifyMode),
            new KeyBinding(new[] { InputKey.J }, GlobalAction.EditorNudgeLeft),
            new KeyBinding(new[] { InputKey.K }, GlobalAction.EditorNudgeRight),
            new KeyBinding(new[] { InputKey.G }, GlobalAction.EditorCycleGridDisplayMode),
            new KeyBinding(new[] { InputKey.F5 }, GlobalAction.EditorTestGameplay),
        };

        public IEnumerable<KeyBinding> InGameKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.ExtraMouseButton2, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.Tilde, GlobalAction.QuickRetry),
            new KeyBinding(new[] { InputKey.Control, InputKey.Tilde }, GlobalAction.QuickExit),
            new KeyBinding(new[] { InputKey.F3 }, GlobalAction.DecreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.F4 }, GlobalAction.IncreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.Shift, InputKey.Tab }, GlobalAction.ToggleInGameInterface),
            new KeyBinding(InputKey.MouseMiddle, GlobalAction.PauseGameplay),
            new KeyBinding(InputKey.Space, GlobalAction.TogglePauseReplay),
            new KeyBinding(InputKey.Left, GlobalAction.SeekReplayBackward),
            new KeyBinding(InputKey.Right, GlobalAction.SeekReplayForward),
            new KeyBinding(InputKey.Control, GlobalAction.HoldForHUD),
            new KeyBinding(InputKey.Tab, GlobalAction.ToggleChatFocus),
        };

        public IEnumerable<KeyBinding> SongSelectKeyBindings => new[]
        {
            new KeyBinding(InputKey.F1, GlobalAction.ToggleModSelection),
            new KeyBinding(InputKey.F2, GlobalAction.SelectNextRandom),
            new KeyBinding(new[] { InputKey.Shift, InputKey.F2 }, GlobalAction.SelectPreviousRandom),
            new KeyBinding(InputKey.F3, GlobalAction.ToggleBeatmapOptions)
        };

        public IEnumerable<KeyBinding> AudioControlKeyBindings => new[]
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

        protected override IEnumerable<Drawable> KeyBindingInputQueue
        {
            get
            {
                // To ensure the global actions are handled with priority, this GlobalActionContainer is actually placed after game content.
                // It does not contain children as expected, so we need to forward the NonPositionalInputQueue from the parent input manager to correctly
                // allow the whole game to handle these actions.

                // An eventual solution to this hack is to create localised action containers for individual components like SongSelect, but this will take some rearranging.
                var inputQueue = parentInputManager?.NonPositionalInputQueue ?? base.KeyBindingInputQueue;

                return handler != null ? inputQueue.Prepend(handler) : inputQueue;
            }
        }
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

        // In-Game Keybindings
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SkipCutscene))]
        SkipCutscene,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.QuickRetry))]
        QuickRetry,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.TakeScreenshot))]
        TakeScreenshot,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleGameplayMouseButtons))]
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

        // Game-wide beatmap music controller keybindings
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

        // Editor
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

        // Song select keybindings
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
        EditorTestGameplay
    }
}
