using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace incidents.Services
{
    public class PermisosRol: ActionFilterAttribute
    {
        private String Rol;
        public PermisosRol(String _rol)
        {   
            Rol = _rol;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("username") != null)
            {
                var role_user = context.HttpContext.Session.GetString("role");
                var existe = this.Rol.Split(',').Where(x => x.Equals(role_user)).FirstOrDefault();
                if (string.IsNullOrEmpty(existe)) context.Result = new RedirectResult("~/Access/Index");
            }
            else
                context.Result = new RedirectResult("~/Login/Index");
            base.OnActionExecuting(context);
        }
    }
}
