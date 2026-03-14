using System.Collections.Generic;
using System.Data.SqlClient;
using ServidorPersistencia.Model;

namespace ServidorPersistencia.Data
{
    internal class UsuarioRepository
    {
        private readonly string cadenaConexion;

        public UsuarioRepository(string cadenaConexion)
        {
            this.cadenaConexion = cadenaConexion;
        }

        public void GuardarUsuario(Usuario usuario)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string sql = "INSERT INTO usuarios (Nombre, Edad, Correo, Ciudad, Telefono) VALUES (@nombre, @edad, @correo, @ciudad, @telefono);";

                using (SqlCommand comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    comando.Parameters.AddWithValue("@edad", usuario.Edad);
                    comando.Parameters.AddWithValue("@correo", usuario.Correo);
                    comando.Parameters.AddWithValue("@ciudad", usuario.Ciudad);
                    comando.Parameters.AddWithValue("@telefono", usuario.Telefono);
                    comando.ExecuteNonQuery();
                }
            }
        }

        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> usuarios = new List<Usuario>();

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string sql = "SELECT Id, Nombre, Edad, Correo, Ciudad, Telefono FROM usuarios;";

                using (SqlCommand comando = new SqlCommand(sql, conexion))
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new Usuario
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Edad = reader.GetInt32(2),
                            Correo = reader.GetString(3),
                            Ciudad = reader.GetString(4),
                            Telefono = reader.GetString(5)
                        });
                    }
                }
            }

            return usuarios;
        }
    }
}
