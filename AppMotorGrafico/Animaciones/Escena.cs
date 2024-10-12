using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMotorGrafico.Animaciones
{
    public class Escena
    {
      
        public Dictionary<string, Accion> Acciones { get; private set; }

        public Escena()
        {
            Acciones = new Dictionary<string, Accion>();
        }

        
        public void AgregarAccion(string nombre, Accion accion)
        {
            Acciones[nombre] = accion;
        }

        
        public void Ejecutar()
        {
            foreach (var accion in Acciones.Values)
            {
                accion.Ejecutar();
            }
        }
    }

}
