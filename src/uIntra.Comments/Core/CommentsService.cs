﻿using System;
using System.Collections.Generic;
using uIntra.Core.Extensions;
using uIntra.Core.Persistence;

namespace uIntra.Comments
{
    public class CommentsService : ICommentsService
    {
        private readonly ISqlRepository<Guid, Comment> _commentsRepository;

        public CommentsService(ISqlRepository<Guid, Comment> commentsRepository)
        {
            _commentsRepository = commentsRepository;
        }

        public virtual CommentModel Get(Guid id)
        {
            return _commentsRepository.Get(id).Map<CommentModel>();
        }

        public virtual IEnumerable<CommentModel> GetMany(Guid activityId)
        {
            return _commentsRepository
                .FindAll(comment => comment.ActivityId == activityId)
                .Map<IEnumerable<CommentModel>>();
        }

        public virtual int GetCount(Guid activityId)
        {
            var count = _commentsRepository.Count(comment => comment.ActivityId == activityId);
            return (int)count;
        }

        public virtual bool CanEdit(CommentModel comment, Guid editorId)
        {
            return comment.UserId == editorId;
        }

        public virtual bool CanDelete(CommentModel comment, Guid editorId)
        {
            return comment.UserId == editorId;
        }

        public virtual bool WasChanged(CommentModel comment)
        {
            return comment.CreatedDate != comment.ModifyDate;
        }

        public virtual bool IsReply(CommentModel comment)
        {
            return comment.ParentId.HasValue;
        }

        public virtual CommentModel Create(Guid userId, Guid activityId, string text, Guid? parentId)
        {
            var entity = new Comment
            {
                Id = Guid.NewGuid(),
                ActivityId = activityId,
                UserId = userId,
                Text = text,
                ParentId = parentId
            };

            entity.CreatedDate = entity.ModifyDate = DateTime.Now.ToUniversalTime();
            _commentsRepository.Add(entity);
            return entity.Map<CommentModel>();
        }

        public virtual CommentModel Update(Guid id, string text)
        {
            var comment = _commentsRepository.Get(id);

            comment.ModifyDate = DateTime.Now.ToUniversalTime();
            comment.Text = text;
            _commentsRepository.Update(comment);
            return comment.Map<CommentModel>();
        }

        public virtual void Delete(Guid id)
        {
            var comment = _commentsRepository.Get(id);

            if (comment.ParentId == null)
            {
                var replies = _commentsRepository.FindAll(c => c.ParentId == id);
                _commentsRepository.Delete(replies);
            }

            _commentsRepository.Delete(comment);
        }

        public virtual void FillComments(ICommentable entity)
        {
            var comments = GetMany(entity.Id);
            entity.Comments = comments;
        }

        public virtual string GetCommentViewId(Guid commentId)
        {
            return $"js-comment-view-{commentId}";
        }

        public bool IsExistsUserComment(Guid activityId, Guid userId)
        {
            var result = _commentsRepository.Exists(c => c.ActivityId == activityId && c.UserId == userId);

            return result;

        }
    }
}