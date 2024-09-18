using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.EFCore
{
    public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomAuthorizationPolicyProvider(
            IOptions<AuthorizationOptions> options,
            IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Scoped servisi dinamik olarak çözümle
            /*var customAuthorizationService = */
            using (var scope = _serviceProvider.CreateScope())
            {
                var t = scope.ServiceProvider.GetRequiredService<ICustomAuthorizationService>();
                var requirements = await t.GetCustomRequirementsAsync();
                var requirement = requirements.FirstOrDefault(r => r.ClaimValue == policyName);

                if (requirement != null)
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(requirement)
                        .Build();
                    return policy;
                }
            }
            return await base.GetPolicyAsync(policyName);
        }
    }

}
