﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Compent.uCommunity.Core.Events;
using uCommunity.CentralFeed;
using uCommunity.Core;
using uCommunity.Core.Activity;
using uCommunity.Core.Activity.Models;
using uCommunity.Core.Controls.LightboxGallery;
using uCommunity.Core.Extentions;
using uCommunity.Core.Media;
using uCommunity.Core.User;
using uCommunity.Core.User.Permissions.Web;
using uCommunity.Events;
using uCommunity.Events.Web;
using uCommunity.Notification.Core.Configuration;
using uCommunity.Notification.Core.Services;
using uCommunity.Tagging;
using uCommunity.Users.Core;
using Umbraco.Core;

namespace Compent.uCommunity.Controllers
{
    public class EventsController : EventsControllerBase
    {
        protected override string ListViewPath => "~/Views/Events/ListView.cshtml";
        protected override string DetailsViewPath => "~/Views/Events/DetailsView.cshtml";
        protected override string CreateViewPath => "~/Views/Events/CreateView.cshtml";
        protected override string EditViewPath => "~/Views/Events/EditView.cshtml";
        protected override string ItemViewPath => "~/Views/Events/ItemView.cshtml";
        protected override int ShortDescriptionLength { get; } = 500;

        private readonly IEventsService<Event> _eventsService;
        private readonly IMediaHelper _mediaHelper;
        private readonly IIntranetUserService<IntranetUser> _intranetUserService;
        private readonly IReminderService _reminderService;
        private readonly ITagsService _tagsService;

        public EventsController(IEventsService<Event> eventsService,
            IMediaHelper mediaHelper,
            IIntranetUserService<IntranetUser> intranetUserService,
            IReminderService reminderService,
            ITagsService tagsService)
            : base(eventsService, mediaHelper, intranetUserService)
        {
            _eventsService = eventsService;
            _mediaHelper = mediaHelper;
            _intranetUserService = intranetUserService;
            _reminderService = reminderService;
            _tagsService = tagsService;
        }

        public ActionResult CentralFeedItem(ICentralFeedItem item)
        {
            var activity = item as Event;
            return PartialView(ItemViewPath, GetItemViewModel(activity));
        }

        public override ActionResult Details(Guid id)
        {
            var @event = _eventsService.Get(id);

            if (@event.IsHidden)
            {
                HttpContext.Response.Redirect(_eventsService.GetOverviewPage().Url);
            }

            var model = @event.Map<IntranetEventViewModel>();
            model.HeaderInfo = @event.Map<IntranetActivityDetailsHeaderViewModel>();
            model.HeaderInfo.Dates = new List<string> { @event.StartDate.ToDateTimeFormat(), @event.EndDate.ToDateTimeFormat() };
            model.CanEdit = _eventsService.CanEdit(@event);
            model.CanSubscribe = _eventsService.CanSubscribe(@event);

            return PartialView(DetailsViewPath, model);
        }

        [RestrictedAction(IntranetActivityActionEnum.Create)]
        public override ActionResult Create()
        {
            var model = new EventExtendedActivityCreateModel
            {
                StartDate = DateTime.Now.Date.AddHours(8),
                EndDate = DateTime.Now.Date.AddHours(8),
                CanSubscribe = true
            };
            FillCreateEditData(model);
            return PartialView(CreateViewPath, model);
        }

        [NonAction]
        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Create)]
        public override ActionResult Create(EventCreateModel createModel)
        {
            return base.Create(createModel);
        }

        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Create)]
        public ActionResult Create(EventExtendedActivityCreateModel activityCreateModel)
        {
            if (!ModelState.IsValid)
            {
                FillCreateEditData(activityCreateModel);
                return PartialView(CreateViewPath, activityCreateModel);
            }

            var @event = activityCreateModel.Map<EventBase>();
            @event.MediaIds = @event.MediaIds.Concat(_mediaHelper.CreateMedia(activityCreateModel));
            @event.CreatorId = _intranetUserService.GetCurrentUserId();

            if (activityCreateModel.IsPinned && activityCreateModel.PinDays > 0)
            {
                @event.EndPinDate = DateTime.Now.AddDays(activityCreateModel.PinDays);
            }

            var activityId = _eventsService.Create(@event);
            _tagsService.Save(activityId, activityCreateModel.Tags.Map<IEnumerable<TagDTO>>());
            OnEventCreated(activityId);

            return RedirectToUmbracoPage(_eventsService.GetDetailsPage(), new NameValueCollection { { "id", activityId.ToString() } });
        }

        [RestrictedAction(IntranetActivityActionEnum.Edit)]
        public override ActionResult Edit(Guid id)
        {
            var @event = _eventsService.Get(id);
            if (@event.IsHidden)
            {
                HttpContext.Response.Redirect(_eventsService.GetOverviewPage().Url);
            }

            if (!_eventsService.CanEdit(@event))
            {
                HttpContext.Response.Redirect(_eventsService.GetDetailsPage().Url.UrlWithQueryString("id", id.ToString()));
            }

            _tagsService.FillTags(@event);

            var model = @event.Map<EventExtendedEditModel>();
            model.CanEditSubscribe = _eventsService.CanEditSubscribe(@event.Id);
            FillCreateEditData(model);
            return PartialView(EditViewPath, model);
        }

        [NonAction]
        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Edit)]
        public override ActionResult Edit(EventEditModel saveModel)
        {
            return base.Edit(saveModel);
        }

        [HttpPost]
        [RestrictedAction(IntranetActivityActionEnum.Edit)]
        public ActionResult Edit(EventExtendedEditModel saveModel)
        {
            if (!ModelState.IsValid)
            {
                FillCreateEditData(saveModel);
                return PartialView(EditViewPath, saveModel);
            }

            var @event = MapEditModel(saveModel);
            @event.MediaIds = @event.MediaIds.Concat(_mediaHelper.CreateMedia(saveModel));
            @event.CreatorId = _intranetUserService.GetCurrentUserId();

            if (_eventsService.CanEditSubscribe(@event.Id))
            {
                @event.CanSubscribe = saveModel.CanSubscribe;
            }
            var isActual = _eventsService.IsActual(@event);
            _eventsService.Save(@event);
            _tagsService.Save(saveModel.Id, saveModel.Tags.Map<IEnumerable<TagDTO>>());

            if (saveModel.IsPinned && saveModel.PinDays > 0 && @event.PinDays != saveModel.PinDays)
            {
                @event.EndPinDate = DateTime.Now.AddDays(saveModel.PinDays);
            }

            OnEventEdited(@event.Id, isActual, saveModel.NotifyAllSubscribers);

            return RedirectToUmbracoPage(_eventsService.GetDetailsPage(), new NameValueCollection { { "id", @event.Id.ToString() } });
        }

        [NonAction]
        [HttpPost]
        public override JsonResult HasConfirmation(EventEditModel model)
        {
            return base.HasConfirmation(model);
        }

        [HttpPost]
        public JsonResult HasConfirmation(EventExtendedEditModel model)
        {
            var @event = MapModel(model);
            return Json(new { HasConfirmation = _eventsService.IsActual(@event) && @event.Subscribers.Any() });
        }

        public ActionResult ItemView(EventOverviewItemModel model)
        {
            return PartialView(ItemViewPath, model);
        }

        protected Event MapModel(EventExtendedEditModel saveModel)
        {
            var @event = _eventsService.Get(saveModel.Id);
            @event = Mapper.Map(saveModel, @event);
            return @event;
        }

        protected override void OnEventCreated(Guid activityId)
        {
            _reminderService.CreateIfNotExists(activityId, ReminderTypeEnum.OneDayBefore);
        }

        protected override void OnEventEdited(Guid id, bool isActual, bool notifySubscribers)
        {
            if (isActual)
            {
                if (notifySubscribers)
                {
                    ((INotifyableService)_eventsService).Notify(id, NotificationTypeEnum.EventUpdated);
                }

                _reminderService.CreateIfNotExists(id, ReminderTypeEnum.OneDayBefore);
            }
        }

        protected override void OnEventHidden(Guid id)
        {
            ((INotifyableService)_eventsService).Notify(id, NotificationTypeEnum.EventHided);
        }
    }
}