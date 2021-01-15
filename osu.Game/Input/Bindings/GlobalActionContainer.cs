// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;

namespace osu.Game.Input.Bindings
{
    public class GlobalActionContainer : DatabasedKeyBindingContainer<GlobalAction>, IHandleGlobalKeyboardInput
    {
        private readonly Drawable handler;

        public GlobalActionContainer(OsuGameBase game)
            : base(matchingMode: KeyCombinationMatchingMode.Modifiers)
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        public override IEnumerable<IKeyBinding> DefaultKeyBindings => GlobalKeyBindings.Concat(InGameKeyBindings).Concat(AudioControlKeyBindings).Concat(EditorKeyBindings);

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
            new KeyBinding(InputKey.Control, GlobalAction.HoldForHUD),
        };

        public IEnumerable<KeyBinding> AudioControlKeyBindings => new[]
        {
            new KeyBinding(new[] { InputKey.Alt, InputKey.Up }, GlobalAction.IncreaseVolume),
            new KeyBinding(new[] { InputKey.Alt, InputKey.MouseWheelUp }, GlobalAction.IncreaseVolume),
            new KeyBinding(new[] { InputKey.Alt, InputKey.Down }, GlobalAction.DecreaseVolume),
            new KeyBinding(new[] { InputKey.Alt, InputKey.MouseWheelDown }, GlobalAction.DecreaseVolume),

            new KeyBinding(new[] { InputKey.Control, InputKey.F4 }, GlobalAction.ToggleMute),

            new KeyBinding(InputKey.TrackPrevious, GlobalAction.MusicPrev),
            new KeyBinding(InputKey.F1, GlobalAction.MusicPrev),
            new KeyBinding(InputKey.TrackNext, GlobalAction.MusicNext),
            new KeyBinding(InputKey.F5, GlobalAction.MusicNext),
            new KeyBinding(InputKey.PlayPause, GlobalAction.MusicPlay),
            new KeyBinding(InputKey.F3, GlobalAction.MusicPlay)
        };

        protected override IEnumerable<Drawable> KeyBindingInputQueue =>
            handler == null ? base.KeyBindingInputQueue : base.KeyBindingInputQueue.Prepend(handler);
    }

    public enum GlobalAction
    {
        [Description("Toggle chat overlay")]
        ToggleChat,

        [Description("Toggle social overlay")]
        ToggleSocial,

        [Description("Reset input settings")]
        ResetInputSettings,

        [Description("Toggle toolbar")]
        ToggleToolbar,

        [Description("Toggle settings")]
        ToggleSettings,

        [Description("Toggle beatmap listing")]
        ToggleBeatmapListing,

        [Description("Increase volume")]
        IncreaseVolume,

        [Description("Decrease volume")]
        DecreaseVolume,

        [Description("Toggle mute")]
        ToggleMute,

        // In-Game Keybindings
        [Description("Skip cutscene")]
        SkipCutscene,

        [Description("Quick retry (hold)")]
        QuickRetry,

        [Description("Take screenshot")]
        TakeScreenshot,

        [Description("Toggle gameplay mouse buttons")]
        ToggleGameplayMouseButtons,

        [Description("Back")]
        Back,

        [Description("Increase scroll speed")]
        IncreaseScrollSpeed,

        [Description("Decrease scroll speed")]
        DecreaseScrollSpeed,

        [Description("Select")]
        Select,

        [Description("Quick exit (hold)")]
        QuickExit,

        // Game-wide beatmap music controller keybindings
        [Description("Next track")]
        MusicNext,

        [Description("Previous track")]
        MusicPrev,

        [Description("Play / pause")]
        MusicPlay,

        [Description("Toggle now playing overlay")]
        ToggleNowPlaying,

        [Description("Previous selection")]
        SelectPrevious,

        [Description("Next selection")]
        SelectNext,

        [Description("Home")]
        Home,

        [Description("Toggle notifications")]
        ToggleNotifications,

        [Description("Pause gameplay")]
        PauseGameplay,

        // Editor
        [Description("Setup mode")]
        EditorSetupMode,

        [Description("Compose mode")]
        EditorComposeMode,

        [Description("Design mode")]
        EditorDesignMode,

        [Description("Timing mode")]
        EditorTimingMode,

        [Description("Hold for HUD")]
        HoldForHUD,

        [Description("Random skin")]
        RandomSkin,

        [Description("Pause / resume replay")]
        TogglePauseReplay,

        [Description("Toggle in-game interface")]
        ToggleInGameInterface,
    }
}
