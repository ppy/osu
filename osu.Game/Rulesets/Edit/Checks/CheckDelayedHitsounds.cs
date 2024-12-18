// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Audio.Track;
using osu.Game.Audio;
using osu.Game.Extensions;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckDelayedHitsounds : ICheck
    {
        /// <summary>
        /// Threshold at which point the sample is considered silent.
        /// </summary>
        private const float silence_threshold = 0.001f;

        private const float falloff_factor = 0.95f;
        private const int delay_threshold = 5;
        private const int delay_threshold_negligible = 1;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Delayed hit sounds.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateConsequentDelay(this),
            new IssueTemplateDelay(this),
            new IssueTemplateDelayNoSilence(this),
            new IssueTemplateMinorDelay(this),
            new IssueTemplateMinorDelayNoSilence(this),
        };

        private float getAverageAmplitude(Waveform.Point point) => (point.AmplitudeLeft + point.AmplitudeRight) / 2;

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;

            if (beatmapSet == null)
                yield break;

            foreach (var file in beatmapSet.Files)
            {
                using (Stream? stream = context.WorkingBeatmap.GetStream(file.File.GetStoragePath()))
                {
                    if (stream == null)
                        continue;

                    if (!isHitSound(file.Filename))
                        continue;

                    using Waveform waveform = new Waveform(stream);

                    var points = waveform.GetPoints();

                    // Skip muted samples
                    if (points.Length == 0 || points.Sum(getAverageAmplitude) <= silence_threshold)
                        continue;

                    float maxAmplitude = points.Select(getAverageAmplitude).Max();

                    int consequentDelay = 0;
                    int delay = 0;
                    float amplitude = 0;

                    while (delay + consequentDelay < points.Length)
                    {
                        amplitude += getAverageAmplitude(points[delay]);

                        // Reached peak amplitude/transient
                        if (amplitude >= maxAmplitude)
                            break;

                        amplitude *= falloff_factor;

                        if (amplitude < silence_threshold)
                        {
                            amplitude = 0;
                            consequentDelay++;
                        }

                        delay++;
                    }

                    if (consequentDelay >= delay_threshold)
                        yield return new IssueTemplateConsequentDelay(this).Create(file.Filename, consequentDelay);
                    else if (consequentDelay + delay >= delay_threshold)
                    {
                        if (consequentDelay > 0)
                            yield return new IssueTemplateDelay(this).Create(file.Filename, consequentDelay, delay);
                        else
                            yield return new IssueTemplateDelayNoSilence(this).Create(file.Filename, delay);
                    }
                    else if (consequentDelay + delay >= delay_threshold_negligible)
                    {
                        if (consequentDelay > 0)
                            yield return new IssueTemplateMinorDelay(this).Create(file.Filename, consequentDelay, delay);
                        else
                            yield return new IssueTemplateMinorDelayNoSilence(this).Create(file.Filename, delay);
                    }
                }
            }
        }

        private bool isHitSound(string filename)
        {
            if (!AudioCheckUtils.HasAudioExtension(filename))
                return false;

            // <bank>-<sampleset>
            string[] parts = filename.ToLowerInvariant().Split('-');

            if (parts.Length != 2)
                return false;

            string bank = parts[0];
            string sampleSet = parts[1];

            return HitSampleInfo.AllBanks.Contains(bank)
                   && HitSampleInfo.AllAdditions.Append(HitSampleInfo.HIT_NORMAL).Any(sampleSet.StartsWith);
        }

        public class IssueTemplateConsequentDelay : IssueTemplate
        {
            public IssueTemplateConsequentDelay(ICheck check)
                : base(check, IssueType.Problem,
                    "\"{0}\" has a {1:0.##} ms period of complete silence at the start.")
            {
            }

            public Issue Create(string filename, int pureDelay) => new Issue(this, filename, pureDelay);
        }

        public class IssueTemplateDelay : IssueTemplate
        {
            public IssueTemplateDelay(ICheck check)
                : base(check, IssueType.Warning,
                    "\"{0}\" has a transient delay of ~{1:0.##} ms, of which {2:0.##} ms is complete silence.")
            {
            }

            public Issue Create(string filename, int consequentDelay, int delay) => new Issue(this, filename, delay, consequentDelay);
        }

        public class IssueTemplateDelayNoSilence : IssueTemplate
        {
            public IssueTemplateDelayNoSilence(ICheck check)
                : base(check, IssueType.Warning,
                    "\"{0}\" has a transient delay of ~{1:0.##} ms.")
            {
            }

            public Issue Create(string filename, int delay) => new Issue(this, filename, delay);
        }

        public class IssueTemplateMinorDelay : IssueTemplate
        {
            public IssueTemplateMinorDelay(ICheck check)
                : base(check, IssueType.Negligible,
                    "\"{0}\" has a transient delay of ~{1:0.##} ms, of which {2:0.##} ms is complete silence.")
            {
            }

            public Issue Create(string filename, int consequentDelay, int delay) => new Issue(this, filename, delay, consequentDelay);
        }

        public class IssueTemplateMinorDelayNoSilence : IssueTemplate
        {
            public IssueTemplateMinorDelayNoSilence(ICheck check)
                : base(check, IssueType.Negligible,
                    "\"{0}\" has a transient delay of ~{1:0.##} ms.")
            {
            }

            public Issue Create(string filename, int delay) => new Issue(this, filename, delay);
        }
    }
}
