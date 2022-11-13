using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace incidents.Models
{
    public class DB
    {
        private security SEC = new security();
        private IConfiguration conf;
        private SqlConnection CON;
        public DB(){}
        public DB(IConfiguration _conf)
        {
            conf = _conf;
        }
        private SqlConnection getcon()
        {
            var constr = conf["constr"];
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            return con;
        }
        private SqlConnection getcon(String constr = "")
        {
            SqlConnection con = new SqlConnection(constr);
            con.Open();
            return con;
        }
        public string Save_New_User(registro usr)
        {
            var status = "ok";
            try
            {
                var SQL = "select top 1 a.id from accounts a, privileges b where a.id = b.ida and " +
                    $"(a.email = '{usr.email}' or b.username = '{usr.username}');";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    status = "duplicate";
                }
                else
                {
                    SQL = "insert into UsusariosTickets (name, lastname, email, phone, department, status, creation) values " +
                    $"('{usr.name}','{usr.lastname}','{usr.email}','{usr.phone}', '{usr.department}',1, GETDATE());select SCOPE_IDENTITY();";
                    da = new SqlDataAdapter(SQL, getcon());
                    dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count <= 0)
                    {
                        status = "account";
                    }
                    else
                    {
                        var ida = dt.Rows[0][0];
                        var skey = SEC.Encrypt(usr.password);
                        SQL = "insert into privileges (ida, username, password, creation, status) values " +
                            $"({ida}, '{usr.username}', '{skey}', GETDATE(), 1);" +
                            $"insert into Atencion (Empleado, Departamento) values ();" +
                            $"insert into Horarios (entrada, salida, comida_in, comida_out) values ();" +
                            $"select SCOPE_IDENTITY();";
                        da = new SqlDataAdapter(SQL, getcon());
                        dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count <= 0)
                        {
                            status = "privileges";
                        }
                        else
                        {//rols_account
                            SQL = "insert into rols_account (ida, idr, creation) values " +
                                $"({ida}, 1, GETDATE());select SCOPE_IDENTITY();";
                            da = new SqlDataAdapter(SQL, getcon());
                            dt = new DataTable();
                            da.Fill(dt);
                            if (dt.Rows.Count <= 0)
                            {
                                status = "privileges";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                status = "error";
            }
            return status;
        }
        public List<String> get_departments(String filterbyid = "")
        {
            List<String> deps = new List<String>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" where a.ida = {filterbyid} order by b.name";
                var SQL = $"select b.name from rols_account a inner join rols b on a.idr = b.id and b.status = 'active'{fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        deps.Add($"{dr[0]}");
                }
            }
            catch (Exception ex)
            {
            }
            return deps;
        }
        public login_validation validate_credentials(login_request usr)
        {
            login_validation obj = new login_validation();
            try
            {
                CON = getcon();
                var SQL = $"select top 1 id from privileges where username = '{usr.username}';";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    SQL = $"select top 1 id from privileges where username = '{usr.username}' and status = 1;";
                    da = new SqlDataAdapter(SQL, CON); dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        var ida = $"{dt.Rows[0][0]}";
                        var skey = SEC.Encrypt(usr.password);
                        SQL = "select top 1 a.name, a.lastname from accounts a, privileges b where a.id = b.ida and " +
                            $"b.username = '{usr.username}' and b.password = '{skey}';";
                        da = new SqlDataAdapter(SQL, CON); dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                        {   
                            var tkn = conf.GetSection("skey").Value;
                            obj.token = SEC.CreateToken(usr.username, tkn);
                            obj.autenticated = true;
                            obj.message = "";
                            obj.login = usr.username;
                            obj.name = $"{dt.Rows[0][0]} {dt.Rows[0][1]}";
                            obj.role = get_user_role(ida);
                        }
                        else
                        {
                            obj.autenticated = false;
                            obj.message = "Error invalid password";
                        }
                    }
                    else
                    {
                        obj.autenticated = false;
                        obj.message = "Error user inactive";
                    }
                }
                else
                {
                    obj.autenticated = false;
                    obj.message = "Error user not exist";
                }
            }
            catch (Exception ex)
            {
                obj.autenticated = false;
                obj.message = "Error something was wrong";
            }
            return obj;
        }
        public List<String> get_roles(String ida = "", String constr = "")
        {
            List<String> rols = new List<String>();
            try
            {
                var fil = string.IsNullOrEmpty(ida) ? "" : $" where a.id = {ida}";
                var SQL = $"select b.name from rols_account a inner join rols b on a.idr = b.id and b.status = 'active'{fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, string.IsNullOrEmpty(constr) ? getcon() : getcon(constr));
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        rols.Add($"{dr[0]}");
                }
            }
            catch (Exception ex)
            {
            }
            return rols;
        }
        public List<incident> get_incidents(String filterbyid = "")
        {
            List<incident> tts = new List<incident>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" where id = {filterbyid}";
                var SQL = $"select * from Ticket{fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        tts.Add(new incident {
                            id = int.Parse($"{dr[0]}"),
                            creation = DateTime.Parse($"{dr[1]}"),
                            status = "active"
                        });
                }
            }
            catch (Exception ex)
            {
            }
            return tts;
        }
        public List<user_register> get_users(String filterbyid = "")
        {
            List<user_register> users = new List<user_register>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" where a.id = {filterbyid}";
                var SQL = "select a.id, a.name, a.lastname, a.phone, a.email, a.department, b.username, b.password from accounts a inner join privileges b " +
                    $"on a.id = b.ida {fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        users.Add(new user_register
                        {
                            id = int.Parse($"{dr[0]}"),
                            name = $"{dr[1]}",
                            lastname = $"{dr[2]}",
                            phone = $"{dr[3]}",
                            email = $"{dr[4]}",
                            username = $"{dr[6]}",
                            department = $"{dr[5]}",
                            roles = String.Join(",", get_roles($"{dr[0]}"))
                        }); ;
                }
            }
            catch (Exception ex)
            {
            }
            return users;
        }
        public string get_user_role(String ida = "", String constr = "")
        {
            String role = "";
            try
            {
                var fil = string.IsNullOrEmpty(ida) ? "" : $" where a.id = {ida}";
                var SQL = $"select b.name from rols_account a inner join rols b on a.idr = b.id and b.status = 'active'{fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, string.IsNullOrEmpty(constr) ? getcon() : getcon(constr));
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    role = $"{dt.Rows[0][0]}";
                }
            }
            catch (Exception ex)
            {
            }
            return role;
        }
    }
}