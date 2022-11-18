using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace incidents.Models
{
    public class department
    {
        public int? id { get; set; }
        public string name { get; set; } = "";
        public string? status { get; set; }
    }
    public class registro
    {
        public int? idatencion { get; set; }
        public string? empleado { get; set; }
        public string? usuario { get; set; }
        public string? contrasena { get; set; }
        public string? departamento { get; set; }
        public string? rol { get; set; }
        public string? phone { get; set; }
        public string? email { get; set; }
        public List<String>? departamentos { get; set; }
        public List<String>? roles { get; set; }
        public string? entrada { get; set; }
        public string? salida { get; set; }
        public string? entrada_comida { get; set; }
        public string? salida_comida { get; set; }
    }
    public class login_request
    {
        //[Required(ErrorMessage = "Username is required")]
        public string username { get; set; } = "";
        //[Required(ErrorMessage = "Password is required")]
        public string password { get; set; } = "";
        public string message { get; set; } = "";
    }
    public class response_sql
    {
        public string valor { get; set; }
        public string message { get; set; }
    }
    public class login_validation : response_sql
    {
        public String login { get; set; } = "";
        public String token { get; set; } = "";
        public String name { get; set; } = "";
        public String role { get; set; }
        public bool autenticated { get; set; }
    }
    public class incident
    {
        public int id { get; set; }
        public String from { get; set; }
        public String to { get; set; }
        public String importance { get; set; }
        public String subject { get; set; }
        public String message { get; set; }
        public String path { get; set; }
        public byte[] base64 { get; set; }
        public DateTime date { get; set; }
        public String status { get; set; } = "active";
    }
}
