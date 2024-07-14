using Day1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Day1.Authorization
{
    public class PermissionBasedAuthorizationFilter(ApplicationDbContext dbContext) : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var attribute = (CheckPermissionAttribute)context.ActionDescriptor.EndpointMetadata.FirstOrDefault(x => x is CheckPermissionAttribute);
            if(attribute != null)
            {
                var claimIdentity = context.HttpContext.User.Identity as ClaimsIdentity; 
                if(claimIdentity == null || !claimIdentity.IsAuthenticated)
                {
                    context.Result = new ForbidResult();
                }
                else{
                    var userid = int.Parse(claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value);
                    var hasPermission = dbContext.Set<UserPermission>().Any(x => x.UserID == userid && x.PermissionID == attribute.Permission);
                    if(!hasPermission)
                    {
                        context.Result = new ForbidResult();
                    }
                }
            }
        }
    }
}
