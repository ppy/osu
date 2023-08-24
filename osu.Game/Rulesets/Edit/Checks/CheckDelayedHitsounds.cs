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

        private readonly string[] audioExtensions = { "mp3", "ogg", "wav" };
        private readonly string[] bankNames = { HitSampleInfo.BANK_NORMAL, HitSampleInfo.BANK_SOFT, HitSampleInfo.BANK_DRUM };
        private readonly string[] sampleSets = { HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE, HitSampleInfo.HIT_FINISH, HitSampleInfo.HIT_CLAP };

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Delayed hit sounds.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateConsequentDelay(this),
            new IssueTemplateDelay(this),
            new IssuTemplateMinorDelay(this)
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

                    if (!hasAudioExtension(file.Filename))
                        continue;

                    if (!isHitSound(file.Filename))
                        continue;

                    Waveform waveform = new Waveform(stream);

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
                        yield return new IssueTemplateDelay(this).Create(file.Filename, consequentDelay, delay);
                    else if (consequentDelay + delay >= delay_threshold_negligible)
                        yield return new IssuTemplateMinorDelay(this).Create(file.Filename, consequentDelay, delay);
                }
            }
        }

        private bool hasAudioExtension(string filename) => audioExtensions.Any(filename.ToLowerInvariant().EndsWith);

        private bool isHitSound(string filename)
        {
            // <bank>-<sampleset>
            string[] parts = filename.ToLowerInvariant().Split('-');

            if (parts.Length != 2)
                return false;

            string bank = parts[0];
            string sampleSet = parts[1];

            return bankNames.Contains(bank) && sampleSets.Any(sampleSet.StartsWith);
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

        public class IssuTemplateMinorDelay : IssueTemplate
        {
            public IssuTemplateMinorDelay(ICheck check)
                : base(check, IssueType.Negligible,
                    "\"{0}\" has a transient delay of ~{1:0.##} ms, of which {2:0.##} ms is complete silence.")
            {
            }

            public Issue Create(string filename, int consequentDelay, int delay) => new Issue(this, filename, delay, consequentDelay);
        }
    }
}
