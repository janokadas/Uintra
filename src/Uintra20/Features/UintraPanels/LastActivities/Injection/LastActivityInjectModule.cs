﻿using Compent.Shared.DependencyInjection.Contract;
using Uintra20.Features.UintraPanels.LastActivities.Helpers;

namespace Uintra20.Features.UintraPanels.LastActivities.Injection
{
    public class LastActivityInjectModule : IInjectModule
    {
        public IDependencyCollection Register(IDependencyCollection services)
        {
            services.AddScoped<ICentralFeedHelper, CentralFeedHelper>();

            return services;
        }
    }
}