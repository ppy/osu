// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentReportButton : CompositeDrawable, IHasPopover
    {
        private readonly Comment comment;

        private LinkFlowContainer link = null!;
        private LoadingSpinner loading = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        public CommentReportButton(Comment comment)
        {
            this.comment = comment;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                link = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 12, weight: FontWeight.Bold))
                {
                    AutoSizeAxes = Axes.Both,
                },
                loading = new LoadingSpinner
                {
                    Size = new Vector2(12f),
                }
            };

            link.AddLink(ReportStrings.CommentButton.ToLower(), this.ShowPopover);
        }

        public Popover GetPopover() => new ReportCommentPopover(comment)
        {
            Action = report
        };

        private void report(CommentReportReason reason, string comments)
        {
            var request = new CommentReportRequest(comment.Id, reason, comments);

            link.Hide();
            loading.Show();

            request.Success += () => Schedule(() =>
            {
                loading.Hide();

                link.Clear(true);
                link.AddText(UsersStrings.ReportThanks, s => s.Colour = colourProvider?.Content2 ?? Colour4.White);
                link.Show();

                this.FadeOut(2000, Easing.InQuint).Expire();
            });

            request.Failure += _ => Schedule(() =>
            {
                loading.Hide();
                link.Show();
            });

            api.Queue(request);
        }
    }
}
