// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UT4MasterServer.Authorization;
using System.Security.Claims;

namespace UT4MasterServer.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,
		UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
	{
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		return await Task.Run(() =>
		{
			var authorizationHeader = Request.Headers["Authorization"];
			var authorization = new HttpAuthorization(authorizationHeader);
			if (!authorization.IsBasic)
			{
				const string errMessage = $"trying to handle a scheme that is not 'basic' inside basic scheme handler";
				Logger.LogInformation(errMessage);
				return AuthenticateResult.Fail(errMessage);
			}

			var client = new ClientIdentification(authorization.Value);
			if (client.ID.IsEmpty || client.Secret.IsEmpty)
			{
				const string errMessage = $"unexpected value in authorization header";
				Logger.LogInformation(errMessage);
				return AuthenticateResult.Fail(errMessage);
			}

			// TODO: we could verify that request is coming from a valid client.
			//       that means either ClientIdentification.Game, ClientIdentification.Launcher
			//       or a new (ID:Secret) pair created just for our website.

			var principal = new ClaimsPrincipal(new EpicClientIdentity(client));
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return AuthenticateResult.Success(ticket);
		});
	}

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		// when the scheme is not present, return 401 Unauthorized
		Response.StatusCode = 401;
		return Task.CompletedTask;
	}
}