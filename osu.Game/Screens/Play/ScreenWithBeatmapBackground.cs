// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.Play
{
    public abstract partial class ScreenWithBeatmapBackground : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        public void ApplyToBackground(Action<BackgroundScreenBeatmap> action)
        {
            Debug.Assert(this.IsCurrentScreen());

            base.ApplyToBackground(b => action.Invoke((BackgroundScreenBeatmap)b));
        }
    }
}
