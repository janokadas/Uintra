﻿using System;
using uIntra.Core.Links;

namespace uIntra.CentralFeed
{
    public class ActivityFeedTabModel : TabModelBase
    {
        public Enum Type { get; set; }
        public bool HasSubscribersFilter { get; set; }
        public bool HasPinnedFilter { get; set; }        
        public IActivityCreateLinks Links { get; set; }
    }
}
