﻿using System;
using System.Collections.Generic;
using System.Linq;
using uIntra.Core.Activity;
using uIntra.Core.Exceptions;
using uIntra.Core.TypeProviders;

namespace uIntra.Core.User.Permissions
{
    public class PermissionsService : IPermissionsService
    {
        private readonly IPermissionsConfiguration _configuration;
        private readonly IExceptionLogger _exceptionLogger;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;

        public PermissionsService(
            IPermissionsConfiguration configuration,
            IExceptionLogger exceptionLogger,
            IIntranetUserService<IIntranetUser> intranetUserService)
        {
            _configuration = configuration;
            _exceptionLogger = exceptionLogger;
            _intranetUserService = intranetUserService;
        }

        public virtual bool IsRoleHasPermissions(IRole role, params string[] permissions)
        {
            if (permissions.Any())
            {
                var rolePermissions = GetRolePermission(role);
                return rolePermissions.Intersect(permissions).Any();
            }

            var defaultValue = false;
            _exceptionLogger.Log(new Exception($"Tryed check role permissions but no permissions was passed into method! Return {defaultValue}!!"));

            return defaultValue;
        }

        public virtual IEnumerable<string> GetRolePermission(IRole role)
        {
            var roleConfiguration = _configuration.Roles.FirstOrDefault(s => s.Key == role.Name);

            if (roleConfiguration == null)
            {
                throw new Exception($"Can't find permissions for role {role.Name}. Please check permissions config!");
            }

            return roleConfiguration.Permissions.Select(s => s.Key);
        }

        public virtual string GetPermissionFromTypeAndAction(IIntranetType activityType, IntranetActivityActionEnum action)
        {
            return $"{activityType.Name}{action}";
        }

        public virtual bool IsCurrentUserHasAccess(IIntranetType activityType, IntranetActivityActionEnum action)
        {
            var currentUser = _intranetUserService.GetCurrentUser();
            if (currentUser == null)
            {
                return false;
            }

            var result = IsUserHasAccess(currentUser, activityType, action);
            return result;
        }

        public virtual bool IsUserHasAccess(IIntranetUser user, IIntranetType activityType, IntranetActivityActionEnum action)
        {
            if (user == null)
            {
                return false;
            }

            if (IsUserWebmaster(user))
            {
                return true;
            }

            var permission = $"{activityType.Name}{action}";
            var userHasPermissions = IsRoleHasPermissions(user.Role, permission);

            return userHasPermissions;
        }

        public virtual bool IsUserWebmaster(IIntranetUser user)
        {
            return user.Role.Name == IntranetRolesEnum.WebMaster.ToString();
        }
    }
}