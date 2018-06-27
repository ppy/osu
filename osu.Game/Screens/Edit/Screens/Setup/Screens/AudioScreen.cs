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
    public class AudioScreen : EditorScreen
    {
        private readonly Container content;

        private readonly LabelledRadioButton samples;

        public string Title => "Audio";

        public AudioScreen()
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
                                    Text = "Default Sample Settings",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                new OsuSpriteText
                                {
                                    Padding = new MarginPadding { Top = 10 },
                                    Colour = Color4.White,
                                    Text = "Misc. Toggles",
                                    TextSize = 20,
                                    Font = @"Exo2.0-Bold",
                                },
                                samples = new LabelledRadioButton
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    LabelText = "Samples match playback rate",
                                    BottomLabelText = "The constant rate of health-bar drain throughout the song",
                                },
                            }
                        },
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            // Could not find a property for this setting, so the radio button is completely useless
            //samples.RadioButtonValueChanged += a => Beatmap.Value = a;
        }

        private void updateInfo()
        {
            // Update info about the beatmap once beatmap being used is updated
        }
    }
}
