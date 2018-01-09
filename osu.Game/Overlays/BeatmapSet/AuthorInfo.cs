// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.BeatmapSet
{
    public class AuthorInfo : Container
    {
        private const float height = 50;

        private readonly UpdateableAvatar avatar;
        private readonly ClickableArea clickableArea;
        private readonly FillFlowContainer fields;

        private UserProfileOverlay profile;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                var i = BeatmapSet.OnlineInfo;

                avatar.User = BeatmapSet.Metadata.Author;
                clickableArea.Action = () => profile?.ShowUser(avatar.User);

                fields.Children = new Drawable[]
                {
                    new Field("made by", BeatmapSet.Metadata.Author.Username, @"Exo2.0-RegularItalic"),
                    new Field("submitted on", i.Submitted.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold")
                    {
                        Margin = new MarginPadding { Top = 5 },
                    },
                };

                if (i.Ranked.HasValue)
                {
                    fields.Add(new Field("ranked on ", i.Ranked.Value.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold"));
                }
                else if (i.LastUpdated.HasValue)
                {
                    fields.Add(new Field("last updated on ", i.LastUpdated.Value.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold"));
                }
            }
        }

        public AuthorInfo()
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                clickableArea = new ClickableArea
                {
                    AutoSizeAxes = Axes.Both,
                    CornerRadius = 3,
                    Masking = true,
                    Child = avatar = new UpdateableAvatar
                    {
                        Size = new Vector2(height),
                    },
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.25f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
                        Offset = new Vector2(0f, 1f),
                    },
                },
                fields = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = height + 5 },
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay profile)
        {
            this.profile = profile;
            clickableArea.Action = () => profile?.ShowUser(avatar.User);
        }

        private class Field : FillFlowContainer
        {
            public Field(string first, string second, string secondFont)
            {
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;

                Children = new[]
                {
                    new OsuSpriteText
                    {
                        Text = $"{first} ",
                        TextSize = 13,
                    },
                    new OsuSpriteText
                    {
                        Text = second,
                        TextSize = 13,
                        Font = secondFont,
                    },
                };
            }
        }

        private class ClickableArea : OsuClickableContainer, IHasTooltip
        {
            public string TooltipText => @"View Profile";
        }
    }
}
