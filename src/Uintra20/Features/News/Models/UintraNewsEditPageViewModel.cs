﻿using System;
using System.Collections.Generic;
using System.Linq;
using UBaseline.Shared.Node;
using UBaseline.Shared.PageSettings;
using UBaseline.Shared.Property;
using Uintra20.Core.Member.Entities;
using Uintra20.Core.UbaselineModels.RestrictedNode;
using Uintra20.Features.Links.Models;

namespace Uintra20.Features.News.Models
{
    public class UintraNewsEditPageViewModel : UintraRestrictedNodeViewModel
    {
        public PropertyViewModel<INodeViewModel[]> Panels { get; set; }
        public PageSettingsCompositionViewModel PageSettings { get; set; }
        public NewsViewModel Details { get; set; }
        public bool CanEditOwner { get; set; }
        public bool PinAllowed { get; set; }
        public IEnumerable<IntranetMember> Members { get; set; } = Enumerable.Empty<IntranetMember>();
        public string AllowedMediaExtensions { get; set; }
        public Guid? GroupId { get; set; }
        public bool RequiresGroupHeader { get; set; }
        public IActivityLinks Links { get; set; }
    }
}