// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Profile
{
    public partial class ProfileProcessingNotice : CompositeDrawable
    {
        private ILocalisedBindableString noticeText = null!;

        [BackgroundDependencyLoader]
        private void load(IAPIProvider? api, OverlayColourProvider colourProvider, OsuColour colours, LocalisationManager localisation)
        {
            if (string.IsNullOrEmpty(api?.ScoreProcessingNoticeUrl))
                return;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            LinkFlowContainer flow;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Vertical = 10, Horizontal = 50 },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            CornerRadius = 5,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Background4,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                flow = new LinkFlowContainer(cp =>
                                {
                                    cp.Colour = colours.Orange1;
                                    cp.Font = OsuFont.Style.Body;
                                })
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(10),
                                }
                            }
                        },
                    }
                },
            };

            var noticeLink = LocalisableString.Interpolate($@"[{UsersStrings.ShowScoreProcessingTitleLink}]({api.ScoreProcessingNoticeUrl})");
            var noticeString = LocalisableString.Interpolate($@"{UsersStrings.ShowScoreProcessingTitle(noticeLink)} {UsersStrings.ShowScoreProcessingMessage}");
            noticeText = localisation.GetLocalisedBindableString(noticeString);
            noticeText.BindValueChanged(t =>
            {
                flow.Clear();
                flow.AddIcon(FontAwesome.Solid.InfoCircle, cp => cp.Padding = new MarginPadding { Right = 5 });
                var result = MessageFormatter.FormatText(t.NewValue);
                flow.AddLinks(result.Text, result.Links);
            }, true);
        }
    }
}
