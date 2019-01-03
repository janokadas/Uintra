﻿using System.ComponentModel.DataAnnotations;

namespace Uintra.Core.User.DTO
{
    public class CreateUserDto
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "FirstName Allowed length 1 - 50")]
        public string FirstName { get; set; }

        [StringLength(50, MinimumLength = 1, ErrorMessage = "LastName Allowed length 1 - 50")]
        public string LastName { get; set; }

        public string Phone { get; set; }

        public string Department { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is absent or empty")]
        public string Email { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Role is absent or empty")]
        public IntranetRolesEnum Role { get; set; }

        public int? MediaId { get; set; }
    }
}
