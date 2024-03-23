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
    public class CheckVideoResolution : ICheck
    {
        private const int max_video_width = 1280;

        private const int max_video_height = 720;

        public CheckMetadata Metadata => new CheckMetadata(CheckCategory.Resources, "Too high video resolution.");

        public IEnumerable<IssueTemplate> PossibleTemplates => new IssueTemplate[]
        {
            new IssueTemplateHighResolution(this),
            new IssueTemplateFileError(this),
        };

        public IEnumerable<Issue> Run(BeatmapVerifierContext context)
        {
            var beatmapSet = context.Beatmap.BeatmapInfo.BeatmapSet;
            var videoPaths = getVideoPaths(context.WorkingBeatmap.Storyboard);

            foreach (string filename in videoPaths)
            {
                string? storagePath = beatmapSet?.GetPathForFile(filename);

                // Don't report any issues for missing video here since another check is already doing that (CheckAudioInVideo)
                if (storagePath == null) continue;

                Issue issue;

                try
                {
                    using (Stream data = context.WorkingBeatmap.GetStream(storagePath))
                    using (File tagFile = File.Create(new StreamFileAbstraction(filename, data)))
                    {
                        int height = tagFile.Properties.VideoHeight;
                        int width = tagFile.Properties.VideoWidth;

                        if (height <= max_video_height || width <= max_video_width)
                            continue;

                        issue = new IssueTemplateHighResolution(this).Create(filename, width, height);
                    }
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
                    Logger.Log($"Failed when running {nameof(CheckVideoResolution)}: {ex}");
                }

                yield return issue;
            }
        }

        private List<string> getVideoPaths(Storyboard storyboard)
        {
            var videoPaths = new List<string>();

            foreach (var layer in storyboard.Layers)
            {
                foreach (var element in layer.Elements)
                {
                    if (element is not StoryboardVideo video)
                        continue;

                    if (!videoPaths.Contains(video.Path))
                        videoPaths.Add(video.Path);
                }
            }

            return videoPaths;
        }

        public class IssueTemplateHighResolution : IssueTemplate
        {
            public IssueTemplateHighResolution(ICheck check)
                : base(check, IssueType.Problem, "\"{0}\" resolution exceeds 1280x720 ({1}x{2})")
            {
            }

            public Issue Create(string filename, int width, int height) => new Issue(this, filename, width, height);
        }

        public class IssueTemplateFileError : IssueTemplate
        {
            public IssueTemplateFileError(ICheck check)
                : base(check, IssueType.Error, "Could not check resolution for \"{0}\" ({1}).")
            {
            }

            public Issue Create(string filename, string errorReason) => new Issue(this, filename, errorReason);
        }

    }
}
