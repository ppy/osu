// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2.Footer
{
    public partial class FooterButtonRandomV2 : FooterButtonV2
    {
        public Action? NextRandom { get; set; }
        public Action? PreviousRandom { get; set; }

        private Container persistentText = null!;
        private OsuSpriteText randomSpriteText = null!;
        private OsuSpriteText rewindSpriteText = null!;
        private bool rewindSearch;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            //TODO: use https://fontawesome.com/icons/shuffle?s=solid&f=classic when local Fontawesome is updated
            Icon = FontAwesome.Solid.Random;
            AccentColour = colour.Blue1;
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
                        Font = OsuFont.TorusAlternate.With(size: 19),
                        AlwaysPresent = true,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "Random",
                    },
                    rewindSpriteText = new OsuSpriteText
                    {
                        Font = OsuFont.TorusAlternate.With(size: 19),
                        AlwaysPresent = true,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "Rewind",
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
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Font = OsuFont.TorusAlternate.With(size: 19),
                    });

                    fallingRewind.FadeOutFromOne(fade_time, Easing.In);
                    fallingRewind.MoveTo(Vector2.Zero).MoveTo(new Vector2(0, 10), fade_time, Easing.In);
                    fallingRewind.Expire();

                    persistentText.FadeInFromZero(fade_time, Easing.In);

                    PreviousRandom?.Invoke();
                }
                else
                {
                    NextRandom?.Invoke();
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            updateText(e.ShiftPressed);
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            updateText(e.ShiftPressed);
            base.OnKeyUp(e);
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
            if (e.Button == MouseButton.Right)
            {
                rewindSearch = true;
                TriggerClick();
                return;
            }

            base.OnMouseUp(e);
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

        private void updateText(bool rewind = false)
        {
            randomSpriteText.Alpha = rewind ? 0 : 1;
            rewindSpriteText.Alpha = rewind ? 1 : 0;
        }
    }
}
