// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API;
using System;

namespace osu.Game.Overlays.Comments
{
    public abstract class ResponseContainer : Container
    {
        private const int corner_radius = 5;

        protected readonly ResponseTextBox TextBox;

        protected readonly Bindable<string> Text = new Bindable<string>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private PostCommentRequest request;
        private readonly PostButton postButton;

        protected ResponseContainer()
        {
            FillFlowContainer<Button> additionalButtonsPlaceholder;

            Height = 60;
            RelativeSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = corner_radius;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f)
                },
                TextBox = new ResponseTextBox
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    PlaceholderText = CreateTextBoxPlaceholder(),
                    Current = Text,
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Padding = new MarginPadding { Horizontal = 10 },
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = @"Press enter to post.",
                            Font = OsuFont.GetFont(size: 14),
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(3, 0),
                            Children = new Drawable[]
                            {
                                additionalButtonsPlaceholder = new FillFlowContainer<Button>
                                {
                                    Direction = FillDirection.Horizontal,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(3, 0),
                                },
                                postButton = CreatePostButton(),
                            }
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    BorderThickness = 3,
                    Masking = true,
                    CornerRadius = corner_radius,
                    BorderColour = OsuColour.Gray(0.2f),
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Transparent
                    }
                }
            };

            var additionalButtons = AddButtons();
            if (additionalButtons != null)
                additionalButtonsPlaceholder.AddRange(additionalButtons);

            TextBox.OnCommit += (u, v) => postButton.Click();
        }

        protected virtual Button[] AddButtons() => null;

        protected abstract string CreateTextBoxPlaceholder();

        protected abstract PostButton CreatePostButton();

        protected abstract PostCommentRequest CreateRequest();

        protected abstract Comment OnSuccess(CommentBundle response);

        protected void OnAction()
        {
            request = CreateRequest();
            request.Success += onSuccess;
            api.Queue(request);
        }

        public Action<Comment> OnResponseReceived;

        private void onSuccess(CommentBundle response)
        {
            postButton.IsLoading = false;
            OnResponseReceived?.Invoke(OnSuccess(response));
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }

        protected class ResponseTextBox : TextBox
        {
            protected override float LeftRightPadding => 10;

            protected override SpriteText CreatePlaceholder() => new SpriteText
            {
                Font = OsuFont.GetFont(),
                Colour = OsuColour.Gray(0.2f),
            };

            public ResponseTextBox()
            {
                TextContainer.Height = 0.5f;
                LengthLimit = 1000;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundUnfocused = BackgroundFocused = colours.Gray2;
            }

            protected override Drawable GetDrawableCharacter(char c) => new SpriteText { Text = c.ToString(), Font = OsuFont.GetFont(size: CalculatedTextSize) };
        }

        protected class Button : LoadingButton
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            private Box background;
            protected SpriteText DrawableText;

            public Button(string buttonText)
            {
                DrawableText.Text = buttonText;

                AutoSizeAxes = Axes.Both;
                LoadingAnimationSize = new Vector2(10);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IdleColour = colours.BlueDark;
                HoverColour = colours.Blue;
            }

            protected override Drawable CreateContent() => new CircularContainer
            {
                Masking = true,
                Height = 20,
                AutoSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    DrawableText = new SpriteText
                    {
                        AlwaysPresent = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Margin = new MarginPadding { Horizontal = 15 },
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
                    }
                }
            };
        }

        protected class PostButton : Button
        {
            private const int duration = 200;

            public readonly Bindable<string> Text = new Bindable<string>();
            protected readonly BindableBool IsReady = new BindableBool();

            public Action ClickAction;

            public PostButton(string name = @"Post")
                : base(name)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                IsReady.BindValueChanged(_ =>
                {
                    Action = ReadyCondition ? ClickAction : null;
                    this.FadeColour(ReadyCondition ? Color4.White : OsuColour.Gray(0.5f), 200, Easing.OutQuint);
                }, true);
                Text.BindValueChanged(_ => IsReady.TriggerChange(), true);
            }

            protected virtual bool ReadyCondition => !string.IsNullOrEmpty(Text.Value);

            protected override void OnLoadStarted() => DrawableText.FadeOut(duration, Easing.OutQuint);

            protected override void OnLoadFinished() => DrawableText.FadeIn(duration, Easing.OutQuint);
        }
    }
}
