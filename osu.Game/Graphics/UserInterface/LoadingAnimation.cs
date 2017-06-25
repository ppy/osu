// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using OpenTK;

namespace osu.Game.Graphics.UserInterface
{
    public class LoadingAnimation : VisibilityContainer
    {
        private readonly TextAwesome spinner;

        public LoadingAnimation()
        {
            Size = new Vector2(20);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                spinner = new TextAwesome
                {
                    TextSize = 20,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_spinner,
                    Shadow = false
                }
            };
        }


        protected override void LoadComplete()
        {
            base.LoadComplete();

            const float duration = 100;
            // 8 notches in spinner requre 8 animation intervals
            const float intervals = 8;
            // angle to rotate the spinner one notch
            const float angle = 360 / intervals;

            for (int i = 0; i < intervals; i++)
            {
                spinner.Transforms.Add(new TransformRotation
                {
                    StartValue = angle * i,
                    EndValue = angle * i,
                    StartTime = duration * i,
                    EndTime = duration + (duration * i),
                    LoopCount = -1,
                    LoopDelay = duration * (intervals - 1)
                });
            }
        }

        private const float transition_duration = 500;

        protected override void PopIn() => FadeIn(transition_duration * 5, EasingTypes.OutQuint);

        protected override void PopOut() => FadeOut(transition_duration, EasingTypes.OutQuint);
    }
}
