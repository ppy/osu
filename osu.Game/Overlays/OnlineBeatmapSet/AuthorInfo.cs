// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;

namespace osu.Game.Overlays.OnlineBeatmapSet
{
    public class AuthorInfo : Container
    {
        private const float height = 50;

        public AuthorInfo(BeatmapSetOnlineInfo info)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;

            FillFlowContainer fields;
            Children = new Drawable[]
            {
                new UpdateableAvatar
                {
                    Size = new Vector2(height),
                    CornerRadius = 3,
                    Masking = true,
                    User = info.Author,
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
                    Children = new Drawable[]
                    {
                        new Field("made by", info.Author.Username, @"Exo2.0-RegularItalic"),
                        new Field("submitted on", info.Submitted.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold")
                        {
                            Margin = new MarginPadding { Top = 5 },
                        },
                    },
                },
            };

            if (info.Ranked.HasValue)
            {
                fields.Add(new Field("ranked on ", info.Ranked.Value.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold"));
            }
            else if (info.LastUpdated.HasValue)
            {
                fields.Add(new Field("last updated on ", info.LastUpdated.Value.ToString(@"MMM d, yyyy"), @"Exo2.0-Bold"));
            }
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
    }
}
