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

namespace osu.Game.Overlays.Comments
{
    public class VotePill : LoadingButton, IHasAccentColour
    {
        public Color4 AccentColour { get; set; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        private readonly Comment comment;
        private Box background;
        private Box hoverLayer;
        private CircularContainer borderContainer;
        private SpriteText sideNumber;
        private OsuSpriteText votesCounter;

        public VotePill(Comment comment)
        {
            this.comment = comment;
            votesCounter.Text = $"+{comment.VotesCount}";

            AutoSizeAxes = Axes.X;
            Height = 20;
            LoadingAnimationSize = new Vector2(10);

            Action = () =>
            {

            };
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

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = borderContainer.BorderColour = sideNumber.Colour = colours.GreenLight;
            background.Colour = comment.IsVoted ? AccentColour : OsuColour.Gray(0.05f);
            hoverLayer.Colour = Color4.Black.Opacity(0.5f);
        }

        protected override void OnLoadingStart()
        {
            sideNumber.Hide();
            borderContainer.BorderThickness = 0;
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (comment.IsVoted)
                hoverLayer.Show();

            if (!IsLoading)
            {
                borderContainer.BorderThickness = 3;

                if (!comment.IsVoted)
                    sideNumber.Show();
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            if (comment.IsVoted)
                hoverLayer.Hide();
            else
                sideNumber.Hide();

            borderContainer.BorderThickness = 0;
        }
    }
}
