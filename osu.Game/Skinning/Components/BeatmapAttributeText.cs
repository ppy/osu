// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class BeatmapAttributeText : FontAdjustableSkinComponent
    {
        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute), nameof(BeatmapAttributeTextStrings.AttributeDescription))]
        public Bindable<BeatmapAttribute> Attribute { get; } = new Bindable<BeatmapAttribute>(BeatmapAttribute.StarRating);

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly OsuSpriteText text;

        public BeatmapAttributeText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateText());
            Template.BindValueChanged(_ => updateText());
            beatmap.BindValueChanged(_ => updateText());

            updateText();
        }

        private void updateText()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{1}");

            List<object?> values = new List<object?>
            {
                getLabelString(Attribute.Value),
                getValueString(Attribute.Value)
            };

            foreach (var type in Enum.GetValues<BeatmapAttribute>())
            {
                string replaced = numberedTemplate.Replace($@"{{{{{type}}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(getValueString(type));
                }
            }

            text.Text = LocalisableString.Format(numberedTemplate, values.ToArray());
        }

        private LocalisableString getLabelString(BeatmapAttribute attribute)
        {
            switch (attribute)
            {
                case BeatmapAttribute.CircleSize:
                    return BeatmapsetsStrings.ShowStatsCs;

                case BeatmapAttribute.Accuracy:
                    return BeatmapsetsStrings.ShowStatsAccuracy;

                case BeatmapAttribute.HPDrain:
                    return BeatmapsetsStrings.ShowStatsDrain;

                case BeatmapAttribute.ApproachRate:
                    return BeatmapsetsStrings.ShowStatsAr;

                case BeatmapAttribute.StarRating:
                    return BeatmapsetsStrings.ShowStatsStars;

                case BeatmapAttribute.Title:
                    return EditorSetupStrings.Title;

                case BeatmapAttribute.Artist:
                    return EditorSetupStrings.Artist;

                case BeatmapAttribute.DifficultyName:
                    return EditorSetupStrings.DifficultyHeader;

                case BeatmapAttribute.Creator:
                    return EditorSetupStrings.Creator;

                case BeatmapAttribute.Source:
                    return EditorSetupStrings.Source;

                case BeatmapAttribute.Length:
                    return ArtistStrings.TracklistLength.ToTitle();

                case BeatmapAttribute.RankedStatus:
                    return BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault;

                case BeatmapAttribute.BPM:
                    return BeatmapsetsStrings.ShowStatsBpm;

                default:
                    return string.Empty;
            }
        }

        private LocalisableString getValueString(BeatmapAttribute attribute)
        {
            switch (attribute)
            {
                case BeatmapAttribute.Title:
                    return new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.TitleUnicode, beatmap.Value.BeatmapInfo.Metadata.Title);

                case BeatmapAttribute.Artist:
                    return new RomanisableString(beatmap.Value.BeatmapInfo.Metadata.ArtistUnicode, beatmap.Value.BeatmapInfo.Metadata.Artist);

                case BeatmapAttribute.DifficultyName:
                    return beatmap.Value.BeatmapInfo.DifficultyName;

                case BeatmapAttribute.Creator:
                    return beatmap.Value.BeatmapInfo.Metadata.Author.Username;

                case BeatmapAttribute.Source:
                    return beatmap.Value.BeatmapInfo.Metadata.Source;

                case BeatmapAttribute.Length:
                    return TimeSpan.FromMilliseconds(beatmap.Value.BeatmapInfo.Length).ToFormattedDuration();

                case BeatmapAttribute.RankedStatus:
                    return beatmap.Value.BeatmapInfo.Status.GetLocalisableDescription();

                case BeatmapAttribute.BPM:
                    return beatmap.Value.BeatmapInfo.BPM.ToLocalisableString(@"F2");

                case BeatmapAttribute.CircleSize:
                    return ((double)beatmap.Value.BeatmapInfo.Difficulty.CircleSize).ToLocalisableString(@"F2");

                case BeatmapAttribute.HPDrain:
                    return ((double)beatmap.Value.BeatmapInfo.Difficulty.DrainRate).ToLocalisableString(@"F2");

                case BeatmapAttribute.Accuracy:
                    return ((double)beatmap.Value.BeatmapInfo.Difficulty.OverallDifficulty).ToLocalisableString(@"F2");

                case BeatmapAttribute.ApproachRate:
                    return ((double)beatmap.Value.BeatmapInfo.Difficulty.ApproachRate).ToLocalisableString(@"F2");

                case BeatmapAttribute.StarRating:
                    return beatmap.Value.BeatmapInfo.StarRating.ToLocalisableString(@"F2");

                default:
                    return string.Empty;
            }
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);

        protected override void SetTextColour(Colour4 textColour) => text.Colour = textColour;
    }

    // WARNING: DO NOT ADD ANY VALUES TO THIS ENUM ANYWHERE ELSE THAN AT THE END.
    // Doing so will break existing user skins.
    public enum BeatmapAttribute
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate,
        StarRating,
        Title,
        Artist,
        DifficultyName,
        Creator,
        Length,
        RankedStatus,
        BPM,
        Source,
    }
}
