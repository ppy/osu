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
        {
            if (game is IKeyBindingHandler<GlobalAction>)
                handler = game;
        }

        public override IEnumerable<KeyBinding> DefaultKeyBindings => GlobalKeyBindings.Concat(InGameKeyBindings).Concat(AudioControlKeyBindings);

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
            new KeyBinding(new[] { InputKey.Control, InputKey.D }, GlobalAction.ToggleDirect),

            new KeyBinding(InputKey.Escape, GlobalAction.Back),
            new KeyBinding(InputKey.ExtraMouseButton1, GlobalAction.Back),

            new KeyBinding(InputKey.Space, GlobalAction.Select),
            new KeyBinding(InputKey.Enter, GlobalAction.Select),
            new KeyBinding(InputKey.KeypadEnter, GlobalAction.Select),
        };

        public IEnumerable<KeyBinding> InGameKeyBindings => new[]
        {
            new KeyBinding(InputKey.Space, GlobalAction.SkipCutscene),
            new KeyBinding(InputKey.Tilde, GlobalAction.QuickRetry),
            new KeyBinding(new[] { InputKey.Control, InputKey.Tilde }, GlobalAction.QuickExit),
            new KeyBinding(new[] { InputKey.Control, InputKey.Plus }, GlobalAction.IncreaseScrollSpeed),
            new KeyBinding(new[] { InputKey.Control, InputKey.Minus }, GlobalAction.DecreaseScrollSpeed),
        };

        public IEnumerable<KeyBinding> AudioControlKeyBindings => new[]
        {
            new KeyBinding(InputKey.Up, GlobalAction.IncreaseVolume),
            new KeyBinding(InputKey.MouseWheelUp, GlobalAction.IncreaseVolume),
            new KeyBinding(InputKey.Down, GlobalAction.DecreaseVolume),
            new KeyBinding(InputKey.MouseWheelDown, GlobalAction.DecreaseVolume),
            new KeyBinding(InputKey.F4, GlobalAction.ToggleMute),

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
        [Description("切换聊天界面")]
        ToggleChat,

        [Description("切换玩家列表界面")]
        ToggleSocial,

        [Description("重置输入设置")]
        ResetInputSettings,

        [Description("切换顶栏")]
        ToggleToolbar,

        [Description("切换设置界面")]
        ToggleSettings,

        [Description("切换osu!direct界面")]
        ToggleDirect,

        [Description("增加音量")]
        IncreaseVolume,

        [Description("减小音量")]
        DecreaseVolume,

        [Description("切换静音")]
        ToggleMute,

        // In-Game Keybindings
        [Description("跳过")]
        SkipCutscene,

        [Description("快速重试 (按住)")]
        QuickRetry,

        [Description("屏幕截图")]
        TakeScreenshot,

        [Description("切换游戏内鼠标按键")]
        ToggleGameplayMouseButtons,

        [Description("返回")]
        Back,

        [Description("增加下落速度")]
        IncreaseScrollSpeed,

        [Description("减小下落速度")]
        DecreaseScrollSpeed,

        [Description("选择")]
        Select,

        [Description("快速退出(按住)")]
        QuickExit,

        // Game-wide beatmap msi ccotolle keybindings
        [Description("下一首")]
        MusicNext,

        [Description("上一首")]
        MusicPrev,

        [Description("播放/暂停")]
        MusicPlay,

        [Description("切换正在播放列表")]
        ToggleNowPlaying,
    }
}
