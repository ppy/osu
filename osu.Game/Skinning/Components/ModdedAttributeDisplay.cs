// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Skinning.Components
{
    public abstract partial class ModdedAttributeDisplay : FontAdjustableSkinComponent
    {
        public Bindable<LocalisableString> Current { get; protected set; } = new();

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; } = new Bindable<string>("{Label}: {Value}");

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        protected Bindable<RulesetInfo> Ruleset = null!;

        [Resolved]
        protected Bindable<WorkingBeatmap> Beatmap { get; private set; } = null!;

        [Resolved]
        protected Bindable<IReadOnlyList<Mod>> Mods { get; private set; } = null!;

        protected BeatmapInfo BeatmapInfo => Beatmap.Value.Beatmap.BeatmapInfo;

        private ModSettingChangeTracker? modSettingChangeTracker;

        private readonly OsuSpriteText text;

        public ModdedAttributeDisplay()
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

            Ruleset = game.Ruleset.GetBoundCopy();

            Current.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(_ => updateLabel());

            Ruleset.BindValueChanged(_ => UpdateValue());
            Beatmap.BindValueChanged(_ => UpdateValue());

            Mods.BindValueChanged(_ =>
            {
                modSettingChangeTracker?.Dispose();
                modSettingChangeTracker = new ModSettingChangeTracker(Mods.Value);
                modSettingChangeTracker.SettingChanged += _ => UpdateValue();
                UpdateValue();
            }, true);
        }

        /// <summary>
        /// Updates Current according to new beatmap info.
        /// Called whenever map changed, or ruleset changed, or mods changed.
        /// </summary>
        protected abstract void UpdateValue();

        /// <summary>
        /// Default label of the attribute.
        /// </summary>
        protected abstract LocalisableString AttributeLabel { get; }

        private void updateLabel()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", "{1}");

            text.Text = LocalisableString.Format(numberedTemplate, AttributeLabel, Current.Value);
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);
    }
}
