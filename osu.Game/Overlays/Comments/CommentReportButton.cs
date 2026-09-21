// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentReportButton : CompositeDrawable, IHasLineBaseHeight
    {
        private readonly Comment comment;

        private LinkFlowContainer link = null!;

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

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
            };

            link.AddLink(ReportStrings.CommentButton.ToLower(), () =>
            {
                dialogOverlay?.Push(new ReportCommentDialog(comment)
                {
                    Success = () => Schedule(() =>
                    {
                        link.Clear(true);
                        link.AddText(UsersStrings.ReportThanks, s => s.Colour = colourProvider?.Content2 ?? Colour4.White);
                        link.Show();

                        this.FadeOut(2000, Easing.InQuint).Expire();
                    }),
                });
            });
        }

        public float LineBaseHeight => link.ChildrenOfType<IHasLineBaseHeight>().FirstOrDefault()?.LineBaseHeight ?? DrawHeight;
    }
}
