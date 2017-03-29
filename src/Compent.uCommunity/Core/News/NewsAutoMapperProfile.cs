﻿using AutoMapper;
using Compent.uCommunity.Core.News.Models;
using uCommunity.News;

namespace Compent.uCommunity.Core.News
{
    public class NewsAutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<Entities.News, NewsViewModel>()
                .IncludeBase<NewsModelBase, NewsViewModel>()
                .ForMember(dst => dst.LikesInfo, o => o.MapFrom(el => el))
                .ForMember(dst => dst.CommentsInfo, o => o.MapFrom(el => el));

            Mapper.CreateMap<Entities.News, NewsOverviewItemModel>()
                .IncludeBase<NewsModelBase, NewsOverviewItemModel>()
                .ForMember(dst => dst.LikesInfo, o => o.MapFrom(el => el));
        }
    }
}