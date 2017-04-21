using System;
using System.Collections.Generic;
using System.Web.Http;
using uCommunity.Core.Extentions;
using Umbraco.Web.WebApi;

namespace uCommunity.News.Dashboard
{
    public class NewsSectionController : UmbracoAuthorizedApiController
    {
        private readonly INewsService _newsService;

        public NewsSectionController(INewsService newsService)
        {
            _newsService = newsService;
        }

        public IEnumerable<NewsBackofficeViewModel> GetAll()
        {
            var news = _newsService.GetAll<NewsBase>(true);
            var result = news.Map<IEnumerable<NewsBackofficeViewModel>>();
            return result;
        }

        [HttpPost]
        public NewsBackofficeViewModel Create(NewsBackofficeCreateModel createModel)
        {
            var newsId = _newsService.Create(createModel.Map<NewsBase>());
            var createdModel = _newsService.Get<NewsBase>(newsId);
            var result = createdModel.Map<NewsBackofficeViewModel>();
            return result;
        }

        [HttpPost]
        public NewsBackofficeViewModel Save(NewsBackofficeSaveModel saveModel)
        {
            _newsService.Save(saveModel.Map<NewsBase>());
            var updatedModel = _newsService.Get<NewsBase>(saveModel.Id);
            var result = updatedModel.Map<NewsBackofficeViewModel>();
            return result;
        }

        [HttpDelete]
        public void Delete(Guid id)
        {
            _newsService.Delete(id);
        }
    }
}