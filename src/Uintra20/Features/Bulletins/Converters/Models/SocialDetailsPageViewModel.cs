﻿using System.Collections.Generic;
using System.Linq;
using UBaseline.Shared.Node;
using UBaseline.Shared.PageSettings;
using UBaseline.Shared.Property;
using Uintra20.Features.Bulletins.Models;
using Uintra20.Features.Comments.Models;
using Uintra20.Features.Likes.Sql;
using Uintra20.Features.Tagging.UserTags.Models;

namespace Uintra20.Features.Bulletins.Converters.Models
{
    public class SocialDetailsPageViewModel : NodeViewModel
    {
        public PropertyViewModel<INodeViewModel[]> Panels { get; set; }
        public PageSettingsCompositionViewModel PageSettings { get; set; }
        public SocialExtendedViewModel Details { get; set; }
        public IEnumerable<UserTag> Tags { get; set; } = Enumerable.Empty<UserTag>();
        public IEnumerable<Like> Likes { get; set; } = Enumerable.Empty<Like>();
        public IEnumerable<CommentViewModel> Comments = Enumerable.Empty<CommentViewModel>();
        public bool LikedByCurrentUser { get; set; }
    }
}