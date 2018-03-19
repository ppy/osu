using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Symcol.Pieces;
using Symcol.Rulesets.Core.Multiplayer.Options;
using System;
using System.Diagnostics;
using System.Linq;

namespace Symcol.Rulesets.Core.Multiplayer.Pieces
{
    public class MatchTools : Container
    {
        public readonly Bindable<MatchScreenMode> Mode = new Bindable<MatchScreenMode>() { Default = MatchScreenMode.MapDetails };

        public readonly Bindable<MatchGamemode> GameMode = new Bindable<MatchGamemode>() { Default = MatchGamemode.HeadToHead };

        public readonly OsuTabControl<MatchScreenMode> TabControl;

        public readonly Container SelectedContent;

        public readonly Container MapDetails;

        public Container RulesetSettings;

        public readonly Container SoundBoard;

        private WorkingBeatmap selectedBeatmap;

        private int selectedBeatmapSetID;

        public MatchTools()
        {
            Masking = true;
            CornerRadius = 16;
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.Both;
            Width = 0.49f;
            Height = 0.45f;
            Position = new Vector2(-10, 10);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.8f)
                },
                TabControl = new OsuTabControl<MatchScreenMode>
                {
                    Position = new Vector2(72, 0),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.08f,
                    Width = 0.8f
                },
                SelectedContent = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.92f
                }
            };
            TabControl.Current.Value = MatchScreenMode.MapDetails;

            Mode.ValueChanged += (value) =>
            {
                switch (value)
                {
                    case MatchScreenMode.MapDetails:
                        if (selectedBeatmap != null)
                            SelectedContent.Child = new MapDetailsSection(selectedBeatmap);
                        else if (selectedBeatmapSetID != 0)
                            SelectedContent.Child = new MapDetailsSection(selectedBeatmapSetID);
                        else
                            SelectedContent.Child = new MapDetailsSection(true);
                        break;
                    case MatchScreenMode.MatchSettings:
                        SelectedContent.Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,

                            Children = new Drawable[]
                            {
                                new MultiplayerDropdownEnumOption<MatchGamemode>(GameMode, "Match Gamemode", 1)
                            }
                        };
                        break;
                    case MatchScreenMode.SoundBoard:
                        SelectedContent.Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new HitSoundBoard
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                ButtonSize = 80
                            }
                        };
                        break;
                }
            };
            Mode.BindTo(TabControl.Current);
        }

        public void MapChange(WorkingBeatmap workingBeatmap)
        {
            if (workingBeatmap == null)
            {
                MapChange(-1);
                return;
            }

            selectedBeatmap = workingBeatmap;
            selectedBeatmapSetID = (int)workingBeatmap.BeatmapSetInfo.OnlineBeatmapSetID;

            if (Mode.Value == MatchScreenMode.MapDetails)
                SelectedContent.Child = new MapDetailsSection(selectedBeatmap);
        }

        public void MapChange(int onlineBeatmapSetID)
        {
            selectedBeatmap = null;
            selectedBeatmapSetID = onlineBeatmapSetID;

            if (Mode.Value == MatchScreenMode.MapDetails)
            {
                if (selectedBeatmapSetID != 0 && selectedBeatmapSetID != -1)
                    SelectedContent.Child = new MapDetailsSection(selectedBeatmapSetID);
                else
                    SelectedContent.Child = new MapDetailsSection(true);
            }
        }
    }

    public class MapDetailsSection : ClickableContainer
    {
        private Sprite beatmapBG;
        private SpriteText name;
        private SpriteText artist;
        private SpriteText difficulty;
        private SpriteText time;

        private Box dim;

        public MapDetailsSection(WorkingBeatmap workingBeatmap)
        {
            draw();

            HitObject lastObject = workingBeatmap.Beatmap.HitObjects.LastOrDefault();
            double endTime = (lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0;

            beatmapBG.Texture = workingBeatmap.Background;
            name.Text = workingBeatmap.BeatmapSetInfo.Metadata.Title;
            artist.Text = "By: " + workingBeatmap.BeatmapSetInfo.Metadata.Artist;
            difficulty.Text = workingBeatmap.BeatmapInfo.Version + " (" + Math.Round(workingBeatmap.BeatmapInfo.StarDifficulty, 2) + " stars) mapped by " + workingBeatmap.BeatmapInfo.Metadata.AuthorString;
            time.Text = getBPMRange(workingBeatmap.Beatmap) + " bpm for " + TimeSpan.FromMilliseconds(endTime - workingBeatmap.Beatmap.HitObjects.First().StartTime).ToString(@"m\:ss");

            BorderColour = getColour(workingBeatmap.BeatmapInfo);
            EdgeEffect = new EdgeEffectParameters
            {
                Radius = 16,
                Type = EdgeEffectType.Shadow,
                Colour = getColour(workingBeatmap.BeatmapInfo).Opacity(0.2f)
            };
            Action = () => Process.Start("https://osu.ppy.sh/beatmapsets/" + workingBeatmap.BeatmapSetInfo.OnlineBeatmapSetID);
        }

        public MapDetailsSection(int onlineBeatmapSetID)
        {
            draw();
            name.Text = "Missing Map!";
            artist.Text = "Click to open in Browser";
            Action = () => Process.Start("https://osu.ppy.sh/beatmapsets/" + onlineBeatmapSetID);
        }

        public MapDetailsSection(bool invalid)
        {
            draw();
            name.Text = "Invalid / No Map Selected!";
            artist.Text = "Don't hit start, weird things might happen";
            Action = () => Process.Start("https://osu.ppy.sh/home");
        }

        private void draw()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;

            Width = 0.95f;
            Height = 0.9f;

            Masking = true;
            BorderColour = Color4.LightBlue;
            BorderThickness = 4;
            CornerRadius = 10;

            EdgeEffect = new EdgeEffectParameters
            {
                Radius = 16,
                Type = EdgeEffectType.Shadow,
                Colour = Color4.LightBlue.Opacity(0.2f)
            };

            Children = new Drawable[]
            {
                beatmapBG = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                },
                dim = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f
                },
                name = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, 0),
                    Font = @"Exo2.0-SemiBoldItalic",
                    TextSize = 40
                },
                artist = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, 38),
                    Font = @"Exo2.0-MediumItalic",
                    TextSize = 24
                },
                difficulty = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, 64),
                    Font = "Exo2.0-Bold",
                    TextSize = 16
                },
                time = new SpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Position = new Vector2(10, 84),
                    TextSize = 16
                }
            };
        }

        protected override bool OnHover(InputState state)
        {
            dim.FadeTo(0.4f, 200);

            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            base.OnHoverLost(state);

            dim.FadeTo(0.6f, 200);
        }

        //"Borrowed" stuff
        private string getBPMRange(Beatmap beatmap)
        {
            double bpmMax = beatmap.ControlPointInfo.BPMMaximum;
            double bpmMin = beatmap.ControlPointInfo.BPMMinimum;

            if (Precision.AlmostEquals(bpmMin, bpmMax))
                return $"{bpmMin:0}";

            return $"{bpmMin:0}-{bpmMax:0} (mostly {beatmap.ControlPointInfo.BPMMode:0})";
        }

        private enum DifficultyRating
        {
            Easy,
            Normal,
            Hard,
            Insane,
            Expert,
            ExpertPlus
        }

        private DifficultyRating getDifficultyRating(BeatmapInfo beatmap)
        {
            if (beatmap == null)
                throw new ArgumentNullException(nameof(beatmap));

            var rating = beatmap.StarDifficulty;

            if (rating < 1.5) return DifficultyRating.Easy;
            if (rating < 2.25) return DifficultyRating.Normal;
            if (rating < 3.75) return DifficultyRating.Hard;
            if (rating < 5.25) return DifficultyRating.Insane;
            if (rating < 6.75) return DifficultyRating.Expert;
            return DifficultyRating.ExpertPlus;
        }

        private Color4 getColour(BeatmapInfo beatmap)
        {
            OsuColour palette = new OsuColour();
            switch (getDifficultyRating(beatmap))
            {
                case DifficultyRating.Easy:
                    return palette.Green;
                default:
                case DifficultyRating.Normal:
                    return palette.Blue;
                case DifficultyRating.Hard:
                    return palette.Yellow;
                case DifficultyRating.Insane:
                    return palette.Pink;
                case DifficultyRating.Expert:
                    return palette.Purple;
                case DifficultyRating.ExpertPlus:
                    return palette.Gray0;
            }
        }
    }

    public enum MatchGamemode
    {
        [System.ComponentModel.Description("Head to Head")]
        HeadToHead,
        [System.ComponentModel.Description("Head to Head with Live Spectator")]
        HeadToHeadSpectator,
        [System.ComponentModel.Description("Team Versus")]
        TeamVS,
        [System.ComponentModel.Description("TAG4")]
        TAG4,
        [System.ComponentModel.Description("Team TAG4")]
        TeamTAG4,
        [System.ComponentModel.Description("Tourny Mode")]
        Tournement,
    }

    public enum MatchScreenMode
    {
        [System.ComponentModel.Description("Map Details")]
        MapDetails,
        [System.ComponentModel.Description("Match Settings")]
        MatchSettings,
        [System.ComponentModel.Description("Ruleset Settings")]
        RulesetSettings,
        [System.ComponentModel.Description("Sound Board")]
        SoundBoard
    }
}
