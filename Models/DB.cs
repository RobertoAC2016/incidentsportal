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
                CON = getcon();
                var SQL = "select a.idAtencion from Atencion a inner join UsuariosTickets b on a.idAtencion = b.idusuarios Where " +
                    $"b.usuario = '{usr.usuario}';";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    status = "duplicate";
                }
                else
                {
                    SQL = $"insert into Atencion (Empleado, Departamento) values ('{usr.empleado}', '{usr.departamento}');" +
                        "select SCOPE_IDENTITY();";
                    da = new SqlDataAdapter(SQL, CON);
                    dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count <= 0)
                    {
                        status = "account";
                    }
                    else
                    {
                        var ida = dt.Rows[0][0];
                        //var skey = SEC.Encrypt(usr.password);
                        SQL = "SET IDENTITY_INSERT UsuariosTickets ON;" +
                            "INSERT into UsuariosTickets ([idUsuarios], [Usuario], [Contraseña], [Rol]) VALUES " +
                            $"({ida}, '{usr.usuario}', '{usr.contrasena}', '{usr.rol}'); " +
                            "SET IDENTITY_INSERT UsuariosTickets OFF; " +
                            "insert into Horarios (idAtencion, entrada, salida, comida_in, comida_out, estatus) values " +
                            $"({ida}, '{usr.entrada.ToString().Replace("T", " ")}', '{usr.salida.ToString().Replace("T", " ")}', " +
                            $"'{usr.entrada_comida.ToString().Replace("T", " ")}', '{usr.salida_comida.ToString().Replace("T", " ")}', 'activo');" +
                            $"select SCOPE_IDENTITY();";
                        da = new SqlDataAdapter(SQL, CON);
                        dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count <= 0)
                        {
                            status = "privileges";
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
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" and id = {filterbyid}";
                var SQL = $"select name from areas where status = 'active'{fil} order by name;";
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
                var SQL = $"select top 1 idusuarios from UsuariosTickets where usuario = '{usr.username}';";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    var ida = $"{dt.Rows[0][0]}";
                    var skey = usr.password;
                    SQL = "select a.Empleado, a.Departamento, b.rol from Atencion a inner join " +
                        "UsuariosTickets b on a.idAtencion = b.idusuarios where " +
                        $"b.usuario = '{usr.username}' and b.Contraseña = '{skey}';";
                    da = new SqlDataAdapter(SQL, CON); 
                    dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        obj.autenticated = true;
                        obj.message = "";
                        obj.login = usr.username;
                        obj.name = $"{dt.Rows[0][0]}";
                        obj.role = $"{dt.Rows[0][2]}";
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
        public List<String> get_roles(String filterbyid = "", String constr = "")
        {
            List<String> rols = new List<String>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" and id = {filterbyid}";
                var SQL = $"select name from rols where status = 'active'{fil} order by name;";
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
        public List<registro> get_users(String filterbyid = "")
        {
            List<registro> users = new List<registro>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" where a.idAtencion = {filterbyid}";
                var SQL = "select a.idAtencion, a.Empleado, b.usuario, b.Contraseña, a.Departamento, b.rol from Atencion a inner join " +
                    $"UsuariosTickets b on a.idAtencion = b.idusuarios{fil};";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        users.Add(new registro
                        {
                            idatencion = int.Parse($"{dr[0]}"),
                            empleado = $"{dr[1]}",
                            usuario = $"{dr[2]}",
                            contrasena = $"{dr[3]}",
                            departamento = $"{dr[4]}",
                            rol = $"{dr[5]}",
                            departamentos = get_departments(),
                            roles = get_roles()
                        });
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