using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServidorPersistencia.Data;
using ServidorPersistencia.Model;
using ServidorPersistencia.Parsing;

namespace ServidorPersistencia.Networking
{
    internal class ServidorUsuarios
    {
        private readonly TcpListener servidor;
        private readonly UsuarioRepository repositorio;
        private readonly UsuarioParser parser;
        private readonly object bloqueo = new object();

        public ServidorUsuarios(int puerto, UsuarioRepository repositorio)
        {
            servidor = new TcpListener(IPAddress.Any, puerto);
            this.repositorio = repositorio;
            parser = new UsuarioParser();
        }

        public void Start()
        {
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

        private void ManejarCliente(TcpClient cliente)
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

                    string respuesta = ProcesarUsuario(json);
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

        private string ProcesarUsuario(string json)
        {
            Usuario usuario;
            try
            {
                usuario = parser.Parsear(json);
            }
            catch
            {
                return "ERROR: JSON inválido.";
            }

            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                return "ERROR: El nombre no puede estar vacío.";
            if (usuario.Edad <= 0 || usuario.Edad > 120)
                return "ERROR: La edad debe estar entre 1 y 120.";
            if (string.IsNullOrWhiteSpace(usuario.Correo))
                return "ERROR: El correo no puede estar vacío.";
            if (string.IsNullOrWhiteSpace(usuario.Ciudad))
                return "ERROR: La ciudad no puede estar vacía.";
            if (string.IsNullOrWhiteSpace(usuario.Telefono))
                return "ERROR: El teléfono no puede estar vacío.";

            try
            {
                lock (bloqueo)
                {
                    repositorio.GuardarUsuario(usuario);
                }
            }
            catch (Exception ex)
            {
                return "ERROR: No se pudo guardar en SQL Server. " + ex.Message;
            }

            Console.WriteLine(string.Format("Guardado en SQL Server -> Nombre: {0}, Edad: {1}, Correo: {2}, Ciudad: {3}, Teléfono: {4}",
                usuario.Nombre, usuario.Edad, usuario.Correo, usuario.Ciudad, usuario.Telefono));

            return "OK: Usuario guardado correctamente.";
        }
    }
}
