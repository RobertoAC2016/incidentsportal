using incidents.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace incidents.Controllers
{
    public class DB
    {
        private IConfiguration conf;
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
        public String Save_New_User(registro usr)
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
                        SQL = "insert into privileges (ida, username, password, creation, status) values " +
                            $"({ida}, '{usr.username}', '{usr.password}', GETDATE(), 1);select SCOPE_IDENTITY();";
                        da = new SqlDataAdapter(SQL, getcon());
                        dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count <= 0)
                        {
                            status = "privileges";
                        }
                        else
                        {
                            //aqui se insertara la seccion de accesos
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
    }
}