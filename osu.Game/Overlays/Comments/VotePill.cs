// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using osuTK;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Bindables;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public partial class VotePill : LoadingButton, IHasAccentColour
    {
        private const int duration = 200;

        public Color4 AccentColour { get; set; }

        protected override IEnumerable<Drawable> EffectTargets => Enumerable.Empty<Drawable>();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved(canBeNull: true)]
        private LoginOverlay login { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        protected Box Background { get; private set; }

        private readonly Comment comment;

        private Box hoverLayer;
        private CircularContainer borderContainer;
        private SpriteText sideNumber;
        private OsuSpriteText votesCounter;
        private CommentVoteRequest request;

        private readonly BindableBool isVoted = new BindableBool();
        private readonly BindableInt votesCount = new BindableInt();

        public VotePill(Comment comment)
        {
            this.comment = comment;

            AutoSizeAxes = Axes.X;
            Height = 20;
            LoadingAnimationSize = new Vector2(10);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = borderContainer.BorderColour = sideNumber.Colour = colours.GreenLight;
            hoverLayer.Colour = Color4.Black.Opacity(0.5f);

            bool ownComment = api.LocalUser.Value.Id == comment.UserId;

            if (!ownComment)
                Action = onAction;

            Background.Alpha = ownComment ? 0 : 1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            isVoted.Value = comment.IsVoted;
            votesCount.Value = comment.VotesCount;
            isVoted.BindValueChanged(voted => Background.Colour = voted.NewValue ? AccentColour : colourProvider.Background6, true);
            votesCount.BindValueChanged(count => votesCounter.Text = $"+{count.NewValue}", true);
        }

        private void onAction()
        {
            if (!api.IsLoggedIn)
            {
                login?.Show();
                return;
            }

            request = new CommentVoteRequest(comment.Id, isVoted.Value ? CommentVoteAction.UnVote : CommentVoteAction.Vote);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            var receivedComment = response.Comments.Single();
            isVoted.Value = receivedComment.IsVoted;
            votesCount.Value = receivedComment.VotesCount;
            IsLoading = false;
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                borderContainer = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        Background = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        hoverLayer = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0
                        }
                    }
                },
                sideNumber = new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Text = "+1",
                    Font = OsuFont.GetFont(size: 14),
                    Alpha = 0,
                },
                votesCounter = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Margin = new MarginPadding { Horizontal = 10 },
                    Font = OsuFont.GetFont(size: 14),
                    AlwaysPresent = true,
                }
            },
        };

        protected override void OnLoadStarted()
        {
            votesCounter.FadeOut(duration, Easing.OutQuint);
            updateDisplay();
        }

        protected override void OnLoadFinished()
        {
            votesCounter.FadeIn(duration, Easing.OutQuint);

            if (IsHovered)
                onHoverAction();
        }

        protected override bool OnHover(HoverEvent e)
        {
            onHoverAction();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
            base.OnHoverLost(e);
        }

        private void updateDisplay()
        {
            if (Action == null)
                return;

            if (isVoted.Value)
            {
                hoverLayer.FadeTo(IsHovered ? 1 : 0);
                sideNumber.Hide();
            }
            else
                sideNumber.FadeTo(IsHovered ? 1 : 0);

            borderContainer.BorderThickness = IsHovered ? 2 : 0;
        }

        private void onHoverAction()
        {
            if (!IsLoading)
                updateDisplay();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
