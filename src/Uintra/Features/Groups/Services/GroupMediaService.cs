﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uintra.Core.Controls.FileUpload;
using Uintra.Features.Media;
using Uintra.Features.Media.Contracts;
using Uintra.Features.Media.Enums;
using Uintra.Features.Media.Helpers;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Uintra.Features.Groups.Services
{
    public class GroupMediaService : IGroupMediaService
    {
        private const string GroupIdPropertyTypeAlias = "GroupId";
        private readonly IMediaService _mediaService;
        private readonly IGroupService _groupService;
        private readonly IMediaHelper _mediaHelper;

        public GroupMediaService(IMediaService mediaService,
            IGroupService groupService,
            IMediaHelper mediaHelper)
        {
            _mediaService = mediaService;
            _groupService = groupService;
            _mediaHelper = mediaHelper;
        }

        public void GroupTitleChanged(Guid groupId, string newTitle)
        {
            var groupFolder = GetOrCreateGroupMediaFolder(groupId);
            groupFolder.Name = newTitle;
            _mediaService.Save(groupFolder);
        }

        public async Task GroupTitleChangedAsync(Guid groupId, string newTitle)
        {
            var groupFolder = await GetOrCreateGroupMediaFolderAsync(groupId);
            groupFolder.Name = newTitle;
            _mediaService.Save(groupFolder);
        }
        public IMedia CreateGroupMedia(string name, byte[] file, Guid groupId)
        {
            var groupFolder = GetOrCreateGroupMediaFolder(groupId);

            var fileModel = new TempFile
            {
                FileBytes = file,
                FileName = name
            };
            var media = _mediaHelper.CreateMedia(fileModel, groupFolder.Id);
            return media;
        }

        public async Task<IMedia> CreateGroupMediaAsync(string name, byte[] file, Guid groupId)
        {
            var groupFolder = await GetOrCreateGroupMediaFolderAsync(groupId);

            var fileModel = new TempFile
            {
                FileBytes = file,
                FileName = name
            };
            var media = _mediaHelper.CreateMedia(fileModel, groupFolder.Id);
            return media;
        }

        //public IMediaModel CreateGroupMedia(string name, byte[] file, Guid groupId)
        //{
        //    var groupFolder = GetOrCreateGroupMediaFolder(groupId);

        //    var fileModel = new TempFile
        //    {
        //        FileBytes = file,
        //        FileName = name
        //    };
        //    var media = _mediaHelper.CreateMedia(fileModel, groupFolder.Id);
        //    return media;
        //}

        //public async Task<IMediaModel> CreateGroupMediaAsync(string name, byte[] file, Guid groupId)
        //{
        //    var groupFolder = await GetOrCreateGroupMediaFolderAsync(groupId);

        //    var fileModel = new TempFile
        //    {
        //        FileBytes = file,
        //        FileName = name
        //    };
        //    var media = _mediaHelper.CreateMedia(fileModel, groupFolder.Id);
        //    return media;
        //}

        public IEnumerable<int> CreateGroupMedia(IContentWithMediaCreateEditModel model, Guid groupId, Guid creatorId)
        {
            var groupFolder = GetOrCreateGroupMediaFolder(groupId);

            var media = _mediaHelper.CreateMedia(model, MediaFolderTypeEnum.GroupsContent, creatorId, groupFolder.Id);
            return media;
        }

        public async Task<IEnumerable<int>> CreateGroupMediaAsync(IContentWithMediaCreateEditModel model, Guid groupId, Guid creatorId)
        {
            var groupFolder = await GetOrCreateGroupMediaFolderAsync(groupId);

            var media = _mediaHelper.CreateMedia(model, MediaFolderTypeEnum.GroupsContent, creatorId, groupFolder.Id);
            return media;
        }

        private IMedia GetOrCreateGroupMediaFolder(Guid groupId)
        {
            var groupFolderSettings = _mediaHelper.GetMediaFolderSettings(MediaFolderTypeEnum.GroupsContent, createFolderIfNotExists: true);

            var medias = _mediaService.GetPagedChildren(groupFolderSettings.MediaRootId ?? -1, 0, Int32.MaxValue, out _);
            var groupFolder = medias.FirstOrDefault(s =>
            {
                if (s.HasProperty(GroupIdPropertyTypeAlias))
                {
                    var id = s.GetValue<Guid?>(GroupIdPropertyTypeAlias);
                    return id.HasValue && id.Value == groupId;
                }
                return false;
            });

            if (groupFolder == null)
            {
                var group = _groupService.Get(groupId);
                groupFolder = _mediaService.CreateMedia(group.Title, groupFolderSettings.MediaRootId ?? -1, "Folder");
                groupFolder.SetValue(GroupIdPropertyTypeAlias, groupId.ToString());
                _mediaService.Save(groupFolder);
            }

            return groupFolder;
        }

        private async Task<IMedia> GetOrCreateGroupMediaFolderAsync(Guid groupId)
        {
            var groupFolderSettings = _mediaHelper.GetMediaFolderSettings(MediaFolderTypeEnum.GroupsContent, createFolderIfNotExists: true);

            var medias = _mediaService.GetPagedChildren(groupFolderSettings.MediaRootId ?? -1, 0, Int32.MaxValue, out _);
            var groupFolder = medias.FirstOrDefault(s =>
            {
                if (s.HasProperty(GroupIdPropertyTypeAlias))
                {
                    var id = s.GetValue<Guid?>(GroupIdPropertyTypeAlias);
                    return id.HasValue && id.Value == groupId;
                }
                return false;
            });

            if (groupFolder == null)
            {
                var group = await _groupService.GetAsync(groupId);
                groupFolder = _mediaService.CreateMedia(group.Title, groupFolderSettings.MediaRootId ?? -1, "Folder");
                groupFolder.SetValue(GroupIdPropertyTypeAlias, groupId.ToString());
                _mediaService.Save(groupFolder);
            }

            return groupFolder;
        }
    }
}