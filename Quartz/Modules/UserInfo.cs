using System.Web;
using System.Web.Security;

namespace Quartz.Presentation.Modules
{
    public class UserInfo
    {
        public static MembershipUser GetUserInfo()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null &&
                HttpContext.Current.User.Identity != null &&
                !string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                return Membership.GetUser(HttpContext.Current.User.Identity.Name.Split('\\')[1]);

            return Membership.GetUser();
        }

        public static string GetMachineIP()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
    }
}