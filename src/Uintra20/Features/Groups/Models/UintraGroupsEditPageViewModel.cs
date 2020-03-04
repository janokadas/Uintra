﻿using System;
using UBaseline.Shared.Node;
using UBaseline.Shared.PageSettings;
using UBaseline.Shared.Property;
using Uintra20.Core.UbaselineModels.RestrictedNode;

namespace Uintra20.Features.Groups.Models
{
    public class UintraGroupsEditPageViewModel : UintraRestrictedNodeViewModel
    {
        public PropertyViewModel<INodeViewModel[]> Panels { get; set; }
        public PageSettingsCompositionViewModel PageSettings { get; set; }
        public GroupNavigationCompositionViewModel GroupNavigation { get; set; }
        public GroupInfoViewModel Info { get; set; }
        public string AllowedMediaExtensions { get; set; }
        public Guid? GroupId { get; set; }
    }
}