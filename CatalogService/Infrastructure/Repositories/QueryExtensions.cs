using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CatalogService.Infrastructure.Repositories
{
    internal static class QueryExtensions
    {
        private static readonly string[] CandidateNames = new[] { "Name", "Description", "CompanyName", "ContactName", "Code" };

        public static IQueryable<T> OrderByNameOrDescription<T>(this IQueryable<T> query)
        {
            var type = typeof(T);

            var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => CandidateNames.Any(c => string.Equals(c, p.Name, StringComparison.OrdinalIgnoreCase))
                                     && p.PropertyType == typeof(string));

            if (prop == null)
                return query;

            var param = Expression.Parameter(type, "x");
            var efPropertyMethod = typeof(EF).GetMethod(nameof(EF.Property), BindingFlags.Public | BindingFlags.Static)?
                .MakeGenericMethod(typeof(string));
            if (efPropertyMethod == null)
                return query;

            var call = Expression.Call(null, efPropertyMethod, param, Expression.Constant(prop.Name));
            var lambda = Expression.Lambda<Func<T, string>>(call, param);

            return query.OrderBy(lambda);
        }
    }
}
