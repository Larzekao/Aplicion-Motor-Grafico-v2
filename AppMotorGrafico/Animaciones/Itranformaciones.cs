using AppMotorGrafico.figuras3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppMotorGrafico.Animaciones
{
    public abstract class Transformacion
    {
        public abstract void Ejecutar();
    }

    // Traslación
    public class Traslacion : Transformacion
    {
        private Figura3D objeto;
        private double tx, ty, tz;

        public Traslacion(Figura3D objeto, double tx, double ty, double tz)
        {
            this.objeto = objeto;
            this.tx = tx;
            this.ty = ty;
            this.tz = tz;
        }

        public override void Ejecutar()
        {
            objeto.Trasladar(tx, ty, tz);
        }
    }

    // Rotación
    public class Rotacion : Transformacion
    {
        private Figura3D objeto;
        private double anguloX, anguloY, anguloZ;
        private UncPunto centro;

        public Rotacion(Figura3D objeto, double anguloX, double anguloY, double anguloZ, UncPunto centro)
        {
            this.objeto = objeto;
            this.anguloX = anguloX;
            this.anguloY = anguloY;
            this.anguloZ = anguloZ;
            this.centro = centro;
        }

        public override void Ejecutar()
        {
            objeto.Rotar(anguloX, anguloY, anguloZ, centro);
        }
    }

    // Escalado
    public class Escalado : Transformacion
    {
        private Figura3D objeto;
        private double factor;
        private UncPunto centro;

        public Escalado(Figura3D objeto, double factor, UncPunto centro)
        {
            this.objeto = objeto;
            this.factor = factor;
            this.centro = centro;
        }

        public override void Ejecutar()
        {
            objeto.Escalar(factor, centro);
        }
    }

}
