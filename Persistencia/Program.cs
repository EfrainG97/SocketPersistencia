using System;
using System.Net.Sockets;
using System.Text;

namespace Persistencia
{
    internal class Usuario
    {
        public string Nombre { get; set; }
        public int Edad { get; set; }
        public string Correo { get; set; }
        public string Ciudad { get; set; }
        public string Telefono { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("IP del servidor: ");
            string servidor = Console.ReadLine();

            int puerto = 0;
            while (true)
            {
                Console.Write("Puerto del servidor: ");
                if (int.TryParse(Console.ReadLine(), out puerto))
                    break;
                Console.WriteLine("Puerto inválido, ingrese un número entero.");
            }

            string continuar;

            do
            {
                Console.WriteLine("\n--- Ingrese los datos del usuario ---");

                Console.Write("Nombre: ");
                string nombre = Console.ReadLine();

                int edad = 0;
                while (true)
                {
                    Console.Write("Edad: ");
                    if (int.TryParse(Console.ReadLine(), out edad))
                        break;
                    Console.WriteLine("Edad inválida, ingrese un número entero.");
                }

                Console.Write("Correo: ");
                string correo = Console.ReadLine();

                Console.Write("Ciudad: ");
                string ciudad = Console.ReadLine();

                Console.Write("Teléfono: ");
                string telefono = Console.ReadLine();

                Usuario usuario = new Usuario
                {
                    Nombre = nombre,
                    Edad = edad,
                    Correo = correo,
                    Ciudad = ciudad,
                    Telefono = telefono
                };

                string json = string.Format(
                    "{{\"nombre\":\"{0}\",\"edad\":{1},\"correo\":\"{2}\",\"ciudad\":\"{3}\",\"telefono\":\"{4}\"}}",
                    usuario.Nombre,
                    usuario.Edad,
                    usuario.Correo,
                    usuario.Ciudad,
                    usuario.Telefono
                );

                try
                {
                    using (TcpClient cliente = new TcpClient(servidor, puerto))
                    using (NetworkStream stream = cliente.GetStream())
                    {
                        byte[] datos = Encoding.UTF8.GetBytes(json);
                        stream.Write(datos, 0, datos.Length);
                        Console.WriteLine("Datos enviados: " + json);

                        byte[] buffer = new byte[1024];
                        int bytesLeidos = stream.Read(buffer, 0, buffer.Length);
                        string respuesta = Encoding.UTF8.GetString(buffer, 0, bytesLeidos);
                        Console.WriteLine("Respuesta del servidor: " + respuesta);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                Console.Write("\n¿Desea enviar otro usuario? (s/n): ");
                continuar = Console.ReadLine();

            } while (continuar != null && continuar.Trim().ToLower() == "s");

            Console.WriteLine("Presione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
