// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using System;

namespace osu.Game.Graphics.UserInterface
{
    // not inheriting osuclickablecontainer/osuhovercontainer
    // because click/hover sounds cannot be disabled, and they make
    // double sounds when reappearing under the cursor
    public class TooltipIconButton : ClickableContainer, IHasTooltip
    {
        private readonly SpriteIcon icon;
        private SampleChannel sampleClick;
        private SampleChannel sampleHover;
        public Action Action;

        private bool isEnabled;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                icon.Alpha = value ? 1 : 0.5f;
            }
        }

        public FontAwesome Icon
        {
            get { return icon.Icon; }
            set { icon.Icon = value; }
        }

        public TooltipIconButton()
        {
            isEnabled = true;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                icon = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(18),
                    Alpha = 0.5f,
                }
            };
        }

        protected override bool OnClick(InputState state)
        {
            if (isEnabled)
            {
                Action?.Invoke();
                sampleClick?.Play();
            }
            return base.OnClick(state);
        }

        protected override bool OnHover(InputState state)
        {
            if (isEnabled)
                sampleHover?.Play();
            return base.OnHover(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleClick = audio.Sample.Get(@"UI/generic-select-soft");
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
        }

        public string TooltipText { get; set; }
    }
}
