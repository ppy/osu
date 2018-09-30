// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Overlays.Dialog
{
    public class PopupDialog : OsuFocusedOverlayContainer
    {
        public static readonly float ENTER_DURATION = 500;
        public static readonly float EXIT_DURATION = 200;

        protected override bool BlockPositionalInput => false;

        private readonly Vector2 ringSize = new Vector2(100f);
        private readonly Vector2 ringMinifiedSize = new Vector2(20f);
        private readonly Vector2 buttonsEnterSpacing = new Vector2(0f, 50f);

        private readonly Container content;
        private readonly Container ring;
        private readonly FillFlowContainer<PopupDialogButton> buttonsContainer;
        private readonly SpriteIcon icon;
        private readonly SpriteText header;
        private readonly TextFlowContainer body;

        private bool actionInvoked;

        public FontAwesome Icon
        {
            get => icon.Icon;
            set => icon.Icon = value;
        }

        public string HeaderText
        {
            get => header.Text;
            set => header.Text = value;
        }

        public string BodyText
        {
            set => body.Text = value;
        }

        public IEnumerable<PopupDialogButton> Buttons
        {
            get => buttonsContainer.Children;
            set
            {
                buttonsContainer.ChildrenEnumerable = value;
                foreach (PopupDialogButton b in value)
                {
                    var action = b.Action;
                    b.Action = () =>
                    {
                        if (actionInvoked) return;

                        actionInvoked = true;
                        action?.Invoke();

                        Hide();
                    };
                }
            }
        }

        public PopupDialog()
        {
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.5f),
                                Radius = 8,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex(@"221a21"),
                                },
                                new Triangles
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourLight = OsuColour.FromHex(@"271e26"),
                                    ColourDark = OsuColour.FromHex(@"1e171e"),
                                    TriangleScale = 4,
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Position = new Vector2(0f, -50f),
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 10f),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Size = ringSize,
                                    Margin = new MarginPadding
                                    {
                                        Bottom = 30,
                                    },
                                    Children = new Drawable[]
                                    {
                                        ring = new CircularContainer
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            Masking = true,
                                            BorderColour = Color4.White,
                                            BorderThickness = 5f,
                                            Children = new Drawable[]
                                            {
                                                new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.Black.Opacity(0),
                                                },
                                                icon = new SpriteIcon
                                                {
                                                    Origin = Anchor.Centre,
                                                    Anchor = Anchor.Centre,
                                                    Icon = FontAwesome.fa_close,
                                                    Size = new Vector2(50),
                                                },
                                            },
                                        },
                                    },
                                },
                                header = new OsuSpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextSize = 25,
                                    Shadow = true,
                                },
                                body = new OsuTextFlowContainer(t => t.TextSize = 18)
                                {
                                    Padding = new MarginPadding(15),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    TextAnchor = Anchor.TopCentre,
                                },
                            },
                        },
                        buttonsContainer = new FillFlowContainer<PopupDialogButton>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        },
                    },
                },
            };
        }

        public override bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Select:
                    Buttons.OfType<PopupDialogOkButton>().FirstOrDefault()?.Click();
                    return true;
            }

            return base.OnPressed(action);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            // press button at number if 1-9 on number row or keypad are pressed
            var k = args.Key;
            if (k >= Key.Number1 && k <= Key.Number9)
            {
                pressButtonAtIndex(k - Key.Number1);
                return true;
            }

            if (k >= Key.Keypad1 && k <= Key.Keypad9)
            {
                pressButtonAtIndex(k - Key.Keypad1);
                return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            base.PopIn();

            actionInvoked = false;

            // Reset various animations but only if the dialog animation fully completed
            if (content.Alpha == 0)
            {
                buttonsContainer.TransformSpacingTo(buttonsEnterSpacing);
                buttonsContainer.MoveToY(buttonsEnterSpacing.Y);
                ring.ResizeTo(ringMinifiedSize);
            }

            content.FadeIn(ENTER_DURATION, Easing.OutQuint);
            ring.ResizeTo(ringSize, ENTER_DURATION, Easing.OutQuint);
            buttonsContainer.TransformSpacingTo(Vector2.Zero, ENTER_DURATION, Easing.OutQuint);
            buttonsContainer.MoveToY(0, ENTER_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            if (!actionInvoked)
                // In the case a user did not choose an action before a hide was triggered, press the last button.
                // This is presumed to always be a sane default "cancel" action.
                buttonsContainer.Last().Click();

            base.PopOut();
            content.FadeOut(EXIT_DURATION, Easing.InSine);
        }

        private void pressButtonAtIndex(int index)
        {
            if (index < Buttons.Count())
                Buttons.Skip(index).First().Click();
        }
    }
}
