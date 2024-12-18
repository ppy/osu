// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public partial class FooterButtonRandom : FooterButton
    {
        public Action NextRandom { get; set; }
        public Action PreviousRandom { get; set; }

        private Container persistentText;
        private OsuSpriteText randomSpriteText;
        private OsuSpriteText rewindSpriteText;
        private bool rewindSearch;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Green;
            DeselectedColour = SelectedColour.Opacity(0.5f);

            TextContainer.Add(persistentText = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AlwaysPresent = true,
                AutoSizeAxes = Axes.Both,
                Children = new[]
                {
                    randomSpriteText = new OsuSpriteText
                    {
                        AlwaysPresent = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "random",
                    },
                    rewindSpriteText = new OsuSpriteText
                    {
                        AlwaysPresent = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Text = "rewind",
                        Alpha = 0f,
                    }
                }
            });

            Action = () =>
            {
                if (rewindSearch)
                {
                    const double fade_time = 500;

                    OsuSpriteText fallingRewind;

                    TextContainer.Add(fallingRewind = new OsuSpriteText
                    {
                        Alpha = 0,
                        Text = rewindSpriteText.Text,
                        AlwaysPresent = true, // make sure the button is sized large enough to always show this
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });

                    fallingRewind.FadeOutFromOne(fade_time, Easing.In);
                    fallingRewind.MoveTo(Vector2.Zero).MoveTo(new Vector2(0, 10), fade_time, Easing.In);
                    fallingRewind.Expire();

                    persistentText.FadeInFromZero(fade_time, Easing.In);

                    PreviousRandom.Invoke();
                }
                else
                {
                    NextRandom.Invoke();
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            updateText(e);
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            updateText(e);
            base.OnKeyUp(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            updateText(e);
            return base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            try
            {
                // this uses OR to handle rewinding when clicks are triggered by other sources (i.e. right button in OnMouseUp).
                rewindSearch |= e.ShiftPressed;
                return base.OnClick(e);
            }
            finally
            {
                rewindSearch = false;
            }
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Right && IsHovered)
            {
                rewindSearch = true;
                TriggerClick();
            }

            updateText(e);
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            rewindSearch = e.Action == GlobalAction.SelectPreviousRandom;

            if (e.Action != GlobalAction.SelectNextRandom && e.Action != GlobalAction.SelectPreviousRandom)
            {
                return false;
            }

            if (!e.Repeat)
                TriggerClick();
            return true;
        }

        public override void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.SelectPreviousRandom)
            {
                rewindSearch = false;
            }
        }

        private void updateText(UIEvent e)
        {
            bool aboutToRewind = e.ShiftPressed || e.CurrentState.Mouse.IsPressed(MouseButton.Right);

            randomSpriteText.Alpha = aboutToRewind ? 0 : 1;
            rewindSpriteText.Alpha = aboutToRewind ? 1 : 0;
        }
    }
}
