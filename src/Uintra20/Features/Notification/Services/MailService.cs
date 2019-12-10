﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Uintra20.Features.Notification.Configuration;
using Uintra20.Features.Notification.Entities.Base.Mails;
using Uintra20.Infrastructure.Constants;
using Uintra20.Infrastructure.Extensions;
using Uintra20.Infrastructure.Helpers;
using Uintra20.Infrastructure.Providers;
using Umbraco.Web;

namespace Uintra20.Features.Notification.Services
{
    public class MailService : IMailService
    {
        //private readonly IEmailJobSenderService _emailJobSenderService;
        //private readonly ISentMailsService _sentMailsService;
        private readonly UmbracoHelper _umbracoHelper;

        public MailService(
            //IEmailJobSenderService emailJobSenderService,
            //ISentMailsService sentMailsService,
            UmbracoHelper umbracoHelper)
        {
            //_emailJobSenderService = emailJobSenderService;
            //_sentMailsService = sentMailsService;
            _umbracoHelper = umbracoHelper;
        }

        public void Send(MailBase mail)
        {
            //if (mail is IEmailBase email)
            //{
            //    foreach (var recipient in mail.Recipients)
            //    {
            //        var sentMailsModel = new SentMails
            //        {
            //            Body = email.Body,
            //            Subject = email.Subject,
            //            IsSent = false,
            //            CreatedUtcDate = DateTime.UtcNow,
            //            FromEmail = email.FromEmail ?? string.Empty,
            //            FromName = email.FromName ?? string.Empty,
            //            ToEmail = recipient.Email,
            //            ToName = recipient.Name,

            //        };
            //        _sentMailsService.Insert(sentMailsModel);
            //    }
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
        }

        public void ProcessMails(int? count = null, int? mailId = null)
        {
            //_emailJobSenderService.SendMails(string.Empty, count, mailId);
        }

        public void SendMailByTypeAndDay(MailBase mail, string email, DateTime date,
            NotificationTypeEnum mailTemplateTypeEnum)
        {
            //var query = new EmailLogQuery
            //{
            //    StartCreateDate = new DateTime(date.Year, date.Month, date.Day),
            //    TypeId = GetEmailTemplatePublishedContentId(mailTemplateTypeEnum),
            //    ToEmail = email
            //};

            //_sentMailsService.GetAllByFilter(query, out var totalCount);
            //if (totalCount == 0)
            //{
            //    Send(mail);
            //}
        }

        public async Task SendAsync(MailBase mail)
        {
            //if (mail is IEmailBase email)
            //{
            //    foreach (var recipient in mail.Recipients)
            //    {
            //        var sentMailsModel = new SentMails
            //        {
            //            Body = email.Body,
            //            Subject = email.Subject,
            //            IsSent = false,
            //            CreatedUtcDate = DateTime.UtcNow,
            //            FromEmail = email.FromEmail ?? string.Empty,
            //            FromName = email.FromName ?? string.Empty,
            //            ToEmail = recipient.Email,
            //            ToName = recipient.Name,

            //        };
            //        await _sentMailsService.InsertAsync(sentMailsModel);
            //    }
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
        }

        public async Task SendMailByTypeAndDayAsync(MailBase mail, string email, DateTime date, NotificationTypeEnum mailTemplateTypeEnum)
        {
            //var query = new EmailLogQuery
            //{
            //    StartCreateDate = new DateTime(date.Year, date.Month, date.Day),
            //    TypeId = GetEmailTemplatePublishedContentId(mailTemplateTypeEnum),
            //    ToEmail = email
            //};

            //_sentMailsService.GetAllByFilter(query, out var totalCount);
            //if (totalCount == 0)
            //{
            //    await SendAsync(mail);
            //}
        }

        private int? GetEmailTemplatePublishedContentId(NotificationTypeEnum mailTemplateTypeEnum)
        {
            //TODO: research when mail service is ready
            var docTypeAliasProvider = HttpContext.Current.GetService<IDocumentTypeAliasProvider>();
            string mailTemplateXpath = string.Empty;//XPathHelper.GetXpath(docTypeAliasProvider.GetDataFolder(),
                //docTypeAliasProvider.GetMailTemplateFolder(), docTypeAliasProvider.GetMailTemplate());
            var mailTemplates = _umbracoHelper.ContentAtXPath(mailTemplateXpath);
            var mailTemplateContent = mailTemplates?.FirstOrDefault(template =>
                template.Value<NotificationTypeEnum>(MailTemplatePropertiesConstants.EmailType) ==
                mailTemplateTypeEnum);
            return mailTemplateContent?.Id;
        }
    }
}