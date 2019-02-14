﻿var app = angular.module('umbraco');
//app.requires.push('angular.filter');
//var app = angular.module('umbraco', ['angular.filter']);

app.filter('groupBy', function ($parse) {
    return _.memoize(function (items, field) {
        var getter = $parse(field);
        return _.groupBy(items, function (item) {
            return getter(item);
        });
    });
});

app.controller('memberGroups.editController',
    function (/*memberGroupsService,*/ $scope, $routeParams, $http, notificationsService, $location, navigationService, currentUserResource, usersResource, editorState, userService) {

        //console.log(currentUserResource);
        //console.log('----------------');
        //console.log(usersResource);
        //console.log(editorState);
        //console.log(userService);
        //var currentUser = userService.getCurrentUser()
        //    .then(function (user) {
        //
        //        console.log(user);
        //    });

        var vm = this;
        vm.memberGroup = null;
        var memberGroupId = $routeParams.id;

        if ($routeParams.create) {
            vm.memberGroup = {};
            vm.isCreate = true;
        } else {
            $http.get('/umbraco/backoffice/api/MemberGroup/Get?id=' + memberGroupId).success(function (response) {
                vm.memberGroup = response;
            });

            $http.get('/umbraco/backoffice/api/MemberGroup/IsSuperUser')
                .success(function (response) {
                    //console.log(response);

                    vm.isSuperUser = response;

                    //TODO get from backend
                    vm.permissions = [
                        { actionId: 0, actionName: "read", activityTypeId: 100, activityTypeName: "Events", enabled: true, allowed: true },
                        { actionId: 1, actionName: "create", activityTypeId: 101, activityTypeName: "Events", enabled: true, allowed: true },
                        { actionId: 2, actionName: "delete", activityTypeId: 102, activityTypeName: "Events", enabled: false, allowed: true },
                        { actionId: 3, actionName: "update", activityTypeId: 103, activityTypeName: "Events", enabled: true, allowed: true },

                        { actionId: 4, actionName: "read", activityTypeId: 110, activityTypeName: "News", enabled: true, allowed: true },
                        { actionId: 5, actionName: "create", activityTypeId: 111, activityTypeName: "News", enabled: true, allowed: true },
                        { actionId: 6, actionName: "delete", activityTypeId: 112, activityTypeName: "News", enabled: false, allowed: true },
                        { actionId: 7, actionName: "update", activityTypeId: 113, activityTypeName: "News", enabled: false, allowed: true },

                        { actionId: 8, actionName: "read", activityTypeId: 120, activityTypeName: "Bulletins", enabled: true, allowed: true },
                        { actionId: 9, actionName: "create", activityTypeId: 121, activityTypeName: "Bulletins", enabled: true, allowed: true },
                        { actionId: 10, actionName: "delete", activityTypeId: 122, activityTypeName: "Bulletins", enabled: false, allowed: true },
                        { actionId: 11, actionName: "update", activityTypeId: 123, activityTypeName: "Bulletins", enabled: true, allowed: true }
                    ];
                });
            
        }

        vm.property = {
            label: "Name",
            description: "Member group name"
        };
        vm.permissionsProperty = {
            label: "",
            description: "Activity type name"
        };

        vm.getProperty = function (activityTypeName) {
            vm.permissionsProperty.label = activityTypeName;
            return vm.permissionsProperty;
        };

        vm.toggleEnabled = function myfunction(permission) {
            //TODO backend support
            permission.enabled = !permission.enabled;
        };

        vm.toggleAllowed = function myfunction(permission) {
            //TODO backend support
            permission.allowed = !permission.allowed;
        };

        vm.buttonState = "init";
        vm.save = function () {
            vm.buttonState = "busy";
            if (vm.isCreate) {
                $http.post('/umbraco/backoffice/api/MemberGroup/Create', { name: vm.memberGroup.name })
                    .success(function (response) {
                        navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", response.toString()], forceReload: true, activate: false });
                        $location.url("/" + $routeParams.section + "/" + $routeParams.tree + "/" + $routeParams.method + "/" + response);
                    });
                return;
            }
            $http.post('/umbraco/backoffice/api/MemberGroup/Save', { id: memberGroupId, name: vm.memberGroup.name })
                .success(function (response) {
                    vm.buttonState = "success";
                    navigationService.syncTree({ tree: $routeParams.tree, path: ["-1", memberGroupId.toString()], forceReload: true, activate: false });
                });
        };
    });