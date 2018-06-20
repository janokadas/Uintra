﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compent.Extensions;
using Uintra.Core.Activity;
using Uintra.Core.ApplicationSettings;
using Uintra.Core.Exceptions;
using Uintra.Core.User;
using Uintra.Notification.Base;
using Uintra.Notification.Configuration;

namespace Uintra.Notification
{
    public abstract class MonthlyEmailServiceBase : IMonthlyEmailService
    {
        private readonly IMailService _mailService;
        private readonly IExceptionLogger _logger;
        private readonly IIntranetUserService<IIntranetUser> _intranetUserService;
        private readonly NotificationSettingsService _notificationSettingsService;
        private readonly IApplicationSettings _applicationSettings;

        protected MonthlyEmailServiceBase(IMailService mailService,
            IIntranetUserService<IIntranetUser> intranetUserService,
            IExceptionLogger logger,
            NotificationSettingsService notificationSettingsService,
            IApplicationSettings applicationSettings)
        {
            _mailService = mailService;
            _intranetUserService = intranetUserService;
            _logger = logger;
            _notificationSettingsService = notificationSettingsService;
            _applicationSettings = applicationSettings;
        }

        public void CreateAndSendMail()
        {
            var currentDate = DateTime.Now;

            var allUsers = _intranetUserService.GetAll();
            var monthlyMails = allUsers
                .Select(user => user.Id.Pipe(GetUserActivitiesFilteredByUserTags).Pipe(userActivities => TryGetMonthlyMail(userActivities, user)))
                .ToList();

            var identity = new ActivityEventIdentity(
                    CommunicationTypeEnum.CommunicationSettings,
                    NotificationTypeEnum.MonthlyMail)
                .AddNotifierIdentity(NotifierTypeEnum.EmailNotifier);

            var settings = _notificationSettingsService.Get<EmailNotifierTemplate>(identity);
            if (!settings.IsEnabled) return;

            foreach (var monthlyMail in monthlyMails)
            {
                monthlyMail.Do(some: mail =>
                {
                    var mailModel = GetMonthlyMailModel(mail.user, mail.monthlyMail, settings.Template);
                    try
                    {
                        _mailService.SendMailByTypeAndDay(
                            mailModel,
                            mail.user.Email,
                            currentDate,
                            NotificationTypeEnum.MonthlyMail);
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ex);
                    }
                });
            }
        }

        public void ProcessMonthlyEmail()
        {
            if (IsSendingDay())
            {
                CreateAndSendMail();
            }
        }



        protected (IIntranetUser user, MonthlyMailDataModel monthlyMail)? TryGetMonthlyMail(
            IEnumerable<(IIntranetActivity activity, string detailsLink)> activities,
            IIntranetUser user)
        {
            var activityList = activities.AsList();
            if (activityList.Any())
            {
                var activityListString = GetActivityListString(activityList);
                var monthlyMail = GetMonthlyMailModel(activityListString, user);
                return (user, monthlyMail);
            }
            else
            {
                return default;
            }
        }

        protected abstract IEnumerable<(IIntranetActivity activity, string detailsLink)> GetUserActivitiesFilteredByUserTags(Guid userId);

        protected abstract MailBase GetMonthlyMailModel(IIntranetUser receiver, MonthlyMailDataModel dataModel, EmailNotifierTemplate template);

        protected virtual MonthlyMailDataModel GetMonthlyMailModel(string userActivities, IIntranetUser user) =>
            new MonthlyMailDataModel
            {
                ActivityList = userActivities
            };

        protected virtual bool IsSendingDay()
        {
            var currentDate = DateTime.Now;

            return currentDate.Day != _applicationSettings.MonthlyEmailJobDay;
        }

        private string GetActivityListString(IEnumerable<(IIntranetActivity activity, string link)> activities) => activities
            .Aggregate(
                new StringBuilder(),
                (builder, activity) => builder.AppendLine($"<a href='{activity.link}'>{activity.activity.Title}</a></br>"))
            .ToString();

    }
}
