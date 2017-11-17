﻿using System;

namespace uIntra.Events.Dashboard
{
    public class EventBackofficeViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Media { get; set; }
        public Guid OwnerId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsHidden { get; set; }
        public string ModifyDate { get; set; }
        public string CreatedDate { get; set; }
    }
}