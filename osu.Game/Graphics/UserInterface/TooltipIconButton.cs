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
using osu.Framework.Input.States;
using System;

namespace osu.Game.Graphics.UserInterface
{
    // not inheriting osuclickablecontainer/osuhovercontainer
    // because click/hover sounds cannot be disabled
    public class TooltipIconButton : ClickableContainer, IHasTooltip
    {
        private readonly SpriteIcon icon;
        private SampleChannel sampleHover;
        private SampleChannel sampleClick;

        /// <summary>
        /// The action to fire upon click, if <see cref="IsEnabled"/> is set to true.
        /// </summary>
        public Action Action;

        private bool isEnabled;

        /// <summary>
        /// If set to true, upon click the <see cref="Action"/> will execute. It wont otherwise.
        /// </summary>
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                icon.FadeTo(value ? 1 : 0.5f, 250);
            }
        }

        public FontAwesome Icon
        {
            get { return icon.Icon; }
            set { icon.Icon = value; }
        }

        /// <summary>
        /// A simple icon that has an action upon click and can be disabled.
        /// </summary>
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
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.8f),
                }
            };
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

        protected override bool OnHover(InputState state)
        {
            if (isEnabled)
                sampleHover?.Play();
            return base.OnHover(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover-soft");
            sampleClick = audio.Sample.Get(@"UI/generic-select-soft");
        }

        public string TooltipText { get; set; }
    }
}
