// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
                    Icon = FontAwesome.fa_spinner
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            using (spinner.BeginLoopedSequence())
                spinner.RotateTo(360, 2000);
        }

        private const float transition_duration = 500;

        protected override void PopIn() => FadeIn(transition_duration * 5, EasingTypes.OutQuint);

        protected override void PopOut() => FadeOut(transition_duration, EasingTypes.OutQuint);
    }
}
