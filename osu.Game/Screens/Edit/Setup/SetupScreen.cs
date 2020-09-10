// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
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
        private FillFlowContainer flow;
        private LabelledTextBox artistTextBox;
        private LabelledTextBox titleTextBox;
        private LabelledTextBox creatorTextBox;
        private LabelledTextBox difficultyTextBox;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(50),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeafoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(10),
                            Child = flow = new FillFlowContainer
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
                                        CornerRadius = 10,
                                        Child = new BeatmapBackgroundSprite(Beatmap.Value)
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            FillMode = FillMode.Fill,
                                        },
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = "谱面元数据"
                                    },
                                    artistTextBox = new LabelledTextBox
                                    {
                                        Label = "艺术家",
                                        Current = { Value = Beatmap.Value.Metadata.Artist },
                                        TabbableContentContainer = this
                                    },
                                    titleTextBox = new LabelledTextBox
                                    {
                                        Label = "标题",
                                        Current = { Value = Beatmap.Value.Metadata.Title },
                                        TabbableContentContainer = this
                                    },
                                    creatorTextBox = new LabelledTextBox
                                    {
                                        Label = "作图者",
                                        Current = { Value = Beatmap.Value.Metadata.AuthorString },
                                        TabbableContentContainer = this
                                    },
                                    difficultyTextBox = new LabelledTextBox
                                    {
                                        Label = "难度名",
                                        Current = { Value = Beatmap.Value.BeatmapInfo.Version },
                                        TabbableContentContainer = this
                                    },
                                }
                            },
                        },
                    }
                }
            };

            foreach (var item in flow.OfType<LabelledTextBox>())
                item.OnCommit += onCommit;
        }

        private void onCommit(TextBox sender, bool newText)
        {
            if (!newText) return;

            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Value.Metadata.Artist = artistTextBox.Current.Value;
            Beatmap.Value.Metadata.Title = titleTextBox.Current.Value;
            Beatmap.Value.Metadata.AuthorString = creatorTextBox.Current.Value;
            Beatmap.Value.BeatmapInfo.Version = difficultyTextBox.Current.Value;
        }
    }
}
