// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayReadCoords : CompositeDrawable
    {
        private Bindable<string> replayPlayerX { get; set; } = new Bindable<string>("0");
        private Bindable<string> replayPlayerY { get; set; } = new Bindable<string>("0");

        private readonly List<OsuReplayFrame> replayFrames;
        private int currentFrame = -1;
        private double lastTime = double.MinValue;

        public ReplayReadCoords(Replay replay)
        {
            RelativeSizeAxes = Axes.Both;
            replayFrames = replay.Frames.Cast<OsuReplayFrame>().ToList();
        }

        [BackgroundDependencyLoader]
        private void load(OsuRulesetConfigManager config)
        {
            config.BindWith(OsuRulesetSetting.ReplayPlayerX, replayPlayerX);
            config.BindWith(OsuRulesetSetting.ReplayPlayerY, replayPlayerY);
        }

        // protected override void LoadComplete()
        // {
        // }

        protected override void Update()
        {
            base.Update();

            if (currentFrame == replayFrames.Count - 1) return; //No more frames

            double time = Clock.CurrentTime;
            bool changeCoords = false;

            // support for next and last frame - Check if the time has changed
            if (lastTime != time)
            {
                // Determine the next frame based on whether time is moving forward or backward
                int nextFrame = currentFrame + (lastTime < time ? 1 : -1);

                lastTime = time;

                // currentFrame < 0 - first frame; set initial value
                // nextFrame >= 0 - to avoid negative indices
                // check if time from next frame is closer to time from current frame
                if (currentFrame < 0 || (nextFrame >= 0 && Math.Abs(replayFrames[nextFrame].Time - time) <= Math.Abs(replayFrames[currentFrame].Time - time)))
                {
                    currentFrame = nextFrame;
                    changeCoords = true; // Indicate that shown replay coordinates need to be updated
                }
            }

            if (changeCoords)
            {
                Vector2 position = replayFrames[currentFrame].Position;
                replayPlayerX.Value = position.X.ToString();
                replayPlayerY.Value = position.Y.ToString();
            }
        }
    }
}
