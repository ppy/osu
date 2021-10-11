// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using osu.Game.IO.FileAbstraction;
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
            var videoPaths = new List<string>();

            foreach (var layer in context.WorkingBeatmap.Storyboard.Layers)
            {
                foreach (var element in layer.Elements)
                {
                    if (!(element is StoryboardVideo video))
                        continue;

                    // Ensures we don't check the same video file multiple times in case of multiple elements using it.
                    if (!videoPaths.Contains(video.Path))
                        videoPaths.Add(video.Path);
                }
            }

            foreach (var filename in videoPaths)
            {
                string storagePath = beatmapSet.GetPathForFile(filename);

                if (storagePath == null)
                {
                    // There's an element in the storyboard that requires this resource, so it being missing is worth warning about.
                    yield return new IssueTemplateMissingFile(this).Create(filename);

                    continue;
                }

                Stream data = context.WorkingBeatmap.GetStream(storagePath);
                StreamFileAbstraction fileAbstraction = new StreamFileAbstraction(filename, data);

                // We use TagLib here for platform invariance; BASS cannot detect audio presence on Linux.
                TagLib.File tagFile = null;
                string errorReason = null;

                try
                {
                    tagFile = TagLib.File.Create(fileAbstraction);
                }
                catch (TagLib.CorruptFileException) { errorReason = "Corrupt file"; }
                catch (TagLib.UnsupportedFormatException) { errorReason = "Unsupported format"; }

                if (errorReason != null)
                {
                    yield return new IssueTemplateFileError(this).Create(filename, errorReason);

                    continue;
                }

                if (tagFile.Properties.AudioChannels == 0)
                    continue;

                yield return new IssueTemplateHasAudioTrack(this).Create(filename);
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
