// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class ParticipantInfo : MultiplayerComposite
    {
        public ParticipantInfo()
        {
            RelativeSizeAxes = Axes.X;
            Height = 15f;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            OsuSpriteText summary;
            OsuSpriteText levelRangeHigher;
            OsuSpriteText levelRangeLower;
            Container flagContainer;
            LinkFlowContainer hostText;

            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5f, 0f),
                    Children = new Drawable[]
                    {
                        flagContainer = new Container
                        {
                            Width = 22f,
                            RelativeSizeAxes = Axes.Y,
                        },
                        /*new Container //todo: team banners
                        {
                            Width = 38f,
                            RelativeSizeAxes = Axes.Y,
                            CornerRadius = 2f,
                            Masking = true,
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.FromHex(@"ad387e"),
                                },
                            },
                        },*/
                        hostText = new LinkFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both
                        }
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Colour = colours.Gray9,
                    Children = new[]
                    {
                        summary = new OsuSpriteText
                        {
                            Text = "0 participants",
                            Font = OsuFont.GetFont(size: 14)
                        }
                    },
                },
            };

            Host.BindValueChanged(host =>
            {
                hostText.Clear();
                flagContainer.Clear();

                if (host.NewValue != null)
                {
                    hostText.AddText("hosted by ");
                    hostText.AddLink(host.NewValue.Username, null, LinkAction.OpenUserProfile, host.NewValue.Id.ToString(), "Open profile",
                        s => s.Font = s.Font.With(Typeface.Exo, weight: FontWeight.Bold, italics: true));
                    flagContainer.Child = new DrawableFlag(host.NewValue.Country) { RelativeSizeAxes = Axes.Both };
                }
            }, true);

            ParticipantCount.BindValueChanged(count => summary.Text = "participant".ToQuantity(count.NewValue), true);

            /*Participants.BindValueChanged(e =>
            {
                var ranks = v.Select(u => u.Statistics.Ranks.Global);
                levelRangeLower.Text = ranks.Min().ToString();
                levelRangeHigher.Text = ranks.Max().ToString();
            });*/
        }
    }
}
