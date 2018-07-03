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
    public class AdvancedScreen : EditorScreen
    {
        private readonly Container content;

        private readonly LabelledSliderBar stackLeniency;
        private readonly LabelledCheckBox maniaSpecialStyle;

        public string Title => "Advanced";

        public AdvancedScreen()
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
                                    Text = "Stacking",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                stackLeniency = new LabelledSliderBar
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    SliderMinValue = 2,
                                    SliderMaxValue = 10,
                                    SliderNormalPrecision = 1,
                                    SliderAlternatePrecision = 1,
                                    LeftTickCaption = "Rarely Stack",
                                    RightTickCaption = "Always Stack",
                                    LabelText = "Stack Leniency",
                                    BottomLabelText = "In osu!, this value determines the time distance between notes with the same position that will be snapped.",
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.White,
                                    Text = "Mode",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                maniaSpecialStyle = new LabelledCheckBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    LabelText = "osu!mania Special Style",
                                    BottomLabelText = "Use N+1 key style for osu!mania maps.",
                                    Alpha = 0
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.Yellow,
                                    Text = "Generally it is preferable to have multiple maps for all gamemodes.",
                                    TextSize = 14,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                new OsuSpriteText
                                {
                                    //Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.Yellow,
                                    Text = "It is often encouraged that you create mode-specific maps, since converted beatmaps often do not offer the desirable gameplay.",
                                    TextSize = 14,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                                new OsuSpriteText
                                {
                                    //Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.Yellow,
                                    Text = "Please take that into consideration before submitting your beatmap set.",
                                    TextSize = 14,
                                    Font = @"Exo2.0-BoldItalic",
                                },
                            }
                        },
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            stackLeniency.SliderBarValueChanged += a => Beatmap.Value.BeatmapInfo.StackLeniency = a;
            maniaSpecialStyle.RadioButtonValueChanged += a => Beatmap.Value.BeatmapInfo.SpecialStyle = a;
        }

        public void ChangeManiaSpecialStyle(bool newValue) => maniaSpecialStyle.CurrentValue = newValue;
        public void ChangeStackLeniency(float newValue) => stackLeniency.CurrentValue = newValue;
        public void ChangeBeatmapRuleset(int newRulesetID)
        {
            Beatmap.Value.BeatmapInfo.RulesetID = newRulesetID;
            updateInfo();
        }

        private void updateInfo()
        {
            stackLeniency.CurrentValue = Beatmap.Value?.BeatmapInfo.StackLeniency ?? 7;
            maniaSpecialStyle.CurrentValue = Beatmap.Value?.BeatmapInfo.SpecialStyle ?? false;

            maniaSpecialStyle.Alpha = Beatmap.Value?.BeatmapInfo.RulesetID == 3 ? 1 : 0;
        }
    }
}
