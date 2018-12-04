// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.AccountCreation;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays
{
    public class AccountCreationOverlay : OsuFocusedOverlayContainer, IOnlineComponent
    {
        private OsuTextFlowContainer usernameDescription;
        private OsuTextFlowContainer emailAddressDescription;
        private OsuTextFlowContainer passwordDescription;

        private OsuTextBox usernameTextBox;
        private OsuTextBox emailTextBox;
        private OsuPasswordTextBox passwordTextBox;

        private APIAccess api;
        private ShakeContainer registerShake;
        private IEnumerable<SpriteText> characterCheckText;

        private const float transition_time = 400;

        public AccountCreationOverlay()
        {
            Size = new Vector2(620, 450);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            State = Visibility.Visible;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, APIAccess api)
        {
            this.api = api;

            api.Register(this);

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.6f,
                        },
                        new DelayedLoadWrapper(new AccountCreationBackground(), 0),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.6f,
                            AutoSizeDuration = transition_time,
                            AutoSizeEasing = Easing.OutQuint,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.9f,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Padding = new MarginPadding(20),
                                    Spacing = new Vector2(0, 10),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            TextSize = 20,
                                            Margin = new MarginPadding { Vertical = 10 },
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Text = "Let's create an account!",
                                        },
                                        usernameTextBox = new OsuTextBox
                                        {
                                            PlaceholderText = "username",
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this
                                        },
                                        usernameDescription = new OsuTextFlowContainer(cp => { cp.TextSize = 12; })
                                        {
                                            RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y
                                        },
                                        emailTextBox = new OsuTextBox
                                        {
                                            PlaceholderText = "email address",
                                            RelativeSizeAxes = Axes.X,
                                            Text = api.ProvidedUsername ?? string.Empty,
                                            TabbableContentContainer = this
                                        },
                                        emailAddressDescription = new OsuTextFlowContainer(cp => { cp.TextSize = 12; })
                                        {
                                            RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y
                                        },
                                        passwordTextBox = new OsuPasswordTextBox
                                        {
                                            PlaceholderText = "password",
                                            RelativeSizeAxes = Axes.X,
                                            TabbableContentContainer = this,
                                        },
                                        passwordDescription = new OsuTextFlowContainer(cp => { cp.TextSize = 12; })
                                        {
                                            RelativeSizeAxes = Axes.X, AutoSizeAxes = Axes.Y
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new Drawable[]
                                            {
                                                registerShake = new ShakeContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Child = new SettingsButton
                                                    {
                                                        Text = "Register",
                                                        Margin = new MarginPadding { Vertical = 20 },
                                                        Action = performRegistration
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        }
                    }
                }
            };

            usernameDescription.AddText("This will be your public presence. No profanity, no impersonation. Avoid exposing your own personal details, too!");

            emailAddressDescription.AddText("Will be used for notifications, account verification and in the case you forget your password. No spam, ever.");
            emailAddressDescription.AddText(" Make sure to get it right!", cp => cp.Font = "Exo2.0-Bold");

            passwordDescription.AddText("At least ");
            characterCheckText = passwordDescription.AddText("8 characters long");
            passwordDescription.AddText(". Choose something long but also something you will remember, like a line from your favourite song.");

            passwordTextBox.Current.ValueChanged += text => {
                characterCheckText.ForEach(s => s.Colour = text.Length == 0 ? Color4.White : Interpolation.ValueAt(text.Length, Color4.OrangeRed, Color4.YellowGreen, 0, 8, Easing.In));
            };
        }

        private void performRegistration()
        {
            var textbox = nextUnfilledTextbox();

            if (textbox != null)
            {
                Schedule(() => GetContainingInputManager().ChangeFocus(textbox));
                registerShake.Shake();
                return;
            }

            api.CreateAccount(emailTextBox.Text, usernameTextBox.Text, passwordTextBox.Text);
        }

        private OsuTextBox nextUnfilledTextbox()
        {
            OsuTextBox textboxIfUsable(OsuTextBox textbox) => !string.IsNullOrEmpty(textbox.Text) ? null : textbox;

            return textboxIfUsable(usernameTextBox) ?? textboxIfUsable(emailTextBox) ?? textboxIfUsable(passwordTextBox);
        }

        protected override void PopIn()
        {
            base.PopIn();
            this.FadeIn(transition_time, Easing.OutQuint);

            var nextTextbox = nextUnfilledTextbox();
            if (nextTextbox != null)
                Schedule(() => GetContainingInputManager().ChangeFocus(nextTextbox));
        }

        protected override void PopOut()
        {
            base.PopOut();
            this.FadeOut(100);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Offline:
                case APIState.Failing:
                    break;
                case APIState.Connecting:
                    break;
                case APIState.Online:
                    break;
            }
        }
    }
}
