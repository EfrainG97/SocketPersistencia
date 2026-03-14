using System;
using System.Text.RegularExpressions;
using ServidorPersistencia.Model;

namespace ServidorPersistencia.Parsing
{
    internal class UsuarioParser
    {
        public Usuario Parsear(string json)
        {
            string nombre = ExtraerCadena(json, "nombre");
            int edad = ExtraerEntero(json, "edad");
            string correo = ExtraerCadena(json, "correo");
            string ciudad = ExtraerCadena(json, "ciudad");
            string telefono = ExtraerCadena(json, "telefono");

            if (nombre == null || correo == null || ciudad == null || telefono == null || edad < 0)
                throw new FormatException("JSON con estructura inválida.");

            return new Usuario { Nombre = nombre, Edad = edad, Correo = correo, Ciudad = ciudad, Telefono = telefono };
        }

        private static string ExtraerCadena(string json, string clave)
        {
            Match match = Regex.Match(json, "\"" + clave + "\"\\s*:\\s*\"(.*?)\"");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static int ExtraerEntero(string json, string clave)
        {
            Match match = Regex.Match(json, "\"" + clave + "\"\\s*:\\s*(\\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int valor))
                return valor;
            return -1;
        }
    }
}
