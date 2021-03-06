﻿using Compent.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uintra.Core.Activity;
using Uintra.Core.Activity.Entities;
using Uintra.Core.Feed.Models;
using Uintra.Core.Feed.Services;
using Uintra.Core.Feed.Settings;
using Uintra.Core.Member.Entities;
using Uintra.Core.Member.Services;
using Uintra.Core.Search.Entities;
using Uintra.Core.Search.Indexers;
using Uintra.Core.Search.Indexers.Diagnostics;
using Uintra.Core.Search.Indexers.Diagnostics.Models;
using Uintra.Core.Search.Indexes;
using Uintra.Features.CentralFeed.Enums;
using Uintra.Features.Comments.Services;
using Uintra.Features.Events.Entities;
using Uintra.Features.Groups.Services;
using Uintra.Features.Likes.Services;
using Uintra.Features.LinkPreview.Services;
using Uintra.Features.Links;
using Uintra.Features.Location.Services;
using Uintra.Features.Media.Enums;
using Uintra.Features.Media.Helpers;
using Uintra.Features.Media.Intranet.Services.Contracts;
using Uintra.Features.Media.Models;
using Uintra.Features.Notification;
using Uintra.Features.Notification.Entities.Base;
using Uintra.Features.Notification.Services;
using Uintra.Features.Permissions;
using Uintra.Features.Permissions.Interfaces;
using Uintra.Features.Reminder.Services;
using Uintra.Features.Subscribe;
using Uintra.Features.Subscribe.Models;
using Uintra.Features.Tagging.UserTags.Services;
using Uintra.Infrastructure.Caching;
using Uintra.Infrastructure.Extensions;
using Uintra.Infrastructure.TypeProviders;
using static Uintra.Features.Notification.Configuration.NotificationTypeEnum;

namespace Uintra.Features.Events
{
    public class EventsService : IntranetActivityService<Event>,
        IEventsService<Event>,
        IFeedItemService,
        ISubscribableService,
        INotifyableService,
        IReminderableService<Event>,
        IIndexer
    {
        private readonly ICommentsService _commentsService;
        private readonly ILikesService _likesService;
        private readonly ISubscribeService _subscribeService;
        private readonly INotificationsService _notificationService;
        private readonly IMediaHelper _mediaHelper;
        private readonly IElasticUintraActivityIndex _activityIndex;
        private readonly IDocumentIndexer _documentIndexer;
        private readonly IIntranetMediaService _intranetMediaService;
        private readonly IGroupActivityService _groupActivityService;
        private readonly IActivitySubscribeSettingService _activitySubscribeSettingService;
        private readonly IGroupService _groupService;
        private readonly INotifierDataBuilder _notifierDataBuilder;
        private readonly IActivityLinkService _activityLinkService;
        private readonly IIndexerDiagnosticService _indexerDiagnosticService;
        private readonly IUserTagService _userTagService;

        public EventsService(
            IIntranetActivityRepository intranetActivityRepository,
            ICacheService cacheService,
            IIntranetMemberService<IntranetMember> intranetMemberService,
            ICommentsService commentsService,
            ILikesService likesService,
            ISubscribeService subscribeService,
            IPermissionsService permissionsService,
            INotificationsService notificationService,
            IMediaHelper mediaHelper,
            IElasticUintraActivityIndex activityIndex,
            IDocumentIndexer documentIndexer,
            IActivityTypeProvider activityTypeProvider,
            IIntranetMediaService intranetMediaService,
            IGroupActivityService groupActivityService,
            IActivityLocationService activityLocationService,
            IActivitySubscribeSettingService activitySubscribeSettingService,
            IActivityLinkPreviewService activityLinkPreviewService,
            IGroupService groupService,
            INotifierDataBuilder notifierDataBuilder,
            IActivityLinkService activityLinkService,
            IIndexerDiagnosticService indexerDiagnosticService,
            IUserTagService userTagService)
            : base(
                intranetActivityRepository,
                cacheService,
                activityTypeProvider,
                intranetMediaService,
                activityLocationService,
                activityLinkPreviewService,
                intranetMemberService,
                permissionsService,
                groupActivityService,
                groupService)
        {
            _commentsService = commentsService;
            _likesService = likesService;
            _subscribeService = subscribeService;
            _notificationService = notificationService;
            _mediaHelper = mediaHelper;
            _activityIndex = activityIndex;
            _documentIndexer = documentIndexer;
            _intranetMediaService = intranetMediaService;
            _groupActivityService = groupActivityService;
            _activitySubscribeSettingService = activitySubscribeSettingService;
            _groupService = groupService;
            _notifierDataBuilder = notifierDataBuilder;
            _activityLinkService = activityLinkService;
            _indexerDiagnosticService = indexerDiagnosticService;
            _userTagService = userTagService;
        }

        public override Enum Type => IntranetActivityTypeEnum.Events;
        public override Enum PermissionActivityType => PermissionResourceTypeEnum.Events;

        public IEnumerable<Event> GetPastEvents()
        {
            return GetAll().Where(@event => !IsActual(@event) && !@event.IsHidden);
        }

        public IEnumerable<Event> GetComingEvents(DateTime fromDate)
        {
            var events = GetAll()
                .Where(e => e.StartDate > fromDate && IsActualPublishDate(e))
                .OrderBy(e => e.StartDate);
            return events;
        }

        public bool CanHide(Guid id)
        {
            var cached = Get(id);
            return CanHide(cached);
        }

        public bool CanHide(IIntranetActivity activity)
        {
            return CanPerform(activity, PermissionActionEnum.Hide, PermissionActionEnum.HideOther);
        }

        public void Hide(Guid id)
        {
            var @event = Get(id);
            @event.IsHidden = true;
            Save(@event);
        }

        public bool CanSubscribe(Guid activityId)
        {
            var @event = Get(activityId);
            return IsActual(@event) && @event.CanSubscribe;
        }

        public MediaSettings GetMediaSettings() =>
            _mediaHelper.GetMediaFolderSettings(MediaFolderTypeEnum.EventsContent, true);

        public FeedSettings GetFeedSettings() =>
            new FeedSettings
            {
                Type = CentralFeedTypeEnum.Events,
                HasSubscribersFilter = true,
                HasPinnedFilter = true
            };

        public IEnumerable<IFeedItem> GetItems() => GetOrderedActualItems();
        public IEnumerable<IFeedItem> GetGroupItems(Guid groupId)
        {
	        return GetOrderedActualItems().Where(a => a.GroupId == groupId);
        }

        public async Task<IEnumerable<IFeedItem>> GetItemsAsync()
        {
            return await GetOrderedActualItemsAsync();
        }

        public override Guid Create(IIntranetActivity activity)
        {
            return base.Create(activity, activityId =>
            {
                var subscribeSettings = Map(activity);
                subscribeSettings.ActivityId = activityId;
                _activitySubscribeSettingService.Create(subscribeSettings);
            });
        }

        public override void Save(IIntranetActivity activity)
        {
            base.Save(activity, savedActivity => _activitySubscribeSettingService.Save(Map(savedActivity)));
        }

        public override void Delete(Guid id)
        {
            _activitySubscribeSettingService.Delete(id);
            base.Delete(id);
        }

        private IOrderedEnumerable<Event> GetOrderedActualItems() =>
            GetManyActual().OrderByDescending(i => i.PublishDate);

        private async Task<IOrderedEnumerable<Event>> GetOrderedActualItemsAsync() =>
            (await GetManyActualAsync()).OrderByDescending(i => i.PublishDate);

        protected override void MapBeforeCache(IList<Event> cached)
        {
            foreach (var activity in cached)
            {
                var entity = activity;
                entity.GroupId = _groupActivityService.GetGroupId(activity.Id);
                _subscribeService.FillSubscribers(entity);
                _commentsService.FillComments(entity);
                _likesService.FillLikes(entity);
                _activitySubscribeSettingService.FillSubscribeSettings(entity);
            }
        }

        protected override async Task MapBeforeCacheAsync(IList<Event> cached)
        {
            foreach (var activity in cached)
            {
                var entity = activity;
                entity.GroupId = await _groupActivityService.GetGroupIdAsync(activity.Id);
                _subscribeService.FillSubscribers(entity);
                await _commentsService.FillCommentsAsync(entity);
                await _likesService.FillLikesAsync(entity);
                _activitySubscribeSettingService.FillSubscribeSettings(entity);
            }
        }

        protected override void UpdateCache()
        {
            base.UpdateCache();
            FillIndex();
        }

        public override Event UpdateActivityCache(Guid id)
        {
            var cachedEvent = Get(id);
            var @event = base.UpdateActivityCache(id);
            if (IsCacheable(@event) && (@event.GroupId is null || _groupService.IsActivityFromActiveGroup(@event)))
            {
                _activityIndex.Index(Map(@event));
                _documentIndexer.Index(@event.MediaIds);
                return @event;
            }

            if (cachedEvent == null) return null;

            _activityIndex.Delete(id);
            _documentIndexer.DeleteFromIndex(cachedEvent.MediaIds);
            _mediaHelper.DeleteMedia(cachedEvent.MediaIds);
            return null;
        }

        public override async Task<Event> UpdateActivityCacheAsync(Guid id)
        {
            var cachedEvent = await GetAsync(id);
            var @event = await base.UpdateActivityCacheAsync(id);
            if (IsCacheable(@event) && (@event.GroupId is null || _groupService.IsActivityFromActiveGroup(@event)))
            {
                _activityIndex.Index(Map(@event));
                _documentIndexer.Index(@event.MediaIds);
                return @event;
            }

            if (cachedEvent == null) return null;

            _activityIndex.Delete(id);
            _documentIndexer.DeleteFromIndex(cachedEvent.MediaIds);
            _mediaHelper.DeleteMedia(cachedEvent.MediaIds);
            return null;
        }

        public override bool IsActual(IIntranetActivity activity)
        {
            return base.IsActual(activity) && IsActualPublishDate((Event) activity);
        }

        public void UnSubscribe(Guid userId, Guid activityId)
        {
            _subscribeService.Unsubscribe(userId, activityId);
            UpdateActivityCache(activityId);
        }

        public void UpdateNotification(Guid id, bool value)
        {
            var subscribe = _subscribeService.UpdateNotification(id, value);
            UpdateActivityCache(subscribe.ActivityId);
        }

        public bool CanEditSubscribe(Guid activityId) => !Get(activityId).Subscribers.Any();

        public void Notify(Guid entityId, Enum notificationType)
        {
            NotifierData notifierData;

            if (notificationType.In(CommentAdded, CommentEdited, CommentLikeAdded, CommentReplied))
            {
                var comment = _commentsService.Get(entityId);
                var parentActivity = Get(comment.ActivityId);
                notifierData = _notifierDataBuilder.GetNotifierData(comment, parentActivity, notificationType);
            }
            else
            {
                var activity = Get(entityId);
                notifierData = _notifierDataBuilder.GetNotifierData(activity, notificationType);
            }

            _notificationService.ProcessNotification(notifierData);
        }

        public async Task NotifyAsync(Guid entityId, Enum notificationType)
        {
            NotifierData notifierData;

            if (notificationType.In(CommentAdded, CommentEdited, CommentLikeAdded, CommentReplied))
            {
                var comment = await _commentsService.GetAsync(entityId);
                var parentActivity = await GetAsync(comment.ActivityId);
                notifierData =
                    await _notifierDataBuilder.GetNotifierDataAsync(comment, parentActivity, notificationType);
            }
            else
            {
                var activity = await GetAsync(entityId);
                notifierData = await _notifierDataBuilder.GetNotifierDataAsync(activity, notificationType);
            }

            await _notificationService.ProcessNotificationAsync(notifierData);
        }

        public ISubscribable Subscribe(Guid userId, Guid activityId)
        {
            _subscribeService.Subscribe(userId, activityId);
            return UpdateActivityCache(activityId);
        }

        public Event GetActual(Guid id)
        {
            var @event = Get(id);
            return !@event.IsHidden ? @event : null;
        }

        public IndexedModelResult FillIndex()
        {
            try
            {
                var activities = GetAll().Where(IsCacheable);
                var searchableActivities = activities.Select(Map);
                _activityIndex.DeleteByType(UintraSearchableTypeEnum.Events);
                _activityIndex.Index(searchableActivities);

                return _indexerDiagnosticService.GetSuccessResult(typeof(EventsService).Name, searchableActivities);
            }
            catch (Exception e)
            {
                return _indexerDiagnosticService.GetFailedResult(e.Message + e.StackTrace, typeof(EventsService).Name);
            }
        }

        private bool IsCacheable(Event @event) => !IsEventHidden(@event) && IsActualPublishDate(@event);

        private bool IsActualPublishDate(Event @event) => DateTime.Compare(@event.PublishDate, DateTime.UtcNow) <= 0;

        private bool IsEventHidden(Event @event) => @event == null || @event.IsHidden;

        private SearchableUintraActivity Map(Event @event)
        {
            var searchableActivity = @event.Map<SearchableUintraActivity>();
            searchableActivity.Url = _activityLinkService.GetLinks(@event.Id).Details;
            searchableActivity.UserTagNames = _userTagService.Get(@event.Id).Select(t => t.Text);
            return searchableActivity;
        }

        private ActivitySubscribeSettingDto Map(IIntranetActivity activity)
        {
            var @event = (Event) activity;
            return @event.Map<ActivitySubscribeSettingDto>();
        }
    }
}