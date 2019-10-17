// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class VotePill : LoadingButton, IHasAccentColour
    {
        public Color4 AccentColour { get; set; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Comment comment;
        private Box background;
        private Box hoverLayer;
        private CircularContainer borderContainer;
        private SpriteText sideNumber;
        private OsuSpriteText votesCounter;
        private CommentVoteRequest request;

        private readonly BindableBool isVoted = new BindableBool();

        public VotePill(Comment comment)
        {
            this.comment = comment;
            setCount(comment.VotesCount);

            Action = onAction;

            AutoSizeAxes = Axes.X;
            Height = 20;
            LoadingAnimationSize = new Vector2(10);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = borderContainer.BorderColour = sideNumber.Colour = colours.GreenLight;
            hoverLayer.Colour = Color4.Black.Opacity(0.5f);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            isVoted.Value = comment.IsVoted;
            isVoted.BindValueChanged(voted => background.Colour = voted.NewValue ? AccentColour : OsuColour.Gray(0.05f), true);
        }

        private void onAction()
        {
            request = new CommentVoteRequest(comment.Id, isVoted.Value ? CommentVoteAction.UnVote : CommentVoteAction.Vote);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void onSuccess(CommentBundle response)
        {
            isVoted.Value = !isVoted.Value;
            setCount(response.Comments.First().VotesCount);
            IsLoading = false;
        }

        protected override Container CreateBackground() => new Container
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
                        background = new Box
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
                sideNumber = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = "+1",
                    Font = OsuFont.GetFont(size: 14),
                    Margin = new MarginPadding { Right = 3 },
                    Alpha = 0,
                },
            },
        };

        protected override Drawable CreateContent() => votesCounter = new OsuSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Margin = new MarginPadding { Horizontal = 10 },
            Font = OsuFont.GetFont(size: 14),
            AlwaysPresent = true,
        };

        protected override void OnLoadingStart() => onHoverLostAction();

        protected override void OnLoadingFinished()
        {
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
            onHoverLostAction();
            base.OnHoverLost(e);
        }

        private void onHoverLostAction()
        {
            if (isVoted.Value)
                hoverLayer.Hide();
            else
                sideNumber.Hide();

            borderContainer.BorderThickness = 0;
        }

        private void onHoverAction()
        {
            if (!IsLoading)
            {
                borderContainer.BorderThickness = 3;

                if (!isVoted.Value)
                    sideNumber.Show();
                else
                    hoverLayer.Show();
            }
        }

        private void setCount(int count) => votesCounter.Text = $"+{count}";

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
