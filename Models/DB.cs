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
                    SQL = "insert into accounts (name, lastname, email, phone, department, status, creation) values " +
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
                            $"({ida}, '{usr.username}', '{skey}', GETDATE(), 1);select SCOPE_IDENTITY();";
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
        public List<department> get_departments()
        {
            List<department> deps = new List<department>();
            try
            {
                var SQL = "select name from areas where status = 'active' order by name;";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        deps.Add(new department { name = $"{dr[0]}" });
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
                            obj.access = get_roles(ida);
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

        private List<string> get_roles(String ida)
        {
            List<String> rols = new List<String>();
            try
            {
                var SQL = $"select b.name from rols_account a inner join rols b on a.idr = b.id and b.status = 'active' where a.ida = {ida};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
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
    }
}