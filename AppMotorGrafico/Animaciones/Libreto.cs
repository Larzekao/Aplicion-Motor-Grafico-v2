using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMotorGrafico.Animaciones
{
    public class Libreto
    {
      
        public Dictionary<string, Escena> Escenas { get; private set; }

        public Libreto()
        {
            Escenas = new Dictionary<string, Escena>();
        }

        public void AgregarEscena(string nombre, Escena escena)
        {
            Escenas[nombre] = escena;
        }

        public void EjecutarEscena(string nombre)
        {
            if (Escenas.TryGetValue(nombre, out Escena escena))
            {
                escena.Ejecutar();
            }
          
        }
    }

}
