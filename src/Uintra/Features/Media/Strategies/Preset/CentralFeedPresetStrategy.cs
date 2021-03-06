﻿using System.Web;
using Uintra.Infrastructure.Extensions;

namespace Uintra.Features.Media.Strategies.Preset
{
    public class CentralFeedPresetStrategy : IPresetStrategy
    {
        public string ThumbnailPreset { get; } = "preset=preview";
        public string PreviewPreset { get; } = "preset=centralFeedPreview";
        public string PreviewTwoPreset { get; } = "preset=previewTwo";
        public int MediaFilesToDisplay { get; } = HttpContext.Current.Request.IsMobileBrowser() ? 2 : 3;
    }
}