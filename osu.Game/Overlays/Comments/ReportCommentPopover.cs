// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public class ReportCommentPopover : OsuPopover
    {
        public readonly long ID;
        private LabelledEnumDropdown<CommentReportReason> reason = null!;
        private LabelledTextBox info = null!;
        private ShakeContainer shaker = null!;

        public ReportCommentPopover(long id)
        {
            ID = id;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Width = 500,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(7),
                Children = new Drawable[]
                {
                    reason = new LabelledEnumDropdown<CommentReportReason>
                    {
                        Label = "Reason"
                    },
                    info = new LabelledTextBox
                    {
                        Label = "Additional info",
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = shaker = new ShakeContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new RoundedButton
                            {
                                BackgroundColour = colours.Pink3,
                                Text = "Send report",
                                RelativeSizeAxes = Axes.X,
                                Action = send
                            }
                        }
                    }
                }
            };
        }

        private void send()
        {
            string infoValue = info.Current.Value;
            var reasonValue = reason.Current.Value;

            if (reasonValue == CommentReportReason.Other && string.IsNullOrWhiteSpace(infoValue))
            {
                shaker.Shake();
                return;
            }
        }
    }
}
