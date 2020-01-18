using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Dto;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.QueryExtensions
{
    public static class UserQueries
    {
        public static Expression<Func<User, bool>> ById(Guid id)
        {
            return u => u.Id == id;
        }
        
        public static Expression<Func<User, bool>> ByLogin(string login)
        {
            return u => u.Login == login;
        }
        
        public static Expression<Func<User, bool>> IsExisted(User entity)
        {
            return x => x.Id == entity.Id || x.Login.ToLower() == entity.Login.ToLower();
        }
        
        public static List<Expression<Func<User, bool>>> ByFilter(UserFilterDto filter)
        {
            var expressions = new List<Expression<Func<User, bool>>>();
            
            if (filter.Roles != null && filter.Roles.Any())
                expressions.Add(x => filter.Roles.Contains(x.Role.Title.ToLower()));
            
            return expressions;
        }
         
        public static Expression<Func<User, UserItemDto>> AsUserItemDtoProjection()
        {
            return u => new UserItemDto()
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email,
                Password = u.PasswordHash,
                Login = u.Login,
                Phone = u.Phone,
                RoleId = u.Role.Id,
                Role = u.Role.Title,
                PhotoBase64 = u.Photo == null ? null : u.Photo.ContentBase64,
                PhotoMimeType = u.Photo == null ? null : u.Photo.MimeType,
                IsActive = u.IsActive
            };
        }
        
        public static Expression<Func<User, UserShortInfoItemDto>> AsUserShortInfoItemDtoProjection()
        {
            return u => new UserShortInfoItemDto()
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Password = u.PasswordHash,
                Login = u.Login,
                Phone = u.Phone,
                RoleId = u.Role.Id,
                Role = u.Role.Title,
                IsActive = u.IsActive
            };
        }
    }
}