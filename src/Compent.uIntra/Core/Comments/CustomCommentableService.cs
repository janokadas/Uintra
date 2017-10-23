﻿using System;
using uIntra.Comments;
using uIntra.Core.TypeProviders;

namespace Compent.uIntra.Core.Comments
{
    public class CustomCommentableService : ICustomCommentableService
    {
        // TODO: remove this contract after decoupling comments from activities
        public IIntranetType ActivityType { get; } = GetDummyType();

        private static IIntranetType GetDummyType()
        {
            return new IntranetType
            {
                Id = Int32.MaxValue,
                Name = typeof(CustomCommentableService).Name
            };
        }

        private readonly ICommentsService _commentsService;

        public CustomCommentableService(ICommentsService commentsService)
        {
            _commentsService = commentsService;
        }

        public Comment CreateComment(Guid userId, Guid activityId, string text, Guid? parentId)
        {
            var comment = _commentsService.Create(userId, activityId, text, parentId);
            return comment;
        }

        public void UpdateComment(Guid id, string text)
        {
            _commentsService.Update(id, text);
        }

        public void DeleteComment(Guid id)
        {
            _commentsService.Delete(id);
        }

        public ICommentable GetCommentsInfo(Guid activityId)
        {
            var result = new CustomCommentable { Id = activityId };
            _commentsService.FillComments(result);

            return result;
        }
    }
}