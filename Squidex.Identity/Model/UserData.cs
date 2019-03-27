﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Squidex.ClientLibrary;

namespace Squidex.Identity.Model
{
    public sealed class UserData
    {
        [JsonConverter(typeof(InvariantConverter))]
        public string Username { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string NormalizedUsername { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string Email { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string NormalizedEmail { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string SecurityStamp { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public string PasswordHash { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public bool EmailConfirmed { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public bool PhoneNumberConfirmed { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public bool LockoutEnabled { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public int AccessFailedCount { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public DateTimeOffset? LockoutEndDateUtc { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public HashSet<string> Roles { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public Dictionary<string, string> Claims { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public Dictionary<string, string> Tokens { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        public HashSet<string> LoginKeys { get; set; }

        [JsonConverter(typeof(InvariantConverter))]
        [JsonProperty("login")]
        public Dictionary<string, UserLogin> LoginsObj { get; set; }

        public void EnsureLogins()
        {
            if (LoginsObj == null)
            {
                LoginKeys = new HashSet<string>();
                LoginsObj = new Dictionary<string, UserLogin>();
            }
        }

        public void EnsureRoles()
        {
            if (Roles == null)
            {
                Roles = new HashSet<string>();
            }
        }

        public void EnsureTokens()
        {
            if (Tokens == null)
            {
                Tokens = new Dictionary<string, string>();
            }
        }

        public void EnsureClaims()
        {
            if (Claims == null)
            {
                Claims = new Dictionary<string, string>();
            }
        }

        public void AddRole(string roleName)
        {
            EnsureRoles();

            Roles.Add(roleName);
        }

        public void RemoveRole(string roleName)
        {
            EnsureRoles();

            Roles.Remove(roleName);
        }

        public IList<string> GetRoles()
        {
            EnsureRoles();

            return Roles.ToList();
        }

        public void AddLogin(UserLoginInfo login)
        {
            EnsureLogins();

            var key = LoginKey(login);

            LoginsObj[key] = login;
            LoginKeys.Add(key);
        }

        public void RemoveLogin(string loginProvider, string providerKey)
        {
            EnsureLogins();

            var key = LoginKey(loginProvider, providerKey);

            LoginsObj.Remove(key);
            LoginKeys.Remove(key);
        }

        public IList<UserLoginInfo> GetLogins()
        {
            EnsureLogins();

            return LoginsObj.Values.Select(x => x.ToInfo()).ToList();
        }

        public void AddClaims(IEnumerable<Claim> claims)
        {
            EnsureClaims();

            foreach (var claim in claims)
            {
                Claims[claim.Type] = claim.Value;
            }
        }

        public void RemoveClaims(IEnumerable<Claim> claims)
        {
            EnsureClaims();

            foreach (var claim in claims)
            {
                Claims.Remove(claim.Type);
            }
        }

        public void ReplaceClaim(Claim claim, Claim newClaim)
        {
            EnsureClaims();

            if (claim != null)
            {
                Claims.Remove(claim.Type);
            }

            if (newClaim != null)
            {
                Claims[claim.Type] = claim.Value;
            }
        }

        public IList<Claim> GetClaims()
        {
            EnsureClaims();

            return Claims.Select(x => new Claim(x.Key, x.Value)).ToList();
        }

        public void SetToken(string loginProvider, string name, string value)
        {
            EnsureTokens();

            Tokens[TokenKey(loginProvider, name)] = value;
        }

        public void RemoveToken(string loginProvider, string name)
        {
            EnsureTokens();

            Tokens.Remove(TokenKey(loginProvider, name));
        }

        public string GetToken(string loginProvider, string name)
        {
            EnsureTokens();

            return Tokens.GetValueOrDefault(TokenKey(loginProvider, name));
        }

        public static string LoginKey(UserLoginInfo info)
        {
            return LoginKey(info.LoginProvider, info.ProviderKey);
        }

        public static string LoginKey(string loginProvider, string providerKey)
        {
            return $"{loginProvider}_{providerKey}";
        }

        public static string TokenKey(string loginProvider, string name)
        {
            return $"{loginProvider}_{name}";
        }
    }
}