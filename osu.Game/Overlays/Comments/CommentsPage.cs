// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using System.Linq;

namespace osu.Game.Overlays.Comments
{
    public class CommentsPage : CompositeDrawable
    {
        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly CommentBundle commentBundle;

        public CommentsPage(CommentBundle commentBundle)
        {
            this.commentBundle = commentBundle;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            FillFlowContainer flow;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                }
            });

            if (!commentBundle.Comments.Any())
            {
                flow.Add(new NoCommentsPlaceholder());
                return;
            }

            foreach (var c in commentBundle.Comments)
            {
                if (c.IsTopLevel)
                {
                    flow.Add(new DrawableComment(c)
                    {
                        ShowDeleted = { BindTarget = ShowDeleted }
                    });
                }
            }
        }

        private class NoCommentsPlaceholder : CompositeDrawable
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Height = 80;
                RelativeSizeAxes = Axes.X;
                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background4
                    },
                    new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding { Left = 50 },
                        Text = @"No comments yet."
                    }
                });
            }
        }
    }
}
