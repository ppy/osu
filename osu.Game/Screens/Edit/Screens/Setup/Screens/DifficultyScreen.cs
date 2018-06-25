// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.SearchableList;
using osu.Game.Screens.Edit.Screens.Setup.BottomHeaders;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledBoxes;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Edit.Screens.Setup.Screens
{
    public class DifficultyScreen : EditorScreen
    {
        private readonly Container content;

        private readonly LabelledSliderBar hpDrainRate;
        private readonly LabelledSliderBar overallDifficulty;
        private readonly LabelledSliderBar circleSize;
        private readonly LabelledSliderBar approachRate;

        public string Title => "Difficulty";

        public DifficultyScreen()
        {
            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding { Left = 75, Top = 200 },
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(3),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Colour = Color4.White,
                                    Text = "Difficulty",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                hpDrainRate = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    SliderMinValue = 0,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    LabelText = "HP Drain Rate",
                                    BottomLabelText = "The constant rate of health-bar drain throughout the song",
                                    SliderBarValueChangedAction = a => Beatmap.Value.BeatmapInfo.BaseDifficulty.DrainRate = a
                                },
                                overallDifficulty = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    SliderMinValue = 0,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 0.1f,
                                    LabelText = "Overall Difficulty",
                                    BottomLabelText = "The harshness of the hit window",
                                    SliderBarValueChangedAction = a => Beatmap.Value.BeatmapInfo.BaseDifficulty.OverallDifficulty = a
                                }
                            }
                        },
                        new DifficultyScreenBottomHeader
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Left = 75, Top = -60 },
                        }
                    },
                },
            };
        }
    }
}
