using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using System.Text.Json;
using System.IO;
using System;

namespace AppMotorGrafico.figuras3d
{
    public class UncObjeto : Figura3D
    {
        public Dictionary<string, UncParte> Partes { get;  set; }
        public Color4 Color { get; set; }
        public bool IsSelected { get; set; } = false;
        public UncObjeto()
        {
            // No inicializar Partes aquí
        }
        public UncObjeto(Color4 color)
        {
            Partes = new Dictionary<string, UncParte>();
            Color = color;
        }

        public void AñadirParte(string id, UncParte parte)
        {
            Partes[id] = parte;
        }

        public bool EliminarParte(string id)
        {
            return Partes.Remove(id);
        }
        // Nuevo método para obtener una parte por su ID
        public UncParte ObtenerParte(string id)
        {
            if (Partes.TryGetValue(id, out UncParte parte))
            {
                return parte;
            }
            else
            {
                Console.WriteLine($"La parte con ID '{id}' no existe en este objeto.");
                return null;
            }
        }
        public UncPunto CalcularCentroDeMasa()
        {
            if (Partes == null || Partes.Count == 0)
                return new UncPunto();

            var centros = Partes.Values.Select(p => p.CalcularCentroDeMasa()).ToList();

            double xProm = centros.Average(p => p.X);
            double yProm = centros.Average(p => p.Y);
            double zProm = centros.Average(p => p.Z);

            return new UncPunto(xProm, yProm, zProm);
        }


        public void Trasladar(double tx, double ty, double tz)
        {
            foreach (var parte in Partes.Values)
            {
                parte.Trasladar(tx, ty, tz);
            }
        }

        public void Escalar(double factor)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Escalar(factor, centro);
        }

        public void Escalar(double factor, UncPunto centro)
        {
            foreach (var parte in Partes.Values)
            {
                parte.Escalar(factor, centro);
            }
        }

        public void Rotar(double anguloX, double anguloY, double anguloZ)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Rotar(anguloX, anguloY, anguloZ, centro);
        }

        public  void Rotar(double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            // Rotar cada parte alrededor del centro del objeto
            foreach (var parte in Partes.Values)
            {
                parte.Trasladar(-centro.X, -centro.Y, -centro.Z);
                parte.Rotar(anguloX, anguloY, anguloZ, new UncPunto(0, 0, 0)); // Rotar en torno al origen
                parte.Trasladar(centro.X, centro.Y, centro.Z); // Regresar a su lugar
            }
        }

        public Figura3D ObtenerElemento(string id)
        {
            if (Partes.ContainsKey(id))
                return Partes[id];
            else
                return null;
        }


        public void Dibujar()
        {
            if (IsSelected)
            {
                GL.Color4(Color4.Yellow); // Resaltar si está seleccionado
            }
            else
            {
                GL.Color4(Color); // Dibujar con el color original
            }

            foreach (var parte in Partes.Values)
            {
                parte.Dibujar();
            }
        }


    }
}
