// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuTouchDeviceDetector : Drawable
    {
        /// <summary>
        /// The distance required for a mouse input to be considered a touchscreen input relative to a previous mouse input.
        /// </summary>
        private const int mouse_input_touchscreen_distance = 100;

        /// <summary>
        /// The max amount of <see cref="flaggedTouchesCount"/> that is required for <see cref="OsuModTouchDevice"/> to be applied.
        /// </summary>
        private const int max_flagged_touches = 10;

        /// <summary>
        /// Tracks the amount of inputs that got flagged as a touch device input.
        /// </summary>
        private int flaggedTouchesCount;

        /// <summary>
        /// Whether touch device input is already detected.
        /// </summary>
        private bool detectedTouchDevice;

        [Resolved(CanBeNull = true)]
        private Player? player { get; set; }

        private Vector2? previousTouchPos;

        /// <summary>
        /// Checks whether a given input position is likely to be from a touch device.
        /// This is done by comparing said position with the previous touch position.
        /// </summary>
        /// <param name="inputPosition">The input position to be compared.</param>
        private bool isTouchInput(Vector2 inputPosition) =>
            previousTouchPos != null && Vector2.Distance(previousTouchPos.Value, inputPosition) > mouse_input_touchscreen_distance;

        /// <summary>
        /// Detects and applies the <see cref="OsuModTouchDevice"/> to the player mods.
        /// </summary>
        /// <param name="newTouchPosition">The new touch position to be checked against <see cref="isTouchInput"/></param>
        private void detectTouchInput(Vector2 newTouchPosition)
        {
            if (player != null && !detectedTouchDevice && isTouchInput(newTouchPosition))
            {
                detectedTouchDevice = ++flaggedTouchesCount > max_flagged_touches;

                if (detectedTouchDevice)
                    player.Score.ScoreInfo.Mods = player.Score.ScoreInfo.Mods.Append(new OsuModTouchDevice()).ToArray();
            }

            previousTouchPos = newTouchPosition;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            detectTouchInput(e.MousePosition);

            return base.OnMouseDown(e);
        }

        public void OnDirectTouch(TouchDownEvent e)
        {
            detectTouchInput(e.TouchDownPosition);
        }
    }
}
