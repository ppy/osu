// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK.Graphics;

namespace osu.Game.Screens.Multi.Components
{
    public class SettingsPasswordTextBox : OsuPasswordTextBox
    {
        private readonly Box labelBox;

        public SettingsPasswordTextBox()
        {
            TextContainer.Margin = new MarginPadding { Left = 80 };

            Content.Add(new Container
            {
                RelativeSizeAxes = Axes.Y,
                Width = 80,
                Children = new Drawable[]
                {
                    labelBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    new OsuSpriteText
                    {
                        Text = @"Password",
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = @"Exo2.0-Bold",
                    },
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            labelBox.Colour = colours.Gray4;
        }

        protected override void OnFocusLost(InputState state)
        {
            base.OnFocusLost(state);

            OnCommit?.Invoke(this, true);
        }
    }
}
