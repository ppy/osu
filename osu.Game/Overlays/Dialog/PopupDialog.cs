// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Dialog
{
    public abstract partial class PopupDialog : VisibilityContainer
    {
        public const float ENTER_DURATION = 500;
        public const float EXIT_DURATION = 500;

        private readonly Vector2 ringSize = new Vector2(100f);
        private readonly Vector2 ringMinifiedSize = new Vector2(20f);

        private readonly Box flashLayer;
        private Sample flashSample = null!;

        private readonly Container content;
        private readonly Container ring;
        private readonly FillFlowContainer<PopupDialogButton> buttonsContainer;
        private readonly SpriteIcon icon;
        private readonly TextFlowContainer header;
        private readonly TextFlowContainer body;

        private bool actionInvoked;

        public IconUsage Icon
        {
            get => icon.Icon;
            set => icon.Icon = value;
        }

        private LocalisableString headerText;

        public LocalisableString HeaderText
        {
            get => headerText;
            set
            {
                if (headerText == value)
                    return;

                headerText = value;
                header.Text = value;
            }
        }

        private LocalisableString bodyText;

        public LocalisableString BodyText
        {
            get => bodyText;
            set
            {
                if (bodyText == value)
                    return;

                bodyText = value;
                body.Text = value;
            }
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

                        // Hide the dialog before running the action.
                        // This is important as the code which is performed may check for a dialog being present (ie. `OsuGame.PerformFromScreen`)
                        // and we don't want it to see the already dismissed dialog.
                        Hide();

                        action?.Invoke();
                    };
                }
            }
        }

        protected PopupDialog()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 20,
                            CornerExponent = 2.5f,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.2f),
                                Radius = 14,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4Extensions.FromHex(@"221a21"),
                                },
                                new Triangles
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourLight = Color4Extensions.FromHex(@"271e26"),
                                    ColourDark = Color4Extensions.FromHex(@"1e171e"),
                                    TriangleScale = 4,
                                },
                                flashLayer = new Box
                                {
                                    Alpha = 0,
                                    RelativeSizeAxes = Axes.Both,
                                    Blending = BlendingParameters.Additive,
                                    Colour = Color4Extensions.FromHex(@"221a21"),
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 10f),
                            Padding = new MarginPadding { Vertical = 60 },
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Padding = new MarginPadding { Bottom = 30 },
                                    Size = ringSize,
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
                                                    Icon = FontAwesome.Solid.TimesCircle,
                                                    Y = -2,
                                                    Size = new Vector2(50),
                                                },
                                            },
                                        },
                                    },
                                },
                                header = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 25))
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    TextAnchor = Anchor.TopCentre,
                                    Padding = new MarginPadding { Horizontal = 5 },
                                },
                                body = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 18))
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    TextAnchor = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = 5 },
                                },
                                buttonsContainer = new FillFlowContainer<PopupDialogButton>
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding { Top = 30 },
                                },
                            },
                        },
                    },
                },
            };

            // It's important we start in a visible state so our state fires on hide, even before load.
            // This is used by the dialog overlay to know when the dialog was dismissed.
            Show();
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            flashSample = audio.Samples.Get(@"UI/default-select-disabled");
        }

        /// <summary>
        /// Programmatically clicks the first <see cref="PopupDialogOkButton"/>.
        /// </summary>
        public void PerformOkAction() => PerformAction<PopupDialogOkButton>();

        /// <summary>
        /// Programmatically clicks the first button of the provided type.
        /// </summary>
        public void PerformAction<T>() where T : PopupDialogButton
        {
            // Buttons are regularly added in BDL or LoadComplete, so let's schedule to ensure
            // they are ready to be pressed.
            Scheduler.AddOnce(() => Buttons.OfType<T>().FirstOrDefault()?.TriggerClick());
        }

        public void Flash()
        {
            flashLayer.FadeInFromZero(80, Easing.OutQuint)
                      .Then()
                      .FadeOutFromOne(1500, Easing.OutQuint);
            flashSample.Play();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            // press button at number if 1-9 on number row or keypad are pressed
            var k = e.Key;

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

            return base.OnKeyDown(e);
        }

        protected override void PopIn()
        {
            actionInvoked = false;

            // Reset various animations but only if the dialog animation fully completed
            if (content.Alpha == 0)
            {
                content.ScaleTo(0.7f);
                ring.ResizeTo(ringMinifiedSize);
            }

            content
                .ScaleTo(1, 750, Easing.OutElasticHalf)
                .FadeIn(ENTER_DURATION, Easing.OutQuint);

            ring.ResizeTo(ringSize, ENTER_DURATION * 1.5f, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            if (!actionInvoked)
                // In the case a user did not choose an action before a hide was triggered, press the last button.
                // This is presumed to always be a sane default "cancel" action.
                buttonsContainer.Last().TriggerClick();

            content
                .ScaleTo(0.7f, EXIT_DURATION, Easing.Out)
                .FadeOut(EXIT_DURATION, Easing.OutQuint);
        }

        private void pressButtonAtIndex(int index)
        {
            if (index < Buttons.Count())
                Buttons.Skip(index).First().TriggerClick();
        }
    }
}
