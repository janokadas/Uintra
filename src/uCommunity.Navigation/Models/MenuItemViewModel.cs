﻿using System.Collections.Generic;
using System.Linq;

namespace uCommunity.Navigation
{
    public class MenuItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public bool HideInNavigation { get; set; }
        public bool IsActive { get; set; }
        public bool IsHomePage { get; set; }
        public IEnumerable<MenuItemViewModel> Children { get; set; }
        //public List<MenuItemViewModel> Children { get; set; }

        public MenuItemViewModel()
        {
            Children = Enumerable.Empty<MenuItemViewModel>();
            //Children = new List<MenuItemViewModel>();
        }
    }
}
