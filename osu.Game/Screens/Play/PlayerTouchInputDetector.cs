// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Play
{
    public partial class PlayerTouchInputDetector : Component
    {
        [Resolved]
        private Player player { get; set; } = null!;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

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

            // `Player` (probably rightly so) assumes immutability of mods,
            // so this will not be shown immediately on the mod display in the top right.
            // if this is to change, the mod immutability should be revisited.
            player.Score.ScoreInfo.Mods = player.Score.ScoreInfo.Mods.Append(touchDeviceMod).ToArray();
        }
    }
}
