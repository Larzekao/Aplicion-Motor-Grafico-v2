using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMotorGrafico.Animaciones
{
    public class Accion
    {
       
        public Dictionary<string, Transformacion> Transformaciones { get; private set; }

        public Accion()
        {
            Transformaciones = new Dictionary<string, Transformacion>();
        }

      
        public void AgregarTransformacion(string nombre, Transformacion transformacion)
        {
            Transformaciones[nombre] = transformacion;
        }

       
        public void Ejecutar()
        {
            foreach (var transformacion in Transformaciones.Values)
            {
                transformacion.Ejecutar();
            }
        }
    }

}
