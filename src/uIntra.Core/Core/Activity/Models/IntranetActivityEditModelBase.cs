﻿using System;
using System.ComponentModel.DataAnnotations;
using uIntra.Core.TypeProviders;
using uIntra.Core.User;

namespace uIntra.Core.Activity
{
    public class IntranetActivityEditModelBase
    {
        [Required]
        public virtual Guid Id { get; set; }
        [Required]
        public virtual string Title { get; set; }

        public bool IsPinned { get; set; }
        
        public virtual DateTime? EndPinDate { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        public IIntranetUser Creator { get; set; }
        public IIntranetType ActivityType { get; set; }
    }
}