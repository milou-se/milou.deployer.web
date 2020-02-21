using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Milou.Deployer.Web.Tools
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            List<string> usedArgs = args.ToList();

            if (usedArgs.Count == 0)
            {
                while (true)
                {
                    Console.WriteLine("Enter arg");

                    string arg = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        break;
                    }

                    usedArgs.Add(arg);
                }
            }

            int expected = 1;

            if (usedArgs.Count == expected)
            {
                using var hmac = new HMACSHA256();
                var keyBytes = hmac.Key;
                string key = Convert.ToBase64String(keyBytes);
                string agentId = usedArgs[0];

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, agentId),
                    new Claim(ClaimTypes.Name, agentId),
                };

                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(keyBytes);
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = new DateTime(DateTime.Today.Year+2, 12,31,0,0,0,0),
                    SigningCredentials = new SigningCredentials(securityKey,
                        SecurityAlgorithms.HmacSha256Signature)
                    //SigningCredentials =  new SigningCredentials(, )
                };

                IdentityModelEventSource.ShowPII = true;

                var securityToken = handler.CreateJwtSecurityToken(tokenDescriptor);
                string jwt = handler.WriteToken(securityToken);
                Console.WriteLine(key);
                Console.WriteLine(jwt);
            }
            else
            {
                Console.WriteLine($"Invalid args, got {usedArgs.Count}, expected {expected}");
            }
        }
    }
}