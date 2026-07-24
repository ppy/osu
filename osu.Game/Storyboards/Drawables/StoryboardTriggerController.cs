// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

                if (definition.Value.Matches(val.NewValue.OfType<HitSampleInfo>()))
                    playTrigger(drawable, triggerGroup);
            });
        }

        private readonly struct HitSampleTriggerDefinition
        {
            public const string PREFIX = @"HitSound";

            public string RawName { get; init; }
            public string? NormalBank { get; init; }
            public string? AdditionBank { get; init; }
            public string? AdditionName { get; init; }
            public string? Suffix { get; init; }

            public override string ToString() => RawName;

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

                // https://github.com/peppy/osu-stable-reference/blob/baa8705f782c0de2b10a7387d78014c61c8b17fb/osu!/GameplayElements/Events/Trigger/EventTriggerHitSound.cs#L70-L80
                bool bank1IsAddition = bank1 != null && bank2 == null && name != null;

                // remap names from the regex onto `HitSampleInfo` constants.
                // this simplifies logic later, and is faster than string checks at time of trigger.
                name = name?.ToLowerInvariant() switch
                {
                    @"whistle" => HitSampleInfo.HIT_WHISTLE,
                    @"clap" => HitSampleInfo.HIT_CLAP,
                    @"finish" => HitSampleInfo.HIT_FINISH,
                    _ => null,
                };
                bank1 = mapBankName(bank1);
                bank2 = mapBankName(bank2);

                var trigger = new HitSampleTriggerDefinition
                {
                    RawName = triggerName,
                    AdditionName = name,
                    NormalBank = bank1IsAddition ? bank2 : bank1,
                    AdditionBank = bank1IsAddition ? bank1 : bank2,
                    Suffix = suffix,
                };

                result = trigger;
                return true;

                string? mapBankName(string? triggerBank) =>
                    triggerBank?.ToLowerInvariant() switch
                    {
                        @"normal" => HitSampleInfo.BANK_NORMAL,
                        @"soft" => HitSampleInfo.BANK_SOFT,
                        @"drum" => HitSampleInfo.BANK_DRUM,
                        _ => null, // "all" falls into this case which is intended as it means no filter anyway.
                    };
            }

            public bool Matches(IEnumerable<HitSampleInfo> hitSamples)
            {
                // lazer treats "addition bank" and "addition samples" slightly differently than stable does.
                // in stable, "normal bank" and "addition bank" were just properties of a hitsound.
                // a hitsound did not *need* to have an addition sound to have a defined bank for it.
                // contrary to this, lazer will just not emit an addition `HitSampleInfo` at all,
                // and because `HitSampleInfo` is what stores the bank, there is nowhere for the addition bank definition to go.
                // this is why the behaviour of the trigger as written below is not 100% accurate to stable.
                // the difference is perceivable the most on triggers like `HitSoundAllSoft` which will sometimes not trigger
                // on objects that do not have an addition sound, even if the "soft" bank should be inherited via a "green line" or baseline beatmap sample set.

                // if the addition name is specified, it must be found among the samples.
                bool foundAddition = AdditionName == null;
                // if the addition bank is specified, at least one addition sound must use this bank.
                bool additionBankCorrect = AdditionBank == null;

                foreach (var hitSampleInfo in hitSamples)
                {
                    if (hitSampleInfo.Name == HitSampleInfo.HIT_NORMAL)
                    {
                        if (NormalBank != null && hitSampleInfo.Bank != NormalBank)
                            return false;
                    }
                    else
                    {
                        if (AdditionName != null && hitSampleInfo.Name == AdditionName)
                            foundAddition = true;

                        if (AdditionBank != null && hitSampleInfo.Bank == AdditionBank)
                            additionBankCorrect = true;
                    }

                    if (Suffix != null && !string.Equals(Suffix, hitSampleInfo.Suffix, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return foundAddition && additionBankCorrect;
            }
        }

        #endregion

        private static void playTrigger<TDrawable>(TDrawable drawable, StoryboardTriggerGroup triggerGroup)
            where TDrawable : Drawable, IFlippable, IVectorScalable
        {
            if (drawable.Time.Current < triggerGroup.TriggerStartTime || drawable.Time.Current > triggerGroup.TriggerEndTime)
                return;

            foreach (var command in triggerGroup.AllCommands.OrderBy(c => c.StartTime))
            {
                using (drawable.BeginDelayedSequence(command.StartTime))
                    command.ApplyTransforms(drawable);
            }
        }
    }
}
