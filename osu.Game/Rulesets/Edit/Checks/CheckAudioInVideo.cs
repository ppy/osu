// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using ManagedBass;
using osu.Framework.Audio.Callbacks;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Storyboards;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckAudioInVideo : ICheck
    {
        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Audio, "Audio track in video files");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateHasAudioTrack(this),
            new IssueTemplateMissingFile(this),
            new IssueTemplateFileError(this)
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;

            foreach (var layer in context.WorkingBeatmap.Storyboard.Layers)
            {
                foreach (var element in layer.Elements)
                {
                    if (!(element is StoryboardVideo video))
                        continue;

                    string filename = video.Path;
                    string storagePath = beatmapSet.GetPathForFile(filename);

                    if (storagePath == null)
                    {
                        // There's an element in the storyboard that requires this resource, so it being missing is worth warning about.
                        yield return new IssueTemplateMissingFile(this).Create(filename);

                        continue;
                    }

                    Stream data = context.WorkingBeatmap.GetStream(storagePath);
                    var fileCallbacks = new FileCallbacks(new DataStreamFileProcedures(data));
                    int decodeStream = Bass.CreateStream(StreamSystem.NoBuffer, BassFlags.Decode, fileCallbacks.Callbacks, fileCallbacks.Handle);
                    if (decodeStream == 0)
                        continue;

                    yield return new IssueTemplateHasAudioTrack(this).Create(filename);
                }
            }
        }

        public class IssueTemplateHasAudioTrack : IssueTemplate
        {
            public IssueTemplateHasAudioTrack(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" has an audio track.")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }

        public class IssueTemplateFileError : IssueTemplate
        {
            public IssueTemplateFileError(ICheck check)
                : base(check, IssueType.Error, "Could not check whether \"{0}\" has an audio track ({1}).")
            {
            }

            public Issue Create(string filename, string errorReason) => new Issue(this, filename, errorReason);
        }

        public class IssueTemplateMissingFile : IssueTemplate
        {
            public IssueTemplateMissingFile(ICheck check)
                : base(check, IssueType.Warning, "Could not check whether \"{0}\" has an audio track, because it is missing.")
            {
            }

            public Issue Create(string filename) => new Issue(this, filename);
        }
    }
}
