using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.VisualBasic;
using MyFace.Models.Database;

namespace MyFace.Authorization
{
    public class UserAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, User>
    {
        private UserManager<IdentityUser> _userManager;

        public UserAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task 
            HandleRequirementAsync(AuthorizationHandlerContext context, 
                OperationAuthorizationRequirement requirement, 
                User resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }

            if (resource.Id.ToString() == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }
            
            return Task.CompletedTask;
        }
    }
}