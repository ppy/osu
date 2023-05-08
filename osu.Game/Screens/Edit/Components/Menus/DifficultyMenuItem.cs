// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Components.Menus
{
    public class DifficultyMenuItem : StatefulMenuItem<bool>
    {
        public BeatmapInfo BeatmapInfo { get; }

        public DifficultyMenuItem(BeatmapInfo beatmapInfo, bool selected, Action<BeatmapInfo> difficultyChangeFunc)
            : base(string.IsNullOrEmpty(beatmapInfo.DifficultyName) ? "(unnamed)" : beatmapInfo.DifficultyName, null)
        {
            BeatmapInfo = beatmapInfo;
            State.Value = selected;

            if (!selected)
                Action.Value = () => difficultyChangeFunc.Invoke(beatmapInfo);
        }

        public override IconUsage? GetIconForState(bool state) => state ? FontAwesome.Solid.Check : null;
    }
}
