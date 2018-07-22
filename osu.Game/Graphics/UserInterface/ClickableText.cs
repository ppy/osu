// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using System;

namespace osu.Game.Graphics.UserInterface
{
    public class ClickableText : SpriteText, IHasTooltip
    {
        private bool isEnabled;
        private SampleChannel sampleHover;
        private SampleChannel sampleClick;

        public Action Action;

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                Alpha = value ? 1 : 0.5f;
            }
        }

        public ClickableText() => isEnabled = true;

        public override bool HandleMouseInput => true;

        protected override bool OnHover(InputState state)
        {
            if (isEnabled)
                sampleHover?.Play();
            return base.OnHover(state);
        }

        protected override bool OnClick(InputState state)
        {
            if (isEnabled)
            {
                sampleClick?.Play();
                Action?.Invoke();
            }
            return base.OnClick(state);
        }

        public string TooltipText { get; set; }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
        }
    }
}
