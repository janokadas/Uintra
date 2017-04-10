﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using uCommunity.Core;
using uCommunity.Core.Activity.Models;
using uCommunity.Core.Extentions;
using uCommunity.Core.Media;
using Umbraco.Web.Mvc;

namespace uCommunity.Events.App_Plugins.Events
{
    public class EventsController : SurfaceController
    {
        private readonly IEventsService<EventBase, Compent.uCommunity.Core.Events.Event> _eventsService;
        private readonly IMediaHelper _mediaHelper;

        public EventsController(
            IEventsService<EventBase, Compent.uCommunity.Core.Events.Event> eventsService,
            IMediaHelper mediaHelper)
        {
            _eventsService = eventsService;
            _mediaHelper = mediaHelper;
        }

        public ActionResult OverView()
        {
            FillLinks();
            return PartialView("~/App_Plugins/Events/List/OverView.cshtml");
        }

        public ActionResult List(EventType type, bool showOnlySubscribed)
        {
            IEnumerable<Compent.uCommunity.Core.Events.Event> events = type == EventType.Actual ?
                _eventsService.GetManyActual().OrderBy(item => item.StartDate).ThenBy(item => item.EndDate) :
                _eventsService.GetPastEvents().OrderByDescending(item => item.StartDate).ThenByDescending(item => item.EndDate);

            FillLinks();
            return PartialView("~/App_Plugins/Events/List/ListView.cshtml", GetOverviewItems(events));
        }

        public ActionResult Details(Guid id)
        {
            var @event = _eventsService.Get(id);

            if (@event.IsHidden)
            {
                HttpContext.Response.Redirect(_eventsService.GetOverviewPage().Url);
            }

            var model = @event.Map<EventViewModelBase>();
            model.HeaderInfo = @event.Map<IntranetActivityDetailsHeaderViewModel>();
            model.HeaderInfo.Dates = new List<string> { @event.StartDate.ToString(IntranetConstants.Common.DefaultDateTimeFormat), @event.EndDate.ToString(IntranetConstants.Common.DefaultDateTimeFormat) };
            model.EditPageUrl = _eventsService.GetEditPage().Url;
            model.OverviewPageUrl = _eventsService.GetOverviewPage().Url;
            model.CanEdit = _eventsService.CanEdit(@event);
            model.CanSubscribe = _eventsService.CanSubscribe(@event);

            return PartialView("~/App_Plugins/Events/Details/DetailsView.cshtml", model);
        }

        public ActionResult Create()
        {
            var model = new EventCreateModel
            {
                StartDate = DateTime.Now.Date.AddHours(8),
                EndDate = DateTime.Now.Date.AddHours(8),
                CanSubscribe = true
            };
            FillCreateEditData(model);
            return PartialView("~/App_Plugins/Events/Create/CreateView.cshtml", model);
        }

        [HttpPost]
        public ActionResult Create(EventCreateModel createModel)
        {
            if (!ModelState.IsValid)
            {
                FillCreateEditData(createModel);
                return PartialView("~/App_Plugins/Events/Create/CreateView.cshtml", createModel);
            }

            var @event = createModel.Map<Compent.uCommunity.Core.Events.Event>();
            @event.MediaIds = @event.MediaIds.Concat(_mediaHelper.CreateMedia(createModel));

            var activityId = _eventsService.Create(@event);

            return RedirectToUmbracoPage(_eventsService.GetDetailsPage(), new NameValueCollection { { "id", activityId.ToString() } });
        }

        public ActionResult Edit(Guid id)
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

            var model = @event.Map<EventEditModel>();
            model.CanEditSubscribe = _eventsService.CanEditSubscribe(@event.Id);
            FillCreateEditData(model);
            return PartialView("~/App_Plugins/Events/Edit/EditView.cshtml", model);
        }

        [HttpPost]
        public ActionResult Edit(EventEditModel saveModel)
        {
            if (!ModelState.IsValid)
            {
                FillCreateEditData(saveModel);
                return PartialView("~/App_Plugins/Events/Edit/EditView.cshtml", saveModel);
            }

            var @event = MapEditModel(saveModel);
            @event.MediaIds = @event.MediaIds.Concat(_mediaHelper.CreateMedia(saveModel));

            if (_eventsService.CanEditSubscribe(@event.Id))
            {
                @event.CanSubscribe = saveModel.CanSubscribe;
            }

            return RedirectToUmbracoPage(_eventsService.GetDetailsPage(), new NameValueCollection { { "id", @event.Id.ToString() } });
        }

        [HttpPost]
        public JsonResult Hide(Guid id)
        {
            _eventsService.Hide(id);

            return Json(new { _eventsService.GetOverviewPage().Url });
        }

        [HttpPost]
        public JsonResult HasConfirmation(EventEditModel model)
        {
            var @event = MapEditModel(model);
            return Json(new { HasConfirmation = _eventsService.IsActual(@event) && @event.Subscribers.Any() });
        }

        public ActionResult ItemView(Compent.uCommunity.Core.Events.EventOverviewItemModel model)
        {
            return PartialView("~/App_Plugins/Events/List/ItemView.cshtml", model);
        }

        public ActionResult CentralFeedItem(CentralFeed.ICentralFeedItem item)
        {
            FillLinks();
            var activity = item as Compent.uCommunity.Core.Events.Event;

            return PartialView("~/App_Plugins/Events/List/ItemView.cshtml", GetOverviewItems(Enumerable.Repeat(activity, 1)).Single());
        }

        private Compent.uCommunity.Core.Events.Event MapEditModel(EventEditModel saveModel)
        {
            var @event = _eventsService.Get(saveModel.Id);
            @event = Mapper.Map(saveModel, @event);
            return @event;
        }

        private void FillCreateEditData(IContentWithMediaCreateEditModel model)
        {
            FillLinks();

            var mediaSettings = _eventsService.GetMediaSettings();
            ViewData["AllowedMediaExtentions"] = mediaSettings.AllowedMediaExtentions;
            model.MediaRootId = mediaSettings.MediaRootId;
        }

        private void FillLinks()
        {
            ViewData["CreatePageUrl"] = _eventsService.GetCreatePage().Url;
            ViewData["DetailsPageUrl"] = _eventsService.GetDetailsPage().Url;
            ViewData["OverviewPageUrl"] = _eventsService.GetOverviewPage().Url;
        }

        private IEnumerable<Compent.uCommunity.Core.Events.EventOverviewItemModel> GetOverviewItems(IEnumerable<Compent.uCommunity.Core.Events.Event> events)
        {
            var detailsPageUrl = _eventsService.GetDetailsPage().Url;
            foreach (var @event in events)
            {
                var model = @event.Map<Compent.uCommunity.Core.Events.EventOverviewItemModel>();
                model.MediaIds = @event.MediaIds.Take(ImageConstants.DefaultActivityOverviewImagesCount).JoinToString(",");
                model.CanSubscribe = _eventsService.CanSubscribe(@event);

                model.HeaderInfo = @event.Map<IntranetActivityItemHeaderViewModel>();
                model.HeaderInfo.DetailsPageUrl = detailsPageUrl.UrlWithQueryString("id", @event.Id.ToString());

                yield return model;
            }
        }
    }
}