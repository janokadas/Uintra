﻿using System;
using System.Collections.Generic;
using System.Linq;
using Uintra.Core.Caching;
using Uintra.Core.Extensions;
using Uintra.Core.Persistence;
using Uintra.Core.User;
using Uintra.Groups.Sql;
using static LanguageExt.Prelude;

namespace Uintra.Groups
{
    public class GroupService : IGroupService
    {
        private readonly ISqlRepository<Group> _groupRepository;
        private readonly ICacheService _memoryCacheService;
        private const string GroupCacheKey = "Groups";

        public GroupService(
            ISqlRepository<Group> groupRepository,
            ICacheService memoryCacheService)
        {
            _groupRepository = groupRepository;
            _memoryCacheService = memoryCacheService;
        }

        public Guid Create(GroupModel model)
        {
            var date = DateTime.Now;
            var group = model.Map<Group>();
            group.CreatedDate = date;
            group.UpdatedDate = date;
            group.Id = Guid.NewGuid();

            _groupRepository.Add(group);
            UpdateCache();

            return group.Id;
        }

        public void Edit(GroupModel model)
        {
            var date = DateTime.Now;
            var group = model.Map<Group>();
            group.UpdatedDate = date;
            _groupRepository.Update(group);
            UpdateCache();
        }

        public GroupModel Get(Guid id)
        {
            return GetAll().SingleOrDefault(g => g.Id == id);
        }

        public IEnumerable<GroupModel> GetAll()
        {
            var groups = _memoryCacheService.GetOrSet(GroupCacheKey, () => _groupRepository.GetAll().ToList(), GetCacheExpiration());
            return groups.Map<IEnumerable<GroupModel>>();
        }

        public IEnumerable<GroupModel> GetAllNotHidden()
        {
            return GetAll().Where(g => !g.IsHidden);
        }

        public IEnumerable<GroupModel> GetMany(IEnumerable<Guid> groupIds)
        {
            return GetAllNotHidden().Join(groupIds, g => g.Id, identity, (g, _) => g);
        }

        public bool CanEdit(Guid groupId, IIntranetMember member)
        {
            var group = Get(groupId);
            return CanEdit(group, member);
        }

        public bool CanEdit(GroupModel groupModel, IIntranetMember member)
        {
            if (member.Group.Id == IntranetRolesEnum.WebMaster.ToInt())
            {
                return true;
            }

            return groupModel.CreatorId == member.Id;
        }

        public void Hide(Guid id)
        {
            var group = Get(id);
            group.IsHidden = true;
            Edit(group);
        }

        public void Unhide(Guid id)
        {
            var group = Get(id);
            group.IsHidden = false;
            Edit(group);
        }

        public bool IsActivityFromActiveGroup(IGroupActivity groupActivity) =>
            groupActivity.GroupId.HasValue && !Get(groupActivity.GroupId.Value).IsHidden;

        private static DateTimeOffset GetCacheExpiration()
        {
            return DateTimeOffset.Now.AddDays(1);
        }

        private void UpdateCache()
        {
            var groups = _groupRepository.GetAll().ToList();
            _memoryCacheService.Set(GroupCacheKey, groups, GetCacheExpiration());
        }
    }
}