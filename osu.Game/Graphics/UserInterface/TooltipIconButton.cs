// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics.Containers;
using System;

namespace osu.Game.Graphics.UserInterface
{
    public class TooltipIconButton : OsuClickableContainer, IHasTooltip
    {
        private readonly SpriteIcon icon;
        public Action OnPressed;

        public FontAwesome Icon
        {
            get { return icon.Icon; }
            set { icon.Icon = value; }
        }

        public TooltipIconButton()
        {
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
                }
            };
        }

        protected override bool OnClick(InputState state)
        {
            OnPressed?.Invoke();
            return base.OnClick(state);
        }

        public string TooltipText { get; set; }
    }
}
