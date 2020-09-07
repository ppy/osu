// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreen : EditorScreen
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray0,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(50),
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(20),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 250,
                                Masking = true,
                                CornerRadius = 50,
                                Child = new BeatmapBackgroundSprite(Beatmap.Value)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fill,
                                },
                            },
                            new OsuSpriteText
                            {
                                Text = "Beatmap metadata"
                            },
                            new LabelledTextBox
                            {
                                Label = "Artist",
                                Current = { Value = Beatmap.Value.Metadata.Artist }
                            },
                            new LabelledTextBox
                            {
                                Label = "Title",
                                Current = { Value = Beatmap.Value.Metadata.Title }
                            },
                            new LabelledTextBox
                            {
                                Label = "Creator",
                                Current = { Value = Beatmap.Value.Metadata.AuthorString }
                            },
                            new LabelledTextBox
                            {
                                Label = "Difficulty Name",
                                Current = { Value = Beatmap.Value.BeatmapInfo.Version }
                            },
                        }
                    },
                },
            };
        }
    }
}
