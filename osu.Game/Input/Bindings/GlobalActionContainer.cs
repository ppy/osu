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
                                                                       .Concat(AudioControlKeyBindings)
                                                                       .Concat(MvisControlKeyBindings);

        public IEnumerable<KeyBinding> MvisControlKeyBindings => new[]
        {
            new KeyBinding(InputKey.Left, GlobalAction.MvisMusicPrev),
            new KeyBinding(InputKey.Right, GlobalAction.MvisMusicNext),
            new KeyBinding(InputKey.Space, GlobalAction.MvisTogglePause),
            new KeyBinding(InputKey.Tab, GlobalAction.MvisToggleOverlayLock),
            new KeyBinding(InputKey.Slash, GlobalAction.MvisTogglePlayList),
            new KeyBinding(InputKey.L, GlobalAction.MvisToggleTrackLoop),
            new KeyBinding(InputKey.Enter, GlobalAction.MvisOpenInSongSelect),
            new KeyBinding(InputKey.H, GlobalAction.MvisForceLockOverlayChanges),
            new KeyBinding(InputKey.Period, GlobalAction.MvisSelectCollection),
            new KeyBinding(InputKey.Comma, GlobalAction.MvisTogglePluginPage),
        };

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
            new KeyBinding(new[] { InputKey.Control, InputKey.Shift, InputKey.A }, GlobalAction.EditorVerifyMode),
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

        [Description("切换谱面列表界面")]
        ToggleBeatmapListing,

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

        // Game-wide beatmap music controller keybindings
        [Description("下一首")]
        MusicNext,

        [Description("上一首")]
        MusicPrev,

        [Description("播放/暂停")]
        MusicPlay,

        [Description("切换正在播放列表")]
        ToggleNowPlaying,

        [Description("上一个")]
        SelectPrevious,

        [Description("下一个")]
        SelectNext,

        [Description("上一首")]
        MvisMusicPrev,

        [Description("下一首")]
        MvisMusicNext,

        [Description("暂停/播放")]
        MvisTogglePause,

        [Description("切换锁定")]
        MvisToggleOverlayLock,

        [Description("切换播放列表")]
        MvisTogglePlayList,

        [Description("切换单曲循环")]
        MvisToggleTrackLoop,

        [Description("在歌曲选择中打开")]
        MvisOpenInSongSelect,

        [Description("返回主页")]
        Home,

        [Description("切换通知栏是否可见")]
        ToggleNotifications,

        [Description("暂停游戏")]
        PauseGameplay,

        [Description("切换强制锁定")]
        MvisForceLockOverlayChanges,

        // Editor
        [Description("谱面设置模式")]
        EditorSetupMode,

        [Description("物件排布模式")]
        EditorComposeMode,

        [Description("谱面设计模式")]
        EditorDesignMode,

        [Description("Timing设计模式")]
        EditorTimingMode,

        [Description("选择收藏夹")]
        MvisSelectCollection,

        [Description("长按以显示HUD")]
        HoldForHUD,

        [Description("随机皮肤")]
        RandomSkin,

        [Description("暂停 / 恢复重放")]
        TogglePauseReplay,

        [Description("切换游戏内界面")]
        ToggleInGameInterface,

        [Description("打开插件列表")]
        MvisTogglePluginPage,

        // Song select keybindings
        [Description("切换Mod选择")]
        ToggleModSelection,

        [Description("随机选择")]
        SelectNextRandom,

        [Description("撤销随机")]
        SelectPreviousRandom,

        [Description("谱面选项")]
        ToggleBeatmapOptions,

        [Description("Verify mode")]
        EditorVerifyMode,
    }
}
