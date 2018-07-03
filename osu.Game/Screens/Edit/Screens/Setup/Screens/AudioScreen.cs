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

        private readonly LabelledCheckBox samples;

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
                                samples = new LabelledCheckBox
                                {
                                    Padding = new MarginPadding { Top = 10, Right = 150 },
                                    LabelText = "Samples Match Playback Rate",
                                    BottomLabelText = "This option is suitable for fully-hitsounded maps.",
                                },
                            }
                        },
                    },
                },
            };

            updateInfo();
            Beatmap.ValueChanged += a => updateInfo();

            samples.RadioButtonValueChanged += a => Beatmap.Value.BeatmapInfo.SamplesMatchPlaybackRate = a;
        }

        public void ChangeSamplesMatchPlaybackRate(bool newValue) => samples.CurrentValue = newValue;

        private void updateInfo()
        {
            samples.CurrentValue = Beatmap.Value?.BeatmapInfo.SamplesMatchPlaybackRate ?? false;
        }
    }
}
