// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.IO.FileAbstraction;
using osu.Game.Rulesets.Edit.Checks.Components;
using osu.Game.Storyboards;
using TagLib;
using File = TagLib.File;

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

            foreach (string filename in videoPaths)
            {
                string? storagePath = beatmapSet?.GetPathForFile(filename);

                if (storagePath == null)
                {
                    // There's an element in the storyboard that requires this resource, so it being missing is worth warning about.
                    yield return new IssueTemplateMissingFile(this).Create(filename);

                    continue;
                }

                Issue issue;

                try
                {
                    // We use TagLib here for platform invariance; BASS cannot detect audio presence on Linux.
                    using (Stream data = context.WorkingBeatmap.GetStream(storagePath))
                    using (File tagFile = File.Create(new StreamFileAbstraction(filename, data)))
                    {
                        if (tagFile.Properties.AudioChannels == 0)
                            continue;
                    }

                    issue = new IssueTemplateHasAudioTrack(this).Create(filename);
                }
                catch (CorruptFileException)
                {
                    issue = new IssueTemplateFileError(this).Create(filename, "Corrupt file");
                }
                catch (UnsupportedFormatException)
                {
                    issue = new IssueTemplateFileError(this).Create(filename, "Unsupported format");
                }
                catch (Exception ex)
                {
                    issue = new IssueTemplateFileError(this).Create(filename, "Internal failure - see logs for more info");
                    Logger.Log($"Failed when running {nameof(CheckAudioInVideo)}: {ex}");
                }

                yield return issue;
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
