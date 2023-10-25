// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Play
{
    public partial class PlayerTouchInputHandler : Component
    {
        [Resolved]
        private Player player { get; set; } = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        private IBindable<bool> touchActive = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(SessionStatics statics)
        {
            touchActive = statics.GetBindable<bool>(Static.TouchInputActive);
            touchActive.BindValueChanged(_ => updateState());
        }

        private void updateState()
        {
            if (!touchActive.Value)
                return;

            if (gameplayState.HasPassed || gameplayState.HasFailed || gameplayState.HasQuit)
                return;

            if (gameplayState.Score.ScoreInfo.Mods.OfType<ModTouchDevice>().Any())
                return;

            if (player.IsBreakTime.Value)
                return;

            var touchDeviceMod = gameplayState.Ruleset.GetTouchDeviceMod();
            if (touchDeviceMod == null)
                return;

            onScreenDisplay?.Display(new TouchDeviceDetectedToast());

            // TODO: this is kinda crude. `Player` (probably rightly so) assumes immutability of mods.
            // this probably should be shown immediately on screen in the HUD,
            // which means that immutability will probably need to be revisited.
            player.Score.ScoreInfo.Mods = player.Score.ScoreInfo.Mods.Append(touchDeviceMod).ToArray();
        }
    }
}
