﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using LanguageExt;
using Uintra.Core.Extensions;
using Uintra.Core.Permissions.Interfaces;
using Uintra.Core.Permissions.Models;
using Uintra.Core.Permissions.TypeProviders;
using Uintra.Core.TypeProviders;
using Uintra.Core.User;
using Umbraco.Web.WebApi;
using static LanguageExt.Prelude;

namespace Uintra.Core.Permissions
{    
    public abstract class PermissionsControllerBase : UmbracoAuthorizedApiController
    {
        private readonly IPermissionsService _permissionsService;
        private readonly IIntranetMemberGroupProvider _intranetMemberGroupProvider;
        private readonly IPermissionResourceTypeProvider _resourceTypeProvider;
        private readonly IPermissionActionTypeProvider _actionTypeProvider;
        private readonly IIntranetMemberService<IIntranetMember> _intranetMemberService;

        protected PermissionsControllerBase(
            IIntranetMemberGroupProvider intranetMemberGroupProvider,
            IPermissionsService permissionsService,
            IPermissionResourceTypeProvider resourceTypeProvider,
            IPermissionActionTypeProvider actionTypeProvider,
            IIntranetMemberService<IIntranetMember> intranetMemberService)
        {
            _intranetMemberGroupProvider = intranetMemberGroupProvider;
            _permissionsService = permissionsService;
            _resourceTypeProvider = resourceTypeProvider;
            _actionTypeProvider = actionTypeProvider;
            _intranetMemberService = intranetMemberService;
        }

        [HttpGet]
        public GroupPermissionsViewModel Get(int memberGroupId)
        {
            var isSuperUser = _intranetMemberService.IsCurrentMemberSuperUser;
            var memberGroup = _intranetMemberGroupProvider[memberGroupId];

            var permissions = _permissionsService
                .GetForGroup(memberGroup)
                .Where(i => i.SettingValues.IsEnabled || isSuperUser)
                .Map<IEnumerable<PermissionViewModel>>()
                .OrderBy(i => i.ResourceTypeId);

            var model = new GroupPermissionsViewModel()
            {
                IsSuperUser = isSuperUser,
                Permissions = permissions,
                MemberGroup = memberGroup.Map<MemberGroupViewModel>()
            };

            return model;
        }

        [HttpPost]
        public Unit Save(PermissionUpdateViewModel update)
        {
            var settingIdentity = PermissionSettingIdentity.Of(
                _actionTypeProvider[update.ActionId],
                _resourceTypeProvider[update.ResourceTypeId]);
            var settingValue = PermissionSettingValues.Of(update.Allowed, update.Enabled);
            var targetGroup = _intranetMemberGroupProvider[update.IntranetMemberGroupId];

            var mappedUpdate = PermissionUpdateModel.Of(targetGroup, settingValue, settingIdentity);
            _permissionsService.Save(mappedUpdate);

            return unit;
        }
    }
}
