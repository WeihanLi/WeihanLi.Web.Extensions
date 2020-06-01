using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WeihanLi.Web.Authentication.QueryAuthentication
{
    public sealed class QueryAuthenticationOptions : AuthenticationSchemeOptions
    {
        private string _userRolesQueryKey = "UserRoles";
        private string _userNameQueryKey = "UserName";
        private string _userIdQueryKey = "UserId";
        private string _delimiter = ",";

        public string UserIdQueryKey
        {
            get => _userIdQueryKey;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _userIdQueryKey = value;
                }
            }
        }

        public string UserNameQueryKey
        {
            get => _userNameQueryKey;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _userNameQueryKey = value;
                }
            }
        }

        public string UserRolesQueryKey
        {
            get => _userRolesQueryKey;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _userRolesQueryKey = value;
                }
            }
        }

        /// <summary>
        /// 自定义其他的 header
        /// key: QueryKey
        /// value: claimType
        /// </summary>
        public Dictionary<string, string> AdditionalQueryToClaims { get; } = new Dictionary<string, string>();

        public string Delimiter
        {
            get => _delimiter;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _delimiter = value;
                }
            }
        }

        private Func<HttpContext, Task<bool>> _authenticationValidator = context =>
        {
            var userIdHeader = context.RequestServices.GetRequiredService<IOptions<QueryAuthenticationOptions>>().Value.UserIdQueryKey;
            return Task.FromResult(context.Request.Query.ContainsKey(userIdHeader));
        };

        public Func<HttpContext, Task<bool>> AuthenticationValidator
        {
            get => _authenticationValidator;
            set => _authenticationValidator = value ?? throw new ArgumentNullException(nameof(AuthenticationValidator));
        }
    }
}
