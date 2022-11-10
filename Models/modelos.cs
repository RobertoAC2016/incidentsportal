using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace incidents.Models
{
    public class register_user
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = "";
        public string lastname { get; set; } = "";
        public string email { get; set; } = "";
        public string phone { get; set; } = "";
        public string department { get; set; } = "";
        public string status { get; set; } = "new";
    }
    public class department
    {
        public int? id { get; set; }
        public string name { get; set; } = "";
        public string? status { get; set; }
    }
    public class registro
    {
        public string name { get; set; } = "";
        public string lastname { get; set; } = "";
        public string department { get; set; } = "";
        public string phone { get; set; } = "";
        public string email { get; set; } = "";
        public string username { get; set; } = "";
        public string password { get; set; } = "";
        public List<department> departments { get; set; }
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
        public string login { get; set; } = "";
        public string token { get; set; } = "";
        public string name { get; set; } = "";
        public List<String> access { get; set; }
        public bool autenticated { get; set; }
    }
}
