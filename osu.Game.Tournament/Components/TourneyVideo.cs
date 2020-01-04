// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using osu.Game.Graphics;

namespace osu.Game.Tournament.Components
{
    public class TourneyVideo : CompositeDrawable
    {
        private readonly VideoSprite video;

        private readonly ManualClock manualClock;

        public TourneyVideo(Stream stream)
        {
            if (stream == null)
            {
                InternalChild = new Box
                {
                    Colour = ColourInfo.GradientVertical(OsuColour.Gray(0.3f), OsuColour.Gray(0.6f)),
                    RelativeSizeAxes = Axes.Both,
                };
            }
            else
            {
                InternalChild = video = new VideoSprite(stream)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,
                    Clock = new FramedClock(manualClock = new ManualClock())
                };
            }
        }

        public bool Loop
        {
            set
            {
                if (video != null)
                    video.Loop = value;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (manualClock != null && Clock.ElapsedFrameTime < 100)
            {
                // we want to avoid seeking as much as possible, because we care about performance, not sync.
                // to avoid seeking completely, we only increment out local clock when in an updating state.
                manualClock.CurrentTime += Clock.ElapsedFrameTime;
            }
        }
    }
}
