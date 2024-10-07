using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using DotnetSpider.Infrastructure;

namespace DotnetSpider.Portal.Common
{
    public static class MapperExtensions
    {
        public static PagedResult<T2> ToPagedQueryResult<T1, T2>(this IMapper mapper, PagedResult<T1> source)
        {
            // return new(source.Page, source.Limit, source.Count, mapper.Map<List<T2>>(source.Data));
            return new PagedResult<T2>()
            {
                CurrentPage = source.CurrentPage,
                PageSize = source.PageSize,
                PageCount = source.PageCount,
                RowCount = source.RowCount,
                Queryable = source.Queryable
                    .AsEnumerable()
                    .Select(x => mapper.Map<T2>(x))
                    .AsQueryable()
            };
        }

        public static PagedResult<T> ToPagedResult<T>(this IEnumerable<T> self, int page, int limit, int total)
        {
            return new PagedResult<T>
            {
                Queryable = self.AsQueryable(),
                RowCount = self.Count(),
                CurrentPage = page,
                PageSize = limit,
                PageCount = total
            };
        }
    }
}
