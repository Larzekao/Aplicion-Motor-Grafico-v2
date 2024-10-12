using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace AppMotorGrafico.figuras3d
{
    public class UncParte : Figura3D
    {
        public Dictionary<string, UncPoligono> Poligonos { get;  set; }
        public Color4 Color { get; set; }
        public bool IsSelected { get; set; } = false;

        public UncParte()
        {
            // No inicializar Poligonos aquí
        }


        public UncParte(Color4 color)
        {
            Poligonos = new Dictionary<string, UncPoligono>();
            Color = color;
        }

        public void AñadirPoligono(string id, UncPoligono poligono)
        {
            Poligonos[id] = poligono;
        }

       
        public bool EliminarPoligono(string id)
        {
            return Poligonos.Remove(id);
        }

        // Nuevo método para obtener un polígono por su ID
        public UncPoligono ObtenerPoligono(string id)
        {
            if (Poligonos.TryGetValue(id, out UncPoligono poligono))
            {
                return poligono;
            }
            else
            {
                Console.WriteLine($"El polígono con ID '{id}' no existe en esta parte.");
                return null;
            }
        }
        public UncPunto CalcularCentroDeMasa()
        {
            if (Poligonos == null || Poligonos.Count == 0)
                return new UncPunto();

            var centros = Poligonos.Values.Select(p => p.CalcularCentroDeMasa()).ToList();

            double xProm = centros.Average(p => p.X);
            double yProm = centros.Average(p => p.Y);
            double zProm = centros.Average(p => p.Z);

            return new UncPunto(xProm, yProm, zProm);
        }


        public void Trasladar(double tx, double ty, double tz)
        {
            foreach (var poligono in Poligonos.Values)
            {
                poligono.Trasladar(tx, ty, tz);
            }
        }

        public void Escalar(double factor)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Escalar(factor, centro);
        }

        public void Escalar(double factor, UncPunto centro)
        {
            // Escalar todos los polígonos de la parte
            foreach (var poligono in Poligonos.Values)
            {
                poligono.Trasladar(-centro.X, -centro.Y, -centro.Z);
                poligono.Escalar(factor, centro); // Escalar en torno al centro
                poligono.Trasladar(centro.X, centro.Y, centro.Z);
            }
        }

        public void Rotar(double anguloX, double anguloY, double anguloZ)
        {
            UncPunto centro = CalcularCentroDeMasa();
            Rotar(anguloX, anguloY, anguloZ, centro);
        }

        public  void Rotar(double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            // Rotar cada polígono de la parte
            foreach (var poligono in Poligonos.Values)
            {
                poligono.Trasladar(-centro.X, -centro.Y, -centro.Z);
                poligono.Rotar(anguloX, anguloY, anguloZ, new UncPunto(0, 0, 0));
                poligono.Trasladar(centro.X, centro.Y, centro.Z);
            }
        }

        public Figura3D ObtenerElemento(string id)
        {
            if (Poligonos.ContainsKey(id))
                return Poligonos[id];
            else
                return null;
        }

        public void Dibujar()
        {
            if (IsSelected)
            {
                GL.Color4(Color4.Green); // Resaltar si está seleccionado
            }
            else
            {
                GL.Color4(Color); // Dibujar con el color original
            }

            foreach (var poligono in Poligonos.Values)
            {
                poligono.Dibujar();
            }
        }

    }
}
