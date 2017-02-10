// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Modes.Osu.Objects.Drawables.Connections
{
    public class FollowPoint : Container
    {
        private Sprite followPoint;

        public double StartTime;
        public double EndTime;
        public Vector2 EndPosition;

        public FollowPoint()
        {
            Origin = Anchor.Centre;
            Alpha = 0;

            Children = new Drawable[]
            {
                followPoint = new Sprite
                {
                    Size = new Vector2(12f),
                    Origin = Anchor.Centre,
                    BlendingMode = BlendingMode.Additive,
                    Alpha = 0.5f
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DelayReset();

            Delay(StartTime);
            FadeIn(DrawableOsuHitObject.TIME_FADEIN);
            ScaleTo(1.5f);
            ScaleTo(1, DrawableOsuHitObject.TIME_FADEIN, EasingTypes.Out);
            MoveTo(EndPosition, DrawableOsuHitObject.TIME_FADEIN, EasingTypes.Out);

            Delay(EndTime - StartTime);
            FadeOut(DrawableOsuHitObject.TIME_FADEIN);

            Delay(DrawableOsuHitObject.TIME_FADEIN);
            Expire(true);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            followPoint.Texture = textures.Get(@"Play/osu/ring-glow");
        }
    }
}