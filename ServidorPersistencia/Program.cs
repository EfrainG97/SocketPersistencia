using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ServidorPersistencia
{
    internal class Estudiante
    {
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Carrera { get; set; }
    }

    internal class Program
    {
        private static readonly string ArchivoJson = "estudiantes.json";
        private static readonly List<Estudiante> Estudiantes = new List<Estudiante>();
        private static readonly object Bloqueo = new object();

        static void Main(string[] args)
        {
            CargarDatos();

            TcpListener servidor = new TcpListener(IPAddress.Any, 8080);
            servidor.Start();
            Console.WriteLine("Servidor escuchando en el puerto 8080...");
            Console.WriteLine("Estudiantes cargados: " + Estudiantes.Count);

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

            lock (Bloqueo)
            {
                Estudiantes.Add(estudiante);
                GuardarDatos();
            }

            Console.WriteLine(string.Format("Guardado -> Nombre: {0}, Edad: {1}, Carrera: {2}",
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

        static void GuardarDatos()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[");
            for (int i = 0; i < Estudiantes.Count; i++)
            {
                Estudiante e = Estudiantes[i];
                sb.Append("  {\"nombre\":\"").Append(EscaparJson(e.Nombre))
                  .Append("\",\"edad\":").Append(e.Edad)
                  .Append(",\"carrera\":\"").Append(EscaparJson(e.Carrera)).Append("\"}");

                if (i < Estudiantes.Count - 1)
                    sb.AppendLine(",");
                else
                    sb.AppendLine();
            }
            sb.Append("]");

            File.WriteAllText(ArchivoJson, sb.ToString(), Encoding.UTF8);
        }

        static void CargarDatos()
        {
            if (!File.Exists(ArchivoJson))
            {
                Console.WriteLine("No se encontró archivo de datos. Iniciando con lista vacía.");
                return;
            }

            try
            {
                string contenido = File.ReadAllText(ArchivoJson, Encoding.UTF8);
                MatchCollection matches = Regex.Matches(contenido, "\\{[^}]+\\}");

                foreach (Match match in matches)
                {
                    try
                    {
                        Estudiante e = ParsearEstudiante(match.Value);
                        Estudiantes.Add(e);
                    }
                    catch { }
                }

                Console.WriteLine("Datos cargados desde " + ArchivoJson + ". Total: " + Estudiantes.Count);
                foreach (Estudiante e in Estudiantes)
                    Console.WriteLine(string.Format("  - Nombre: {0}, Edad: {1}, Carrera: {2}", e.Nombre, e.Edad, e.Carrera));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar datos: " + ex.Message);
            }
        }

        static string EscaparJson(string texto)
        {
            return texto.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
