// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Textures;
using System.Linq;

namespace osu.Game.Storyboards.Drawables
{
    public class StoryboardAnimation : TextureAnimation, IFlippable
    {
        public AnimationDefinition Definition { get; private set; }

        protected override bool ShouldBeAlive => Definition.HasCommands && base.ShouldBeAlive;
        public override bool RemoveWhenNotAlive => !Definition.HasCommands || base.RemoveWhenNotAlive;

        public bool FlipH { get; set; }
        public bool FlipV { get; set; }
        protected override Vector2 DrawScale => new Vector2(FlipH ? -base.DrawScale.X : base.DrawScale.X, FlipV ? -base.DrawScale.Y : base.DrawScale.Y);

        public StoryboardAnimation(AnimationDefinition definition)
        {
            Definition = definition;
            Origin = definition.Origin;
            Position = definition.InitialPosition;
            Repeat = definition.LoopType == AnimationLoopType.LoopForever;

            if (definition.HasCommands)
            {
                LifetimeStart = definition.StartTime;
                LifetimeEnd = definition.EndTime;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, TextureStore textureStore)
        {
            for (var frame = 0; frame < Definition.FrameCount; frame++)
            {
                var framePath = Definition.Path.Replace(".", frame + ".");

                var path = game.Beatmap.Value.BeatmapSetInfo.Files.FirstOrDefault(f => f.Filename == framePath)?.FileInfo.StoragePath;
                if (path == null)
                    continue;

                var texture = textureStore.Get(path);
                AddFrame(texture, Definition.FrameDelay);
            }
            Definition.ApplyTransforms(this);
        }
    }
}
