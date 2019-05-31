// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;

namespace osu.Game.Storyboards.Drawables
{
    public class DrawableStoryboardAnimation : TextureAnimation, IFlippable
    {
        public StoryboardAnimation Animation { get; private set; }

        public bool FlipH { get; set; }
        public bool FlipV { get; set; }

        public override bool RemoveWhenNotAlive => false;

        protected override Vector2 DrawScale
            => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y);

        public override Anchor Origin
        {
            get
            {
                var origin = base.Origin;

                if (FlipH)
                {
                    if (origin.HasFlag(Anchor.x0))
                        origin = Anchor.x2 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                    else if (origin.HasFlag(Anchor.x2))
                        origin = Anchor.x0 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                }

                if (FlipV)
                {
                    if (origin.HasFlag(Anchor.y0))
                        origin = Anchor.y2 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                    else if (origin.HasFlag(Anchor.y2))
                        origin = Anchor.y0 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                }

                return origin;
            }
        }

        public override bool IsPresent
            => !float.IsNaN(DrawPosition.X) && !float.IsNaN(DrawPosition.Y) && base.IsPresent;

        public DrawableStoryboardAnimation(StoryboardAnimation animation)
        {
            Animation = animation;
            Origin = animation.Origin;
            Position = animation.InitialPosition;
            Repeat = animation.LoopType == AnimationLoopType.LoopForever;

            LifetimeStart = animation.StartTime;
            LifetimeEnd = animation.EndTime;
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap> beatmap, TextureStore textureStore)
        {
            for (var frame = 0; frame < Animation.FrameCount; frame++)
            {
                var framePath = Animation.Path.Replace(".", frame + ".");

                var path = beatmap.Value.BeatmapSetInfo.Files.Find(f => f.Filename.Equals(framePath, StringComparison.InvariantCultureIgnoreCase))?.FileInfo.StoragePath;
                if (path == null)
                    continue;

                var texture = textureStore.Get(path);
                AddFrame(texture, Animation.FrameDelay);
            }

            Animation.ApplyTransforms(this);
        }
    }
}
