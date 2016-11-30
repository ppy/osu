//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;

namespace osu.Game.Overlays.PopUpDialogs
{
    public class DeleteBeatmapDialog : PopUpDialog
    {
        protected override FontAwesome icon => FontAwesome.fa_trash_o;
        protected override string title => "DELETE BEATMAP";

        private WorkingBeatmap beatmap;
        private SpriteText trackMetadata;

        protected override Container<Drawable> CreateHeader()
        {
            FlowContainer headCont = new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                AutoSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Bottom = -40,
                },
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Text = "Confirm deletion of",
                        Font = @"Exo2.0-Bold",
                        TextSize = 19,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Colour = new Color4(153, 238, 255, 255),
                        Padding = new MarginPadding
                        {
                            Bottom = 5,
                        },
                    },
                    trackMetadata = new SpriteText
                    {
                        Text = string.Empty,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = @"Exo2.0-BoldItalic",
                    },

                }
            };
            return headCont;
        }

        public void UpdateSelectedBeatmap(WorkingBeatmap b)
        {
            beatmap = b;
            trackMetadata.Text = $"{beatmap.BeatmapInfo.Metadata.Artist}" + " - " + $"{beatmap.BeatmapInfo.Metadata.Title}";
        }

        protected override Container<Drawable> CreateBody()
        {
            FlowContainer bodyCont = new FlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FlowDirection.VerticalOnly,
                Children = new Drawable[]
                {
                    new PopUpDialogButton
                    {
                        Text = "Yes. Totally. Delete it.",
                        Colour = new Color4(238, 51, 153, 255),
                        BackgroundColour = new Color4(159, 14, 102, 255),
                        Width = button_width,
                        Height = button_height,
                        BackgroundWidth = button_background_width,
                        BackgroundHeight = button_height,
                        Action = Hide, //TODO: Hide, then delete beatmap
                    },
                    new PopUpDialogButton
                    {
                        Text = "Firetruck, I didn't mean to!",
                        Colour = new Color4(68, 170, 221, 225),
                        BackgroundColour = new Color4(14, 116, 145, 255),
                        Width = button_width,
                        Height = button_height,
                        BackgroundWidth = button_background_width,
                        BackgroundHeight = button_height,
                        Action = Hide,
                    }
                }
            };
            return bodyCont;
        }
    }
}
