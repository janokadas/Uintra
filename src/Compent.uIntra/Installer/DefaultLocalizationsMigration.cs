﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Localization.Core;
using uIntra.Core.Extentions;
using uIntra.Core.Installer.Migrations;

namespace Compent.uIntra.Installer
{
    public class DefaultLocalizationsMigration
    {
        private readonly string DefaultLocalizationsEmbeddedResourceFilePath = $"{Assembly.GetExecutingAssembly().GetName().Name}.Installer.PreValues.DefaultLocalizations.json";

        private readonly ILocalizationCoreService _localizationCoreService;

        public DefaultLocalizationsMigration()
        {
            _localizationCoreService = HttpContext.Current.GetService<ILocalizationCoreService>();
        }

        public void Init()
        {
            AddLocalizations();
        }

        private void AddLocalizations()
        {
            var existedLocalizations = _localizationCoreService.GetAllResourceModels();

            var fileContent = CoreInstallationStep_0_0_1.GetEmbeddedResourceValue(DefaultLocalizationsEmbeddedResourceFilePath, Assembly.GetExecutingAssembly());
            var newLocalizations = fileContent.Deserialize<List<ResourceModel>>();

            var parentKeys = newLocalizations
                .Where(loc => loc.ParentKey.IsNotNullOrEmpty())
                .Select(loc => loc.ParentKey)
                .Distinct();

            var parentLocalizations = newLocalizations.Where(loc => parentKeys.Contains(loc.Key)).ToList();

            AddNewLocalizationsIfNotExists(existedLocalizations, parentLocalizations);
            AddNewLocalizationsIfNotExists(existedLocalizations, newLocalizations.Except(parentLocalizations));
        }

        private void AddNewLocalizationsIfNotExists(List<ResourceModel> existedLocalizations, IEnumerable<ResourceModel> newLocalizations)
        {
            foreach (var loc in newLocalizations)
            {
                if (existedLocalizations.Exists(el => el.Key.Equals(loc.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                _localizationCoreService.Create(loc);
            }
        }
    }
}