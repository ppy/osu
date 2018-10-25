using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.States;
using osu.Framework.MathUtils;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;
using OpenTK.Graphics;
using Symcol.Base.Graphics.Containers;
using Symcol.Base.Graphics.Sprites;

namespace osu.Mods.Multi.Screens.Pieces
{
    public class MapDetails : SymcolClickableContainer
    {
        // ReSharper disable once InconsistentNaming
        private readonly SymcolSprite beatmapBG;
        private readonly SpriteText name;
        private readonly SpriteText artist;
        private readonly SpriteText difficulty;
        private readonly SpriteText time;

        private readonly Box dim;

        public MapDetails()
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
                beatmapBG = new SymcolSprite
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

        public void SetMap(WorkingBeatmap beatmap)
        {
            if (beatmap is DummyWorkingBeatmap) return;

            HitObject lastObject = beatmap.Beatmap.HitObjects.LastOrDefault();
            double endTime = (lastObject as IHasEndTime)?.EndTime ?? lastObject?.StartTime ?? 0;

            beatmapBG.Texture = beatmap.Background;
            name.Text = beatmap.BeatmapSetInfo.Metadata.Title;
            artist.Text = "By: " + beatmap.BeatmapSetInfo.Metadata.Artist;
            difficulty.Text = beatmap.BeatmapInfo.Version + " (" + Math.Round(beatmap.BeatmapInfo.StarDifficulty, 2) + " stars) mapped by " + beatmap.BeatmapInfo.Metadata.AuthorString;
            time.Text = getBPMRange(beatmap.Beatmap) + " bpm for " + TimeSpan.FromMilliseconds(endTime - beatmap.Beatmap.HitObjects.First().StartTime).ToString(@"m\:ss");

            BorderColour = getColour(beatmap.BeatmapInfo);
            EdgeEffect = new EdgeEffectParameters
            {
                Radius = 16,
                Type = EdgeEffectType.Shadow,
                Colour = getColour(beatmap.BeatmapInfo).Opacity(0.2f)
            };
            Action = () => openLink("https://osu.ppy.sh/beatmapsets/" + beatmap.BeatmapSetInfo.OnlineBeatmapSetID);
        }

        public void SetMap(int onlineBeatmapSetID)
        {
            name.Text = "Missing Map!";
            artist.Text = "Click to open in Browser";
            Action = () => openLink($"https://osu.ppy.sh/beatmapsets/{onlineBeatmapSetID}");
        }

        public void SetMap(bool selecting, int onlineBeatmapSetID = -1)
        {
            name.Text = selecting ? "Searching for Map!" : "Invalid / No Map Selected!";
            artist.Text = selecting ?  "osu! is looking for the selected map. . ." : "Don't hit start, weird things might happen";
            Action = () => openLink(selecting ? $"https://osu.ppy.sh/beatmapsets/{onlineBeatmapSetID}" : "https://osu.ppy.sh/home");
        }

        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;
        }

        private void openLink(string link) => host.OpenUrlExternally(link);

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
        private string getBPMRange(IBeatmap beatmap)
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
}
