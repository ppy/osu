// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
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
        };

        public IEnumerable<KeyBinding> InGameKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.ExtraMouseButton2, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.Tilde, GlobalAction.QuickRetry),
            new KeyBinding(new[] { InputKey.Control, InputKey.Tilde }, GlobalAction.QuickExit),
            new KeyBinding(new[] { InputKey.Control, InputKey.Plus }, GlobalAction.IncreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.Control, InputKey.Minus }, GlobalAction.DecreaseScrollSpeed),
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
        [Description("Toggle chat overlay")]
        ToggleChat,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSocial))]
        [Description("Toggle social overlay")]
        ToggleSocial,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ResetInputSettings))]
        [Description("Reset input settings")]
        ResetInputSettings,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleToolbar))]
        [Description("Toggle toolbar")]
        ToggleToolbar,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSettings))]
        [Description("Toggle settings")]
        ToggleSettings,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleBeatmapListing))]
        [Description("Toggle beatmap listing")]
        ToggleBeatmapListing,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.IncreaseVolume))]
        [Description("Increase volume")]
        IncreaseVolume,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.DecreaseVolume))]
        [Description("Decrease volume")]
        DecreaseVolume,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleMute))]
        [Description("Toggle mute")]
        ToggleMute,

        // In-Game Keybindings
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SkipCutscene))]
        [Description("Skip cutscene")]
        SkipCutscene,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.QuickRetry))]
        [Description("Quick retry (hold)")]
        QuickRetry,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.TakeScreenshot))]
        [Description("Take screenshot")]
        TakeScreenshot,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleGameplayMouseButtons))]
        [Description("Toggle gameplay mouse buttons")]
        ToggleGameplayMouseButtons,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Back))]
        [Description("Back")]
        Back,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.IncreaseScrollSpeed))]
        [Description("Increase scroll speed")]
        IncreaseScrollSpeed,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.DecreaseScrollSpeed))]
        [Description("Decrease scroll speed")]
        DecreaseScrollSpeed,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Select))]
        [Description("Select")]
        Select,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.QuickExit))]
        [Description("Quick exit (hold)")]
        QuickExit,

        // Game-wide beatmap music controller keybindings
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicNext))]
        [Description("Next track")]
        MusicNext,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicPrev))]
        [Description("Previous track")]
        MusicPrev,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.MusicPlay))]
        [Description("Play / pause")]
        MusicPlay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleNowPlaying))]
        [Description("Toggle now playing overlay")]
        ToggleNowPlaying,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectPrevious))]
        [Description("Previous selection")]
        SelectPrevious,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectNext))]
        [Description("Next selection")]
        SelectNext,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.Home))]
        [Description("Home")]
        Home,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleNotifications))]
        [Description("Toggle notifications")]
        ToggleNotifications,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.PauseGameplay))]
        [Description("Pause gameplay")]
        PauseGameplay,

        // Editor
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorSetupMode))]
        [Description("Setup mode")]
        EditorSetupMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorComposeMode))]
        [Description("Compose mode")]
        EditorComposeMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorDesignMode))]
        [Description("Design mode")]
        EditorDesignMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorTimingMode))]
        [Description("Timing mode")]
        EditorTimingMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.HoldForHUD))]
        [Description("Hold for HUD")]
        HoldForHUD,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.RandomSkin))]
        [Description("Random skin")]
        RandomSkin,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.TogglePauseReplay))]
        [Description("Pause / resume replay")]
        TogglePauseReplay,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleInGameInterface))]
        [Description("Toggle in-game interface")]
        ToggleInGameInterface,

        // Song select keybindings
        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleModSelection))]
        [Description("Toggle Mod Select")]
        ToggleModSelection,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectNextRandom))]
        [Description("Random")]
        SelectNextRandom,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SelectPreviousRandom))]
        [Description("Rewind")]
        SelectPreviousRandom,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleBeatmapOptions))]
        [Description("Beatmap Options")]
        ToggleBeatmapOptions,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorVerifyMode))]
        [Description("Verify mode")]
        EditorVerifyMode,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorNudgeLeft))]
        [Description("Nudge selection left")]
        EditorNudgeLeft,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.EditorNudgeRight))]
        [Description("Nudge selection right")]
        EditorNudgeRight,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleSkinEditor))]
        [Description("Toggle skin editor")]
        ToggleSkinEditor,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.PreviousVolumeMeter))]
        [Description("Previous volume meter")]
        PreviousVolumeMeter,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.NextVolumeMeter))]
        [Description("Next volume meter")]
        NextVolumeMeter,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SeekReplayForward))]
        [Description("Seek replay forward")]
        SeekReplayForward,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.SeekReplayBackward))]
        [Description("Seek replay backward")]
        SeekReplayBackward,

        [LocalisableDescription(typeof(GlobalActionKeyBindingStrings), nameof(GlobalActionKeyBindingStrings.ToggleChatFocus))]
        [Description("Toggle chat focus")]
        ToggleChatFocus
    }
}
