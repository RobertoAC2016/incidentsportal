﻿using NuGet.Protocol.Plugins;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.Intrinsics.X86;

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
                            $"({ida}, '1900-01-01 {usr.entrada}', '1900-01-01 {usr.salida}', " +
                            $"'1900-01-01 {usr.entrada_comida}', '1900-01-01 {usr.salida_comida}', 'activo');" +
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
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" and idticket = {filterbyid}";
                var SQL = $"select idticket, [Date], [from], [to], importance, subject, message, [Path Attachment], [Attachment To Base 64] " +
                    $"from Ticket where date >= '{DateTime.Now.AddDays(-30)}'{fil} order by idticket desc;";
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                        tts.Add(new incident {
                            id = int.Parse($"{dr[0]}"),
                            date = DateTime.Parse($"{dr[1]}"),
                            status = "active",
                            from = $"{dr[2]}",
                            to = $"{dr[3]}",
                            importance = $"{dr[4]}",
                            subject = $"{dr[5]}",
                            message = $"{dr[6]}",
                            path = $"{dr[7]}",
                            base64 = (byte[])dr[8],
                        });
                }
            }
            catch (Exception ex)
            {
            }
            return tts;
        }
        public List<registro> get_users(String filterbyid = "", String fmt = "HH:mm")
        {
            List<registro> users = new List<registro>();
            try
            {
                var fil = string.IsNullOrEmpty(filterbyid) ? "" : $" where a.idAtencion = {filterbyid}";
                var SQL = "select a.idAtencion, a.Empleado, b.usuario, b.Contraseña, a.Departamento, b.rol, c.entrada, c.salida, c.comida_in, c.comida_out from Atencion a inner join " +
                    $"UsuariosTickets b on a.idAtencion = b.idusuarios inner join Horarios c on a.idAtencion = c.idAtencion{fil} order by a.idAtencion desc;";
                users = get_users_filtered(SQL, fmt);
            }
            catch (Exception ex)
            {
            }
            return users;
        }
        public List<registro> get_filter_users(String filter = "", String fmt = "HH:mm")
        {
            List<registro> users = new List<registro>();
            try
            {
                var fil = string.IsNullOrEmpty(filter) ? "" : $" where a.Empleado like '%{filter}%' or a.Departamento like '%{filter}%'";
                var SQL = "select a.idAtencion, a.Empleado, b.usuario, b.Contraseña, a.Departamento, b.rol, c.entrada, c.salida, c.comida_in, c.comida_out from Atencion a inner join " +
                    $"UsuariosTickets b on a.idAtencion = b.idusuarios inner join Horarios c on a.idAtencion = c.idAtencion{fil} order by a.idAtencion desc;";
                users = get_users_filtered(SQL, fmt);
            }
            catch (Exception ex)
            {
            }
            return users;
        }
        private List<registro> get_users_filtered(string SQL, String fmt = "")
        {
            List<registro> users = new List<registro>();
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(SQL, getcon());
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        users.Add(new registro
                        {
                            idatencion = int.Parse($"{dr[0]}"),
                            empleado = $"{dr[1]}",
                            usuario = $"{dr[2]}",
                            contrasena = $"{dr[3]}",
                            departamento = $"{dr[4]}",
                            rol = $"{dr[5]}",
                            departamentos = get_departments(),
                            roles = get_roles(),
                            entrada = DateTime.Parse($"{dr[6]}").ToString(fmt),
                            salida = DateTime.Parse($"{dr[7]}").ToString(fmt),
                            entrada_comida = DateTime.Parse($"{dr[8]}").ToString(fmt),
                            salida_comida = DateTime.Parse($"{dr[9]}").ToString(fmt),
                        });
                    }
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
        public bool delete_user(String id)
        {
            bool status = true;
            try
            {
                CON = getcon();
                var SQL = $"delete from Atencion Where idAtencion = {id};" +
                    $"delete from UsuariosTickets Where idUsuarios = {id};" +
                    $"delete from Horarios Where idAtencion = {id};select 1;";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count <= 0)
                {
                    status = false;
                }
            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;
        }
        /// <summary>
        /// Metodo para actualizar los datos del usuario q llegan del formulario de edicion
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        public string Update_Data_User(registro usr)
        {
            String status = "ok";
            try
            {
                CON = getcon();
                var SQL = $"select idAtencion from Atencion where idAtencion = '{usr.idatencion}';";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count <= 0)
                {
                    status = "notexist";
                }
                else
                {
                    SQL = $"Update Atencion set Empleado = '{usr.empleado}', Departamento = '{usr.departamento}' where idAtencion = {usr.idatencion};" +
                        $"Update UsuariosTickets Set [Contraseña] = '{usr.contrasena}', Rol = '{usr.rol}' Where idUsuarios = {usr.idatencion};" +
                        $"Update Horarios Set entrada = '1900-01-01 {usr.entrada}', salida = '1900-01-01 {usr.salida}', comida_in = '1900-01-01 {usr.entrada_comida}', " +
                        $"comida_out = '1900-01-01 {usr.salida_comida}' Where idAtencion = {usr.idatencion};select 1;";
                    da = new SqlDataAdapter(SQL, CON);
                    dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count <= 0)
                    {
                        status = "updateerror";
                    }
                }
            }
            catch (Exception ex)
            {
                status = "error";
            }
            return status;
        }
        public string Update_Data_Incident(incident tt)
        {
            String status = "ok";
            try
            {
                CON = getcon();
                var SQL = $"select idticket from Ticket where idticket = '{tt.id}';";
                SqlDataAdapter da = new SqlDataAdapter(SQL, CON);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count <= 0)
                {
                    status = "notexist";
                }
                else
                {
                    String subsql = tt.base64 != null ? ", [Attachment To Base 64]=@att " : "";
                    SQL = "UPDATE Ticket SET [from]=@from, [to]=@to, importance=@imp, [subject]=@subject, " +
                        $"[message]=@message{subsql} WHERE idticket=@id;";
                    SqlCommand query = new SqlCommand(SQL, CON);

                    query.Parameters.AddWithValue("@from", tt.from);
                    query.Parameters.AddWithValue("@to", tt.to);
                    query.Parameters.AddWithValue("@imp", tt.importance);
                    query.Parameters.AddWithValue("@subject", tt.subject);
                    query.Parameters.AddWithValue("@message", tt.message);
                    if (tt.base64 != null)
                    {
                        query.Parameters.AddWithValue("@att", tt.base64);
                    }
                    query.Parameters.AddWithValue("@id", tt.id);
                    query.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                status = "error";
            }
            return status;
        }
    }
}