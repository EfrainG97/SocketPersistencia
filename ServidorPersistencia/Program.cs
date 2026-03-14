using System;
using System.Collections.Generic;
using System.Linq;
using ServidorPersistencia.Data;
using ServidorPersistencia.Model;
using ServidorPersistencia.Networking;

namespace ServidorPersistencia
{
    internal class Program
    {
        private static readonly string CadenaConexion = "Data Source=localhost;Initial Catalog=SocketsBD;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";

        static void Main(string[] args)
        {
            Console.WriteLine("Iniciando servidor...");
            UsuarioRepository repositorio = new UsuarioRepository(CadenaConexion);

            try
            {
                Console.WriteLine("Conectando a SQL Server...");
                MostrarUsuariosGuardados(repositorio);
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo conectar a SQL Server. Verifica la base de datos.");
                Console.WriteLine("Detalle: " + ex.Message);
                return;
            }

            ServidorUsuarios servidor = new ServidorUsuarios(8080, repositorio);
            servidor.Start();
        }

        static void MostrarUsuariosGuardados(UsuarioRepository repositorio)
        {
            List<Usuario> usuarios = repositorio.ObtenerUsuarios();

            Console.WriteLine("Usuarios cargados desde SQL Server: " + usuarios.Count);
            foreach (Usuario u in usuarios.OrderBy(x => x.Id))
            {
                Console.WriteLine(string.Format("  - Id: {0}, Nombre: {1}, Edad: {2}, Correo: {3}, Ciudad: {4}, Teléfono: {5}",
                    u.Id, u.Nombre, u.Edad, u.Correo, u.Ciudad, u.Telefono));
            }
        }
    }
}
