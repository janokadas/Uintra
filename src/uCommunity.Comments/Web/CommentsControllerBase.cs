﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using uCommunity.Comments.Core.Events;
using uCommunity.Core.Activity;
using uCommunity.Core.Extentions;
using uCommunity.Core.User;
using Umbraco.Web.Mvc;

namespace uCommunity.Comments.Web
{
    public abstract class CommentsControllerBase : SurfaceController
    {
        protected virtual string OverviewViewPath { get; } = "~/App_Plugins/Comments/View/CommentsOverView.cshtml";
        protected virtual string PreviewViewPath { get; } = "~/App_Plugins/Comments/View/CommentsPreView.cshtml";
        protected virtual string EditViewPath { get; } = "~/App_Plugins/Comments/View/CommentsEditView.cshtml";
        protected virtual string CreateViewPath { get; } = "~/App_Plugins/Comments/View/CommentsCreateView.cshtml";

        protected readonly ICommentsService CommentsService;
        protected readonly IIntranetUserService IntranetUserService;
        protected readonly IActivitiesServiceFactory ActivitiesServiceFactory;

        protected CommentsControllerBase(
            ICommentsService commentsService,
            IIntranetUserService intranetUserService,
            IActivitiesServiceFactory activitiesServiceFactory)
        {
            CommentsService = commentsService;
            IntranetUserService = intranetUserService;
            ActivitiesServiceFactory = activitiesServiceFactory;
        }

        [HttpPost]
        public virtual PartialViewResult Add(CommentCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return OverView(model.ActivityId);
            }
            var service = ActivitiesServiceFactory.GetService(model.ActivityId);
            var commentableService = (ICommentableService)service;
            var comment = commentableService.CreateComment(IntranetUserService.GetCurrentUser().Id, model.ActivityId, model.Text, model.ParentId);
            OnCommentCreated(new CommentCreated(comment.Id, comment.ActivityId, comment.ParentId));

            return OverView(model.ActivityId);
        }

        [HttpPut]
        public virtual PartialViewResult Edit(CommentEditModel model)
        {
            var comment = CommentsService.Get(model.Id);

            if (!ModelState.IsValid || !CommentsService.CanEdit(comment, IntranetUserService.GetCurrentUser().Id))
            {
                return OverView(model.Id);
            }

            var service = ActivitiesServiceFactory.GetService(comment.ActivityId);
            var commentableService = (ICommentableService)service;
            commentableService.UpdateComment(model.Id, model.Text);
            OnCommentEdited(new CommentEdited(comment.Id, comment.ActivityId));
            return OverView(comment.ActivityId);
        }

        [HttpDelete]
        public virtual PartialViewResult Delete(Guid id)
        {
            var comment = CommentsService.Get(id);
            var currentUserId = IntranetUserService.GetCurrentUser().Id;

            if (!CommentsService.CanDelete(comment, currentUserId))
            {
                return OverView(comment.ActivityId);
            }

            var service = ActivitiesServiceFactory.GetService(comment.ActivityId);
            var commentableService = (ICommentableService)service;
            commentableService.DeleteComment(id);

            return OverView(comment.ActivityId);
        }

        public virtual PartialViewResult CreateView(Guid activityId)
        {
            var model = new CommentCreateModel
            {
                ActivityId = activityId,
                UpdateElementId = GetOverviewElementId(activityId)
            };
            return PartialView(CreateViewPath, model);
        }

        public virtual PartialViewResult EditView(Guid id, string updateElementId)
        {
            var comment = CommentsService.Get(id);
            var model = new CommentEditModel
            {
                Id = id,
                Text = comment.Text,
                UpdateElementId = updateElementId
            };
            return PartialView(EditViewPath, model);
        }

        public virtual PartialViewResult OverView(ICommentable commentsInfo)
        {
            return OverView(commentsInfo.Id, commentsInfo.Comments);
        }

        public virtual PartialViewResult PreView(Guid activityId, string link)
        {
            var model = new CommentPreviewModel
            {
                Count = CommentsService.GetCount(activityId),
                Link = $"{link}#{GetOverviewElementId(activityId)}"
            };
            return PartialView(PreviewViewPath, model);
        }

        protected virtual void OnCommentCreated(Comment comment)
        {

        }

        protected virtual void OnCommentEdited(Comment comment)
        {
        }

        protected virtual PartialViewResult OverView(Guid activityId)
        {
            return OverView(activityId, CommentsService.GetMany(activityId));
        }

        protected virtual PartialViewResult OverView(Guid activityId, IEnumerable<Comment> comments)
        {
            var model = new CommentsOverviewModel
            {
                ActivityId = activityId,
                Comments = GetCommentViews(comments),
                ElementId = GetOverviewElementId(activityId)
            };

            return PartialView(OverviewViewPath, model);
        }

        protected virtual IEnumerable<CommentViewModel> GetCommentViews(IEnumerable<Comment> comments)
        {
            comments = comments.OrderBy(c => c.CreatedDate);
            var commentsList = comments as List<Comment> ?? comments.ToList();
            var currentUserId = IntranetUserService.GetCurrentUser().Id;
            var creators = IntranetUserService.GetAll().ToList();
            var replies = commentsList.FindAll(CommentsService.IsReply);

            foreach (var comment in commentsList.FindAll(c => !CommentsService.IsReply(c)))
            {
                var model = GetCommentView(comment, currentUserId, creators.SingleOrDefault(c => c.Id == comment.UserId));
                var commentReplies = replies.FindAll(reply => reply.ParentId == model.Id);
                model.Replies = commentReplies.Select(reply => GetCommentView(reply, currentUserId, creators.SingleOrDefault(c => c.Id == reply.UserId)));
                yield return model;
            }
        }

        protected virtual CommentViewModel GetCommentView(Comment comment, Guid currentUserId, IIntranetUser creator)
        {
            var model = comment.Map<CommentViewModel>();
            model.ModifyDate = CommentsService.WasChanged(comment) ? comment.ModifyDate : default(DateTime?);
            model.CanEdit = CommentsService.CanEdit(comment, currentUserId);
            model.CanDelete = CommentsService.CanDelete(comment, currentUserId);
            model.CreatorFullName = creator?.DisplayedName;
            model.Photo = creator?.Photo;
            model.ElementOverviewId = GetOverviewElementId(comment.ActivityId);
            model.CommentViewId = CommentsService.GetCommentViewId(comment.Id);
            return model;
        }

        protected virtual string GetOverviewElementId(Guid activityId)
        {
            return $"js-comments-overview-{activityId}";
        }
    }
}
