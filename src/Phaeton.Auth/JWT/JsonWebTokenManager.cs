﻿using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Phaeton.Auth;
using Phaeton.Auth.JWT.Abstractions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;

namespace Phaeton.Auth.JWT;

internal sealed class JsonWebTokenManager : IJsonWebTokenManager
{
    private static readonly Dictionary<string, IEnumerable<string>> EmptyClaims = new();
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly string? _issuer;
    private readonly TimeSpan _expiry;
    private readonly SigningCredentials _signingCredentials;
    private readonly string? _audience;

    public JsonWebTokenManager(
        IOptions<AuthOptions> options,
        SecurityKeyDetails securityKeyDetails
    )
    {
        if (options.Value?.Jwt is null)
        {
            throw new InvalidOperationException("Missing JWT options.");
        }

        _audience = options.Value.Jwt.Audience;
        _issuer = options.Value.Jwt.Issuer;
        _expiry = options.Value.Jwt.Expiry ?? TimeSpan.FromHours(1);
        _signingCredentials = new SigningCredentials(
            securityKeyDetails.Key,
            securityKeyDetails.Algorithm
        );
    }

    public JsonWebToken CreateToken(
        long userId,
        string? email = null,
        string? role = null,
        IDictionary<string, IEnumerable<string>>? claims = null
    ) 
    {
        var now = DateTime.UtcNow;

        var jwtClaims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub, 
                userId.ToString()
            ),
            new(
                JwtRegisteredClaimNames.UniqueName, 
                userId.ToString()
            )
        };
        if (!string.IsNullOrWhiteSpace(email))
            jwtClaims.Add(new Claim(
                JwtRegisteredClaimNames.Email,
                email
            ));

        if (!string.IsNullOrWhiteSpace(role))
            jwtClaims.Add(new Claim(
                ClaimTypes.Role,
                role
            ));


        if (!string.IsNullOrWhiteSpace(_audience))
            jwtClaims.Add(new Claim(
                JwtRegisteredClaimNames.Aud, 
                _audience
            ));

        if (claims?.Any() is true)
        {
            var customClaims = new List<Claim>();
            foreach (var (claim, values) in claims)
            {
                customClaims.AddRange(values.Select(q => new Claim(
                    claim, 
                    q
                )));
            }

            jwtClaims.AddRange(customClaims);
        }

        var expires = now.Add(_expiry);

        var jwt = new JwtSecurityToken(
            _issuer,
            claims: jwtClaims,
            notBefore: now,
            expires: expires,
            signingCredentials: _signingCredentials
        );

        var token = _jwtSecurityTokenHandler.WriteToken(jwt);

        return new JsonWebToken
        {
            AccessToken = token,
            Expiry = new DateTimeOffset(expires).ToUnixTimeMilliseconds(),
            UserId = userId,
            Email = email ?? string.Empty,
            Role = role ?? string.Empty,
            Claims = claims ?? EmptyClaims
        };
    }
}