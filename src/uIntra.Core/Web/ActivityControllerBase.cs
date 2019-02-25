﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Uintra.Core.Activity;
using Uintra.Core.Extensions;
using Uintra.Core.Links;
using Uintra.Core.Permissions;
using Uintra.Core.User;
using Umbraco.Web.Mvc;

namespace Uintra.Core.Web
{
    public abstract class ActivityControllerBase : SurfaceController
    {
        protected virtual string DetailsHeaderViewPath { get; } = "~/App_Plugins/Core/Activity/ActivityDetailsHeader.cshtml";
        protected virtual string ItemHeaderViewPath { get; } = "~/App_Plugins/Core/Activity/ActivityItemHeader.cshtml";
        protected virtual string OwnerEditViewPath { get; } = "~/App_Plugins/Core/Activity/ActivityOwnerEdit.cshtml";
        protected virtual string PinActivityViewPath { get; } = "~/App_Plugins/Core/Activity/ActivityPinView.cshtml";

        private readonly IIntranetMemberService<IIntranetMember> _intranetMemberService;
        private readonly IPermissionsService _basePermissionsService;

        protected ActivityControllerBase(
            IIntranetMemberService<IIntranetMember> intranetMemberService, IPermissionsService basePermissionsService)
        {
            _intranetMemberService = intranetMemberService;
            _basePermissionsService = basePermissionsService;
        }

        public virtual ActionResult DetailsHeader(IntranetActivityDetailsHeaderViewModel header)
        {
            return PartialView(DetailsHeaderViewPath, header);
        }

        public virtual ActionResult ItemHeader(object header)
        {
            return PartialView(ItemHeaderViewPath, header);
        }

        public virtual ActionResult OwnerEdit(Guid ownerId, string ownerIdPropertyName, PermissionActivityTypeEnum activityType, IActivityCreateLinks links)
        {
            var model = new IntranetActivityOwnerEditModel
            {
                Owner = _intranetMemberService.Get(ownerId).Map<MemberViewModel>(),
                OwnerIdPropertyName = ownerIdPropertyName,
                Links = links
            };

            model.CanEditOwner = _basePermissionsService.Check( activityType, PermissionActionEnum.EditOwner);
            if (model.CanEditOwner)
            {
                model.Members = GetUsersWithAccess(activityType, PermissionActionEnum.Create);
            }

            return PartialView(OwnerEditViewPath, model);
        }

        public virtual ActionResult PinActivity(bool isPinned, DateTime? endPinDate)
        {
            return PartialView(PinActivityViewPath,
                new IntranetPinActivityModel
                {
                    IsPinned = isPinned,
                    EndPinDate = endPinDate ?? DateTime.Now
                });
        }

        protected virtual IEnumerable<IIntranetMember> GetUsersWithAccess(PermissionActivityTypeEnum activityType, PermissionActionEnum action)
        {
            var result = _intranetMemberService
                .GetAll()
                .Where(member => _basePermissionsService.Check(member, activityType, action))
                .OrderBy(user => user.DisplayedName);

            return result;
        }
    }
}