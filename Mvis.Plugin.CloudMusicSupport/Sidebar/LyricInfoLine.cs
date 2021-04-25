using System;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricInfoContainer : CompositeDrawable
    {
        private readonly Lyric lyric;

        public LyricInfoContainer(Lyric lrc)
        {
            CornerRadius = 5f;
            Masking = true;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            lyric = lrc;
        }

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider colourProvider)
        {
            var timeSpan = TimeSpan.FromMilliseconds(lyric.Time);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Masking = true,
                            CornerRadius = 5,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding(5),
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = colourProvider.Highlight1,
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 17, weight: FontWeight.Bold),
                                    Text = $"{timeSpan:mm\\:ss\\.fff}",
                                    Colour = Color4.Black,
                                    Margin = new MarginPadding { Horizontal = 5, Vertical = 3 }
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Text = lyric.Content,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                        },
                        new OsuSpriteText
                        {
                            Text = lyric.TranslatedString,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Margin = new MarginPadding { Left = 5, Bottom = 5 }
                        },
                    }
                }
            };
        }
    }
}
