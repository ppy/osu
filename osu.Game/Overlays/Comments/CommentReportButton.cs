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
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentReportButton : CompositeDrawable, IHasLineBaseHeight
    {
        private readonly Comment comment;

        private LinkFlowContainer link = null!;
        private LoadingSpinner loading = null!;

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
                loading = new LoadingSpinner
                {
                    Size = new Vector2(12f),
                }
            };

            link.AddLink(ReportStrings.CommentButton.ToLower(), () => dialogOverlay?.Push(createDialog()));
        }

        private CommentReportDialog createDialog()
        {
            var dialog = new CommentReportDialog(comment, colourProvider);

            dialog.Submitted += () =>
            {
                link.Hide();
                loading.Show();
            };

            dialog.Success += () => Schedule(() =>
            {
                loading.Hide();

                link.Clear(true);
                link.AddText(UsersStrings.ReportThanks, s => s.Colour = colourProvider?.Content2 ?? Colour4.White);
                link.Show();

                this.FadeOut(2000, Easing.InQuint).Expire();
            });

            dialog.Failure += () => Schedule(() =>
            {
                loading.Hide();
                link.Show();
            });

            return dialog;
        }

        public float LineBaseHeight => link.ChildrenOfType<IHasLineBaseHeight>().FirstOrDefault()?.LineBaseHeight ?? DrawHeight;
    }
}
