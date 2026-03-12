using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ServidorPersistencia
{
    internal class Estudiante
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Carrera { get; set; }
    }

    internal class Program
    {
        private static readonly string CadenaConexion = "Data Source=estudiantes.db";
        private static readonly object Bloqueo = new object();

        static void Main(string[] args)
        {
            SQLitePCL.Batteries_V2.Init();

            try
            {
                InicializarBaseDatos();
                MostrarEstudiantesGuardados();
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo inicializar SQLite. Verifica el paquete instalado.");
                Console.WriteLine("Detalle: " + ex.Message);
                return;
            }

            TcpListener servidor = new TcpListener(IPAddress.Any, 8080);
            servidor.Start();
            Console.WriteLine("Servidor escuchando en el puerto 8080...");

            while (true)
            {
                TcpClient cliente = servidor.AcceptTcpClient();
                Thread hilo = new Thread(() => ManejarCliente(cliente));
                hilo.IsBackground = true;
                hilo.Start();
            }
        }

        static void ManejarCliente(TcpClient cliente)
        {
            try
            {
                using (cliente)
                using (NetworkStream stream = cliente.GetStream())
                {
                    byte[] buffer = new byte[4096];
                    int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);

                    Console.WriteLine("\nDatos recibidos: " + json);

                    string respuesta = ProcesarEstudiante(json);
                    byte[] datos = Encoding.UTF8.GetBytes(respuesta);
                    stream.Write(datos, 0, datos.Length);

                    Console.WriteLine("Respuesta enviada: " + respuesta);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al manejar cliente: " + ex.Message);
            }
        }

        static string ProcesarEstudiante(string json)
        {
            Estudiante estudiante;
            try
            {
                estudiante = ParsearEstudiante(json);
            }
            catch
            {
                return "ERROR: JSON inválido.";
            }

            if (string.IsNullOrWhiteSpace(estudiante.Nombre))
                return "ERROR: El nombre no puede estar vacío.";
            if (estudiante.Edad <= 0 || estudiante.Edad > 120)
                return "ERROR: La edad debe estar entre 1 y 120.";
            if (string.IsNullOrWhiteSpace(estudiante.Carrera))
                return "ERROR: La carrera no puede estar vacía.";

            try
            {
                lock (Bloqueo)
                {
                    GuardarEstudiante(estudiante);
                }
            }
            catch (Exception ex)
            {
                return "ERROR: No se pudo guardar en SQLite. " + ex.Message;
            }

            Console.WriteLine(string.Format("Guardado en SQLite -> Nombre: {0}, Edad: {1}, Carrera: {2}",
                estudiante.Nombre, estudiante.Edad, estudiante.Carrera));

            return "OK: Estudiante guardado correctamente.";
        }

        static Estudiante ParsearEstudiante(string json)
        {
            string nombre = ExtraerCadena(json, "nombre");
            int edad = ExtraerEntero(json, "edad");
            string carrera = ExtraerCadena(json, "carrera");

            if (nombre == null || carrera == null || edad < 0)
                throw new FormatException("JSON con estructura inválida.");

            return new Estudiante { Nombre = nombre, Edad = edad, Carrera = carrera };
        }

        static string ExtraerCadena(string json, string clave)
        {
            Match match = Regex.Match(json, "\"" + clave + "\"\\s*:\\s*\"(.*?)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        static int ExtraerEntero(string json, string clave)
        {
            Match match = Regex.Match(json, "\"" + clave + "\"\\s*:\\s*(\\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int valor))
                return valor;
            return -1;
        }

        static void InicializarBaseDatos()
        {
            using (SqliteConnection conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Estudiantes (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                Nombre TEXT NOT NULL,
                                Edad INTEGER NOT NULL,
                                Carrera TEXT NOT NULL
                              );";

                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    comando.ExecuteNonQuery();
                }
            }
        }

        static void GuardarEstudiante(Estudiante estudiante)
        {
            using (SqliteConnection conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                string sql = "INSERT INTO Estudiantes (Nombre, Edad, Carrera) VALUES (@nombre, @edad, @carrera);";

                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@nombre", estudiante.Nombre);
                    comando.Parameters.AddWithValue("@edad", estudiante.Edad);
                    comando.Parameters.AddWithValue("@carrera", estudiante.Carrera);
                    comando.ExecuteNonQuery();
                }
            }
        }

        static List<Estudiante> ObtenerEstudiantes()
        {
            List<Estudiante> estudiantes = new List<Estudiante>();

            using (SqliteConnection conexion = new SqliteConnection(CadenaConexion))
            {
                conexion.Open();
                string sql = "SELECT Id, Nombre, Edad, Carrera FROM Estudiantes;";

                using (SqliteCommand comando = new SqliteCommand(sql, conexion))
                using (SqliteDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        estudiantes.Add(new Estudiante
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Edad = reader.GetInt32(2),
                            Carrera = reader.GetString(3)
                        });
                    }
                }
            }

            return estudiantes;
        }

        static void MostrarEstudiantesGuardados()
        {
            List<Estudiante> estudiantes = ObtenerEstudiantes();

            Console.WriteLine("Estudiantes cargados desde SQLite: " + estudiantes.Count);
            foreach (Estudiante e in estudiantes.OrderBy(x => x.Id))
            {
                Console.WriteLine(string.Format("  - Id: {0}, Nombre: {1}, Edad: {2}, Carrera: {3}",
                    e.Id, e.Nombre, e.Edad, e.Carrera));
            }
        }
    }
}
