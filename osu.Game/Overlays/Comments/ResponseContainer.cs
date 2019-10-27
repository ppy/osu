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
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class ResponseContainer : Container
    {
        private const int height = 60;
        private const int corner_radius = 5;

        public readonly BindableBool Expanded = new BindableBool();

        private readonly Bindable<string> text = new Bindable<string>();

        private readonly Comment comment;
        private readonly ReplyButton replyButton;
        private readonly ResponseTextBox textBox;

        [Resolved]
        private IAPIProvider api { get; set; }

        private PostCommentRequest request;

        public ResponseContainer(Comment comment)
        {
            this.comment = comment;

            Height = height;
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
                textBox = new ResponseTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height / 2f,
                    PlaceholderText = @"Type your response here",
                    Current = text,
                },
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = height / 2f,
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
                                new CancelButton
                                {
                                    Expanded = { BindTarget = Expanded }
                                },
                                replyButton = new ReplyButton
                                {
                                    ClickAction = onAction,
                                    Expanded = { BindTarget = Expanded },
                                    Text = { BindTarget = text }
                                }
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Expanded.BindValueChanged(expanded =>
            {
                if (expanded.NewValue)
                    Show();
                else
                {
                    Hide();
                    textBox.Current.Value = string.Empty;
                }
            }, true);
        }

        private void onAction()
        {
            request = new PostCommentRequest(comment.CommentableId, comment.CommentableType, textBox.Current.Value, comment.Id);
            request.Success += onSuccess;
            api.Queue(request);
        }

        public Action<Comment> OnResponseReceived;

        private void onSuccess(CommentBundle response)
        {
            replyButton.IsLoading = false;
            Comment newReply = response.Comments.First();
            newReply.ParentComment = comment;
            OnResponseReceived?.Invoke(newReply);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }

        private class ResponseTextBox : TextBox
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

        private class Button : LoadingButton
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { background };

            public readonly BindableBool Expanded = new BindableBool();

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

        private class CancelButton : Button
        {
            public CancelButton()
                : base(@"Cancel")
            {
                Action = () => Expanded.Value = false;
            }

            protected override void OnLoadStarted() => IsLoading = false;
        }

        private class ReplyButton : Button
        {
            private const int duration = 200;

            public readonly Bindable<string> Text = new Bindable<string>();
            public Action ClickAction;

            public ReplyButton()
                : base(@"Reply")
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Text.BindValueChanged(onTextChanged, true);
            }

            private void onTextChanged(ValueChangedEvent<string> text)
            {
                Action = string.IsNullOrEmpty(text.NewValue) ? null : ClickAction;
                this.FadeColour(string.IsNullOrEmpty(text.NewValue) ? OsuColour.Gray(0.5f) : Color4.White, 200, Easing.OutQuint);
            }

            protected override void OnLoadStarted() => DrawableText.FadeOut(duration, Easing.OutQuint);

            protected override void OnLoadFinished()
            {
                DrawableText.FadeIn(duration, Easing.OutQuint);
                Expanded.Value = false;
            }
        }
    }
}
