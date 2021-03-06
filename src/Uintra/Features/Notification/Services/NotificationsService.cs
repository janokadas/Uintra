﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uintra.Features.Notification.Entities.Base;
using Uintra.Persistence.Sql;
using Umbraco.Core.Logging;

namespace Uintra.Features.Notification.Services
{
    public class NotificationsService : INotificationsService
    {
        private readonly ISqlRepository<Sql.Notification> _notificationRepository;
        private readonly ILogger _logger;
        private readonly IMemberNotifiersSettingsService _memberNotifiersSettingsService;
        private readonly IEnumerable<INotifierService> _notifiers;

        public NotificationsService(
            ISqlRepository<Sql.Notification> notificationRepository,
            ILogger logger,
            IMemberNotifiersSettingsService memberNotifiersSettingsService,
            IEnumerable<INotifierService> notifiers)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _memberNotifiersSettingsService = memberNotifiersSettingsService;
            _notifiers = notifiers;
        }

        public void ProcessNotification(NotifierData data)
        {
            var allReceiversIds = data.ReceiverIds.ToList();
            var allReceiversNotifiersSettings = _memberNotifiersSettingsService.GetForMembers(allReceiversIds);

            (IEnumerable<Guid> receiverIds, bool isNotEmpty) GetReceiverIdsForNotifier(Enum notifierType)
            {
                var ids = allReceiversIds
                    .Where(receiverId => allReceiversNotifiersSettings[receiverId].Contains(notifierType))
                    .ToList();
                return (ids, ids.Any());
            }

            foreach (var notifier in _notifiers)
            {
                var filterResult = GetReceiverIdsForNotifier(notifier.Type);
                if (filterResult.isNotEmpty) Notify(notifier, data);
            }
        }

        public IEnumerable<Sql.Notification> GetUserNotifications(Guid userId, int notificationTypeId)
        {
            return _notificationRepository.FindAll(n => n.ReceiverId == userId && n.Type == notificationTypeId);
        }

        protected void Notify(INotifierService service, NotifierData data)
        {
            try
            {
                service.Notify(data);
            }
            catch (Exception ex)
            {
                _logger.Error<NotificationsService>(ex);
            }
        }

        public async Task ProcessNotificationAsync(NotifierData data)
        {
            var allReceiversIds = data.ReceiverIds.ToList();
            var allReceiversNotifiersSettings = await _memberNotifiersSettingsService.GetForMembersAsync(allReceiversIds);

            (IEnumerable<Guid> receiverIds, bool isNotEmpty) GetReceiverIdsForNotifier(Enum notifierType)
            {
                var ids = allReceiversIds
                    .Where(receiverId => allReceiversNotifiersSettings[receiverId].Contains(notifierType))
                    .ToList();
                return (ids, ids.Any());
            }

            foreach (var notifier in _notifiers)
            {
                var filterResult = GetReceiverIdsForNotifier(notifier.Type);
                if (filterResult.isNotEmpty) await NotifyAsync(notifier, data);
            }
        }

        public async Task<IEnumerable<Sql.Notification>> GetUserNotificationsAsync(Guid userId, int notificationTypeId)
        {
            return await _notificationRepository.FindAllAsync(n => n.ReceiverId == userId && n.Type == notificationTypeId);
        }

        protected async Task NotifyAsync(INotifierService service, NotifierData data)
        {
            try
            {
                await service.NotifyAsync(data);
            }
            catch (Exception ex)
            {
                _logger.Error<NotificationsService>(ex);
            }
        }

    }
}