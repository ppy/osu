// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Utils;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class ScoreAttributeText : FontAdjustableSkinComponent, ISerialisableDrawable
    {
        public const float DEFAULT_TEXT_SIZE = 40;

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute))]
        public Bindable<ScoreAttribute> Attribute { get; } = new Bindable<ScoreAttribute>();

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();
        private readonly OsuSpriteText text;

        public ScoreAttributeText()
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

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the legacy skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateText());
            Template.BindValueChanged(_ => updateText());

            score.BindValueChanged(_ => updateText());
            scoringMode.BindValueChanged(_ => updateText());
            updateText();
        }

        private void updateText()
        {
            var ruleset = score.Value.Ruleset.CreateInstance();

            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{1}");

            List<object?> values = new List<object?>
            {
                getLabelString(Attribute.Value),
                getValueString(Attribute.Value, ruleset)
            };

            foreach (var type in Enum.GetValues<ScoreAttribute>())
            {
                string replaced = numberedTemplate.Replace($@"{{{{{type}}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(getValueString(type, ruleset));
                }
            }

            foreach (var type in Enum.GetValues<HitResult>())
            {
                string replaced = numberedTemplate.Replace($@"{{{{{type}Label}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(ruleset.GetDisplayNameForHitResult(type));
                }

                replaced = numberedTemplate.Replace($@"{{{{{type}}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(score.Value.Statistics.GetValueOrDefault(type).ToLocalisableString(@"N0"));
                }

                replaced = numberedTemplate.Replace($@"{{{{{type}Max}}}}", $@"{{{values.Count}}}");

                if (numberedTemplate != replaced)
                {
                    numberedTemplate = replaced;
                    values.Add(score.Value.MaximumStatistics.GetValueOrDefault(type).ToLocalisableString(@"N0"));
                }
            }

            text.Text = LocalisableString.Format(numberedTemplate, values.ToArray());
        }

        private LocalisableString getLabelString(ScoreAttribute attribute)
        {
            switch (attribute)
            {
                case ScoreAttribute.Username:
                    return ScoresStrings.ShowPlayerBy;

                case ScoreAttribute.TotalScore:
                    return BeatmapsetsStrings.ShowScoreboardHeadersScoreTotal;

                case ScoreAttribute.MaxCombo:
                    return BeatmapsetsStrings.ShowScoreboardHeadersCombo;

                case ScoreAttribute.Accuracy:
                    return BeatmapsetsStrings.ShowScoreboardHeadersAccuracy;

                case ScoreAttribute.Date:
                    return BeatmapLeaderboardWedgeStrings.Date;

                case ScoreAttribute.PP:
                    return ResultsScreenStrings.PerformancePointsHeader;

                case ScoreAttribute.Rank:
                    return BeatmapsetsStrings.ShowScoreboardHeadersRank;

                case ScoreAttribute.Mods:
                    return BeatmapsetsStrings.ShowScoreboardHeadersMods;

                default:
                    return string.Empty;
            }
        }

        private LocalisableString getValueString(ScoreAttribute attribute, Ruleset ruleset)
        {
            switch (attribute)
            {
                case ScoreAttribute.Username:
                    return score.Value.User.Username;

                case ScoreAttribute.TotalScore:
                    return score.Value.GetDisplayScore(scoringMode.Value).ToLocalisableString(@"N0");

                case ScoreAttribute.MaxCombo:
                    return $@"{score.Value.MaxCombo:N0}x";

                case ScoreAttribute.Accuracy:
                    return score.Value.Accuracy.FormatAccuracy();

                case ScoreAttribute.Date:
                    return score.Value.Date.ToLocalisableString(@"G");

                case ScoreAttribute.PP:
                    return score.Value.PP != null ? LocalisableString.Interpolate($@"{score.Value.PP}pp") : "-";

                case ScoreAttribute.Rank:
                    return score.Value.Rank.ToLocalisableString();

                case ScoreAttribute.Mods:
                    return formatMods(score.Value.Mods, ruleset);

                default:
                    return string.Empty;
            }
        }

        private static LocalisableString formatMods(IEnumerable<IConfiguredMod> mods, Ruleset ruleset)
        {
            var parts = new List<LocalisableString>();

            foreach (var mod in mods)
            {
                if (parts.Count > 0)
                    parts.Add(", ");

                parts.Add(mod.Acronym);

                var modInstance = mod.ToMod(ruleset);

                if (!modInstance.UsesDefaultConfiguration)
                {
                    parts.Add(" (");

                    foreach (var setting in modInstance.SettingDescription)
                    {
                        parts.Add(setting.setting);
                        parts.Add(": ");
                        parts.Add(setting.value);
                    }

                    parts.Add(")");
                }
            }

            return LocalisableString.Concat(parts.ToArray());
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: DEFAULT_TEXT_SIZE);

        protected override void SetTextColour(Colour4 textColour) => text.Colour = textColour;
    }

    // WARNING: DO NOT ADD ANY VALUES TO THIS ENUM ANYWHERE ELSE THAN AT THE END.
    // Doing so will break existing user skins.
    public enum ScoreAttribute
    {
        Username,
        TotalScore,
        MaxCombo,
        Accuracy,
        Date,
        PP,
        Rank,
        Mods,
    }
}
