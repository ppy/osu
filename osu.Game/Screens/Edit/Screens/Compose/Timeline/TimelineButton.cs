// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class TimelineButton : CompositeDrawable
    {
        public Action Action;
        public readonly BindableBool Enabled = new BindableBool(true);

        public FontAwesome Icon
        {
            get { return button.Icon; }
            set { button.Icon = value; }
        }

        private readonly IconButton button;

        public TimelineButton()
        {
            InternalChild = button = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                IconColour = OsuColour.Gray(0.35f),
                IconHoverColour = Color4.White,
                HoverColour = OsuColour.Gray(0.25f),
                FlashColour = OsuColour.Gray(0.5f),
                Action = () => Action?.Invoke()
            };

            button.Enabled.BindTo(Enabled);
            Width = button.ButtonSize.X;
        }

        protected override void Update()
        {
            base.Update();

            button.ButtonSize = new Vector2(button.ButtonSize.X, DrawHeight);
        }
    }
}
