﻿using System;
using System.Collections.Generic;
using Uintra20.Core.Controls.FileUpload;
using Uintra20.Features.Media.Contracts;
using Uintra20.Features.Media.Enums;
using Uintra20.Features.Media.Models;
using Umbraco.Core.Models;

namespace Uintra20.Features.Media.Helpers
{
    public interface IMediaHelper
    {
        IEnumerable<int> CreateMedia(IContentWithMediaCreateEditModel model, MediaFolderTypeEnum mediaFolderType, Guid? userId = null, int? mediaRootId = null);
        void DeleteMedia(int mediaId);
        void DeleteMedia(string mediaPath);
        void DeleteMedia(IEnumerable<int> mediaIds);
        void RestoreMedia(int mediaId);
        void RestoreMedia(IEnumerable<int> mediaIds);
        //IMediaModel CreateMedia(TempFile file, int rootMediaId, Guid? userId = null);
        IMedia CreateMedia(TempFile file, int rootMediaId, Guid? userId = null);
        //bool IsMediaDeleted(IPublishedContent media);
        MediaSettings GetMediaFolderSettings(Enum mediaFolderType, bool createFolderIfNotExists = false);
    }
}