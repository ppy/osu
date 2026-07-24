// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Storyboards.Commands;

namespace osu.Game.Storyboards.Drawables
{
    public partial class StoryboardTriggerController : Component
    {
        public Bindable<bool> Passing
        {
            get => passing.Current;
            set => passing.Current = value;
        }

        private readonly BindableWithCurrent<bool> passing = new BindableWithCurrent<bool>();
        private readonly IBindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();
        private readonly IBindable<ISampleInfo[]?> lastPlayedSamples = new Bindable<ISampleInfo[]?>();

        [BackgroundDependencyLoader]
        private void load(GameplayState? gameplayState)
        {
            if (gameplayState != null)
            {
                lastJudgementResult.BindTo(gameplayState.LastJudgementResult);
                lastPlayedSamples.BindTo(gameplayState.LastPlayedSamples);
            }
        }

        public void Bind<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            switch (triggerGroup.TriggerName)
            {
                case @"Passing":
                    bindPassing(drawable, triggerGroup, true);
                    break;

                case @"Failing":
                    bindPassing(drawable, triggerGroup, false);
                    break;

                case @"HitObjectHit":
                    bindHitObjectHit(drawable, triggerGroup);
                    break;

                case string s when s.StartsWith(HitSampleTriggerDefinition.PREFIX, StringComparison.OrdinalIgnoreCase):
                    bindHitSample(drawable, triggerGroup);
                    break;
            }
        }

        private void bindPassing<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup, bool passing)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            this.passing.BindValueChanged(val =>
            {
                if (val.NewValue != passing)
                    return;

                playTrigger(drawable, triggerGroup);
            });
        }

        private void bindHitObjectHit<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup) where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            lastJudgementResult.BindValueChanged(val =>
            {
                if (val.NewValue.IsNotNull() && val.NewValue.IsHit && val.NewValue.Type.IsScorable())
                    playTrigger(drawable, triggerGroup);
            });
        }

        #region Hit sample triggers

        private void bindHitSample<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup) where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            if (!HitSampleTriggerDefinition.TryParse(triggerGroup.TriggerName, out var definition))
                return;

            // TODO: consider optimising. this likely does a lot of redundant work
            lastPlayedSamples.BindValueChanged(val =>
            {
                if (val.NewValue == null)
                    return;

                foreach (var sample in val.NewValue.OfType<HitSampleInfo>())
                {
                    if (definition.Value.Matches(sample))
                        playTrigger(drawable, triggerGroup);
                }
            });
        }

        private struct HitSampleTriggerDefinition
        {
            public const string PREFIX = @"HitSound";

            public string? NormalBank { get; set; }
            public string? AdditionBank { get; set; }
            public string? Name { get; set; }
            public string? Suffix { get; set; }

            public string CanonicalName => $@"{PREFIX}{NormalBank}{AdditionBank}{Name}{Suffix}";

            private static readonly Regex parse_regex = new Regex(
                @$"(?i)^{PREFIX}(?<bank1>(All|Normal|Soft|Drum))?(?<bank2>(All|Normal|Soft|Drum))?(?<name>(Whistle|Clap|Finish))?(?<suffix>\d+)?$",
                RegexOptions.Compiled);

            public static bool TryParse(string triggerName, [NotNullWhen(true)] out HitSampleTriggerDefinition? result)
            {
                var match = parse_regex.Match(triggerName);

                if (!match.Success)
                {
                    result = null;
                    return false;
                }

                string? name = match.Groups[@"name"].Success ? match.Groups[@"name"].Value : null;
                string? bank1 = match.Groups[@"bank1"].Success ? match.Groups[@"bank1"].Value : null;
                string? bank2 = match.Groups[@"bank2"].Success ? match.Groups[@"bank2"].Value : null;
                string? suffix = match.Groups[@"suffix"].Success ? match.Groups[@"suffix"].Value : null;

                bool bank1IsAddition = bank1 != null && bank2 == null && name != null;

                var trigger = new HitSampleTriggerDefinition
                {
                    Name = name,
                    NormalBank = bank1IsAddition ? bank2 : bank1,
                    AdditionBank = bank1IsAddition ? bank1 : bank2,
                    Suffix = suffix,
                };

                if (!string.Equals(triggerName, trigger.CanonicalName, StringComparison.OrdinalIgnoreCase))
                {
                    result = null;
                    return false;
                }

                result = trigger;
                return true;
            }

            public bool Matches(HitSampleInfo hitSampleInfo)
            {
                if (Name != null && !string.Equals(Name, hitSampleInfo.Name, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (NormalBank != null && (hitSampleInfo.Name != HitSampleInfo.HIT_NORMAL || !string.Equals(hitSampleInfo.Bank, NormalBank, StringComparison.OrdinalIgnoreCase)))
                    return false;

                if (AdditionBank != null && (hitSampleInfo.Name == HitSampleInfo.HIT_NORMAL || !string.Equals(hitSampleInfo.Bank, AdditionBank, StringComparison.OrdinalIgnoreCase)))
                    return false;

                if (Suffix != null && !string.Equals(Name, hitSampleInfo.Name, StringComparison.OrdinalIgnoreCase))
                    return false;

                return true;
            }
        }

        #endregion

        private static void playTrigger<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            using (drawable.BeginDelayedSequence(0))
            {
                foreach (var command in triggerGroup.AllCommands.OrderBy(c => c.StartTime))
                    command.ApplyTransforms(drawable);
            }
        }
    }
}
