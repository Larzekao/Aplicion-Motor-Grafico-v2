using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AppMotorGrafico.Pantalla;
using AppMotorGrafico.seializacion;
using AppMotorGrafico.figuras3d;
using AppMotorGrafico.Animaciones;

namespace AppMotorGrafico
{
    public partial class Form1 : Form
    {
        private GLControl glControl1;
        private TreeView treeView1;
        private System.Windows.Forms.Timer timer;

        private Escenario escenario;
        private Camara3D camara;
        private MenuStrip menuStrip1;

        private enum ModoTransformacion { Ninguno, Trasladar, Rotar, Escalar }
        private ModoTransformacion modoActual = ModoTransformacion.Ninguno;

        private enum Eje { Ninguno, X, Y, Z }
        private Eje ejeActual = Eje.Ninguno;

        private bool mouseTransforming = false;
        private Point lastMousePos;

        // Lista para manejar múltiples selecciones
        private List<Figura3D> objetosSeleccionados = new List<Figura3D>();

        // Variables para la selección de objetos mediante rectángulo
        private bool isSelecting = false;
        private Point selectionStart;
        private Point selectionEnd;
        private Rectangle selectionRectangle;


        private Libreto libreto;
        private DateTime tiempoInicioAnimacion;


        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            this.WindowState = FormWindowState.Maximized;

            InicializarMenuStrip();
            InicializarTreeView();
            InicializarGLControl();

            camara = new Camara3D();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 16; //|
            timer.Tick += Timer_Tick;
            timer.Start();

            // Asume que tienes botones con estos nombres en tu formulario
            button1.Click += BtnTrasladar_Click;
            button2.Click += BtnRotar_Click;
            button3.Click += BtnEscalar_Click;
            // Crear y ajustar el botón de animación
            Button buttonAnimar = new Button();
            buttonAnimar.Text = "Ejecutar Animación";
            buttonAnimar.Size = new Size(150, 50);
            buttonAnimar.BackColor = Color.LightBlue;
            buttonAnimar.Location = new Point(10, 30); // Posición visible por debajo del menú
            buttonAnimar.Click += BtnAnimar_Click;
            this.Controls.Add(buttonAnimar);
        }
     

        // Función auxiliar para crear un polígono (una cara del cubo)
        private UncPoligono CrearPoligono(UncPunto[] vertices, Color4 color)
        {
            UncPoligono poligono = new UncPoligono(color);
            for (int i = 0; i < vertices.Length; i++)
            {
                poligono.AñadirVertice("v" + i, vertices[i]);
            }
            return poligono;
        }

        private void InicializarMenuStrip()
        {
            menuStrip1 = new MenuStrip();
            var archivo = new ToolStripMenuItem("Archivo");
            var nuevo = new ToolStripMenuItem("Nuevo");
            var abrir = new ToolStripMenuItem("Abrir");
            var guardar = new ToolStripMenuItem("Guardar");
            var salir = new ToolStripMenuItem("Salir");
            salir.Click += (s, e) => this.Close();

            archivo.DropDownItems.Add(nuevo);
            archivo.DropDownItems.Add(abrir);
            archivo.DropDownItems.Add(guardar);
            archivo.DropDownItems.Add(new ToolStripSeparator());
            archivo.DropDownItems.Add(salir);

            var opciones = new ToolStripMenuItem("Opciones");
            var ayuda = new ToolStripMenuItem("Ayuda");

            menuStrip1.Items.Add(archivo);
            menuStrip1.Items.Add(opciones);
            menuStrip1.Items.Add(ayuda);
            this.MainMenuStrip = menuStrip1;
            this.Controls.Add(menuStrip1);
        }

        private void InicializarTreeView()
        {
            treeView1 = new TreeView();
            treeView1.Width = 200;
            treeView1.Height = (this.ClientSize.Height / 2) - menuStrip1.Height - 20;
            treeView1.Location = new Point(this.ClientSize.Width - treeView1.Width - 10, menuStrip1.Height + 10);
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            treeView1.AfterSelect += TreeView1_AfterSelect;
            this.Controls.Add(treeView1);
        }

        private void InicializarGLControl()
        {
            glControl1 = new GLControl(new GraphicsMode(32, 24, 0, 4));
            glControl1.BackColor = Color.Black;
            glControl1.Location = new Point(0, menuStrip1.Height + 10);
            glControl1.Size = new Size(this.ClientSize.Width - treeView1.Width - 30, this.ClientSize.Height - menuStrip1.Height - 20);
            glControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            glControl1.Load += glControl1_Load;
            glControl1.Paint += glControl1_Paint;
            glControl1.Resize += glControl1_Resize;
            glControl1.MouseDown += GlControl1_MouseDown;
            glControl1.MouseMove += GlControl1_MouseMove;
            glControl1.MouseUp += GlControl1_MouseUp;
            glControl1.MouseWheel += GlControl1_MouseWheel;
            this.Controls.Add(glControl1);
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            camara.IniciarMatrices(glControl1.Width, glControl1.Height);
            InicializarEscena();
        }
        private async void BtnAnimar_Click(object sender, EventArgs e)
        {
            Figura3D objeto1 = escenario.ObtenerFigura("objetoT1");
          
            libreto = new Libreto();
            Escena escena1 = new Escena();

            double duracionTotal = 10.0;
            double duracionPorLado = duracionTotal / 4.0;

            UncObjeto objetoCompleto = objeto1 as UncObjeto;

            UncParte parteHori = objetoCompleto.ObtenerParte("rectanguloHorizontal");
            
            // Lado 1: 
            Accion moverDerecha = new Accion(0.0, duracionPorLado);
            moverDerecha.AgregarTransformacion(new Traslacion(objeto1, 10.0, 0.0, 0.0, duracionPorLado));
            moverDerecha.AgregarTransformacion(new Rotacion(parteHori, 0.0, 10080.0, 0.0, () => parteHori.CalcularCentroDeMasa(), duracionPorLado));

            // Lado 2: 
            Accion moverAdelante = new Accion(duracionPorLado, duracionPorLado);
            moverAdelante.AgregarTransformacion(new Traslacion(objeto1, 0.0, 0.0, 5.0, duracionPorLado));
            moverAdelante.AgregarTransformacion(new Rotacion(parteHori, 0.0, 720.0, 0.0, () => parteHori.CalcularCentroDeMasa(), duracionPorLado));

            // Lado 3: 
            Accion moverIzquierda = new Accion(duracionPorLado * 2.0, duracionPorLado);
            moverIzquierda.AgregarTransformacion(new Traslacion(objeto1, -10.0, 0.0, 0.0, duracionPorLado));
            moverIzquierda.AgregarTransformacion(new Rotacion(parteHori, 0.0, 720.0, 0.0, () => parteHori.CalcularCentroDeMasa(), duracionPorLado));

            // Lado 4: 
            Accion moverAtras = new Accion(duracionPorLado * 3.0, duracionPorLado);
            moverAtras.AgregarTransformacion(new Traslacion(objeto1, 0.0, 0.0, -5.0, duracionPorLado));
            moverAtras.AgregarTransformacion(new Rotacion(parteHori, 0.0, 720.0, 0.0, () => parteHori.CalcularCentroDeMasa(), duracionPorLado));

           
            escena1.AgregarAccion("MoverDerecha", moverDerecha);
            escena1.AgregarAccion("MoverAdelante", moverAdelante);
            escena1.AgregarAccion("MoverIzquierda", moverIzquierda);
            escena1.AgregarAccion("MoverAtras", moverAtras);

           
            libreto.AgregarEscena("AnimacionCancha", escena1);

           
            await EjecutarEscenaAsincrona(libreto, "AnimacionCancha");
        }






        private async Task EjecutarEscenaAsincrona(Libreto libreto, string nombreEscena)
           {
    tiempoInicioAnimacion = DateTime.Now;

    while (!libreto.EstaCompletado(GetTiempoActual()))
    {
        double tiempoActual = GetTiempoActual();
        libreto.Ejecutar(tiempoActual); 
        glControl1.Invalidate();  
        await Task.Delay(16);  
    }
              }

               private double GetTiempoActual()
           {
    return (DateTime.Now - tiempoInicioAnimacion).TotalSeconds;
                  }






        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            camara.ConfigurarMatrices();
            escenario.Dibujar();

            //  rectángulo de selección si est selección
            if (isSelecting)
            {
                DrawSelectionRectangle();
            }

            glControl1.SwapBuffers();
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            if (glControl1.ClientSize.Height == 0)
                glControl1.ClientSize = new Size(glControl1.ClientSize.Width, 1);

            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            camara.IniciarMatrices(glControl1.Width, glControl1.Height);

            treeView1.Height = (this.ClientSize.Height / 2) - menuStrip1.Height - 20;
            treeView1.Location = new Point(this.ClientSize.Width - treeView1.Width - 10, menuStrip1.Height + 10);

            glControl1.Size = new Size(this.ClientSize.Width - treeView1.Width - 30, this.ClientSize.Height - menuStrip1.Height - 20);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            glControl1.Invalidate();
        }

        private void InicializarEscena()
        {
            escenario = new Escenario(Color4.Black);
            Serializador ser = new Serializador();

            Figura3D objetoT1 = ser.Deserializar("ObjetoT");
            Figura3D objetoT2 = ser.Deserializar("ObjetoT");



            objetoT1.Trasladar(3, 1, 0); 


            objetoT2.Trasladar(-2, 0, 0);


            objetoT1.Escalar(1.5, objetoT1.CalcularCentroDeMasa());

            escenario.AgregarFigura("objetoT1", objetoT1);
            escenario.AgregarFigura("objetoT2", objetoT2);

         
            ActualizarTreeView();
        }

        private void ActualizarTreeView()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);
                TreeNode nodoObjeto = new TreeNode(figuraEntry) { Tag = objeto };

                if (objeto is UncObjeto uncObjeto)
                {
                    foreach (var parteEntry in uncObjeto.Partes)
                    {
                        var parte = parteEntry.Value;
                        TreeNode nodoParte = new TreeNode(parteEntry.Key) { Tag = parte };

                        foreach (var poligonoEntry in parte.Poligonos)
                        {
                            var poligono = poligonoEntry.Value;
                            TreeNode nodoPoligono = new TreeNode(poligonoEntry.Key) { Tag = poligono };

                            foreach (var puntoEntry in poligono.Puntos)
                            {
                                var punto = puntoEntry.Value;
                                TreeNode nodoPunto = new TreeNode(puntoEntry.Key) { Tag = punto };
                                nodoPoligono.Nodes.Add(nodoPunto);
                            }

                            nodoParte.Nodes.Add(nodoPoligono);
                        }

                        nodoObjeto.Nodes.Add(nodoParte);
                    }
                }

                treeView1.Nodes.Add(nodoObjeto);
            }

            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        private void TreeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var seleccionado = e.Node.Tag;

            // Deseleccionamos todo antes de aplicar la selección
            DeseleccionarTodos();

            // Limpiar la lista de seleccionados
            objetosSeleccionados.Clear();

            // Marcamos lo que seleccionamos como "objetoSeleccionado"
            if (seleccionado is Figura3D figura)
            {
                figura.IsSelected = true;
                objetosSeleccionados.Add(figura);
               
            }
            else if (seleccionado is UncParte parte)
            {
                parte.IsSelected = true;
                objetosSeleccionados.Add(parte);
              
            }
            else if (seleccionado is UncPoligono poligono)
            {
                poligono.IsSelected = true;
                objetosSeleccionados.Add(poligono);
                
            }
            else if (seleccionado is UncPunto punto)
            {
                // No asignamos puntos como objetos seleccionados directamente
                Console.WriteLine($"Vértice seleccionado: {e.Node.Text}");
            }

            glControl1.Invalidate(); // Para refrescar la pantalla después de la selección
        }

        private void DeseleccionarTodos()
        {
            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);
                objeto.IsSelected = false;

                if (objeto is UncObjeto uncObjeto)
                {
                    foreach (var parte in uncObjeto.Partes.Values)
                    {
                        parte.IsSelected = false;
                        foreach (var poligono in parte.Poligonos.Values)
                        {
                            poligono.IsSelected = false;
                        }
                    }
                }
            }
        }

      



        private void BtnTrasladar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Trasladar;
        }

        private void BtnRotar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Rotar;
        }

        private void BtnEscalar_Click(object sender, EventArgs e)
        {
            modoActual = ModoTransformacion.Escalar;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.X:
                    ejeActual = Eje.X;
                    break;
                case Keys.Y:
                    ejeActual = Eje.Y;
                    break;
                case Keys.Z:
                    ejeActual = Eje.Z;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.X || e.KeyCode == Keys.Y || e.KeyCode == Keys.Z)
                ejeActual = Eje.Ninguno;
        }

        private void GlControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                isSelecting = true;
                selectionStart = e.Location;
                selectionEnd = e.Location;
            }
            else if (e.Button == MouseButtons.Right && modoActual != ModoTransformacion.Ninguno && ejeActual != Eje.Ninguno)
            {
                if (objetosSeleccionados.Count > 0)
                {
                    mouseTransforming = true;
                    lastMousePos = e.Location;
                }
            }
            else
            {
                camara.OnMouseDown(e);
            }
        }

        private void GlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                selectionEnd = e.Location;
                selectionRectangle = GetRectangleFromPoints(selectionStart, selectionEnd);
                glControl1.Invalidate();
            }
            else if (mouseTransforming && objetosSeleccionados.Count > 0)
            {
                int deltaX = e.X - lastMousePos.X;
                int deltaY = e.Y - lastMousePos.Y;
                lastMousePos = e.Location;

                switch (modoActual)
                {
                    case ModoTransformacion.Trasladar:
                        AplicarTraslacion(deltaX, deltaY);
                        break;
                    case ModoTransformacion.Rotar:
                        AplicarRotacion(deltaX, deltaY);
                        break;
                    case ModoTransformacion.Escalar:
                        AplicarEscalado(deltaX, deltaY);
                        break;
                }

                glControl1.Invalidate();
            }
            else
            {
                camara.OnMouseMove(e);
                glControl1.Invalidate();
            }
        }

        private void GlControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                isSelecting = false;
                PerformSelection();
                glControl1.Invalidate();
            }
            else if (mouseTransforming)
            {
                mouseTransforming = false;
            }
            else
            {
                camara.OnMouseUp(e);
            }
        }

        private void GlControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            camara.OnMouseWheel(e);
            glControl1.Invalidate();
        }

        private Rectangle GetRectangleFromPoints(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        private void DrawSelectionRectangle()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, glControl1.Width, glControl1.Height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Disable(EnableCap.DepthTest);
            GL.Color4(1.0f, 1.0f, 1.0f, 0.3f); // Blanco semitransparente
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(selectionRectangle.Left, selectionRectangle.Top);
            GL.Vertex2(selectionRectangle.Right, selectionRectangle.Top);
            GL.Vertex2(selectionRectangle.Right, selectionRectangle.Bottom);
            GL.Vertex2(selectionRectangle.Left, selectionRectangle.Bottom);
            GL.End();
            GL.Enable(EnableCap.DepthTest);

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void PerformSelection()
        {
            objetosSeleccionados.Clear(); // Limpiar la lista antes de la selección

            foreach (var figuraEntry in escenario.ListarFiguras())
            {
                var objeto = escenario.ObtenerFigura(figuraEntry);

                // Revisar si el objeto completo está dentro de la selección
                if (IsObjectInSelection(objeto))
                {
                    objeto.IsSelected = true;
                    objetosSeleccionados.Add(objeto);
                }
                else
                {
                    objeto.IsSelected = false;
                }

                // Si es un objeto compuesto, revisar sus partes y polígonos
                if (objeto is UncObjeto objetoCompuesto)
                {
                    foreach (var parte in objetoCompuesto.Partes.Values)
                    {
                        if (IsObjectInSelection(parte))
                        {
                            parte.IsSelected = true;
                            objetosSeleccionados.Add(parte);
                        }
                        else
                        {
                            parte.IsSelected = false;
                        }

                        foreach (var poligono in parte.Poligonos.Values)
                        {
                            if (IsObjectInSelection(poligono))
                            {
                                poligono.IsSelected = true;
                                objetosSeleccionados.Add(poligono);
                            }
                            else
                            {
                                poligono.IsSelected = false;
                            }
                        }
                    }
                }
            }

            // Actualizar el TreeView para reflejar la selección
            ActualizarTreeView();
        }

        private bool IsObjectInSelection(Figura3D objeto)
        {
            // Obtener el centro de masa del objeto o parte/polígono
            UncPunto centro = objeto.CalcularCentroDeMasa();

            // Convertir las coordenadas del mundo a coordenadas de clip space
            Vector4 worldPosition = new Vector4((float)centro.X, (float)centro.Y, (float)centro.Z, 1.0f);
            Vector4 clipSpacePos = Vector4.Transform(worldPosition, camara.GetModelViewProjectionMatrix());

        
            if (clipSpacePos.W == 0)
                return false;

            Vector3 ndcSpacePos = new Vector3(
                clipSpacePos.X / clipSpacePos.W,
                clipSpacePos.Y / clipSpacePos.W,
                clipSpacePos.Z / clipSpacePos.W);

            
            Point screenPos = new Point(
                (int)(((ndcSpacePos.X + 1.0f) / 2.0f) * glControl1.Width),
                (int)(((1.0f - ndcSpacePos.Y) / 2.0f) * glControl1.Height));

            // Verificar si las coordenadas del objeto están dentro del rectángulo de selección
            return selectionRectangle.Contains(screenPos);
        }
     

        private void AplicarTraslacion(int deltaX, int deltaY)
        {
            double factor = 0.01;
            double dx = 0, dy = 0, dz = 0;

            switch (ejeActual)
            {
                case Eje.X:
                    dx = deltaX * factor;
                    break;
                case Eje.Y:
                    dy = -deltaY * factor;
                    break;
                case Eje.Z:
                    dz = deltaX * factor;
                    break;
            }

            foreach (var objeto in objetosSeleccionados)
            {
                if (objeto is UncPoligono poligono)  // Si es una cara (polígono)
                {
                    poligono.Trasladar(dx, dy, dz); // Solo trasladamos la cara
                }
                else if (objeto is UncParte parte)  // Si es una parte
                {
                    parte.Trasladar(dx, dy, dz); // Trasladamos toda la parte
                }
                else  // Si es un objeto completo
                {
                    objeto.Trasladar(dx, dy, dz);
                }
            }
        }
        private void AplicarRotacion(int deltaX, int deltaY)
        {
            // Factor de sensibilidad para controlar la velocidad de rotación
            double factor = 0.5;
            double angleX = 0, angleY = 0, angleZ = 0;

            // Asignar el ángulo de rotación dependiendo del eje actual
            switch (ejeActual)
            {
                case Eje.X:
                    angleX = deltaY * factor;
                    break;
                case Eje.Y:
                    angleY = deltaX * factor;
                    break;
                case Eje.Z:
                    angleZ = deltaX * factor;
                    break;
            }

           
             
                UncPunto centroGlobal = CalcularCentroDeSeleccion();

                // Aplicar la rotación a cada objeto seleccionado
                foreach (var objeto in objetosSeleccionados)
                {
                    objeto.Rotar(angleX, angleY, angleZ, centroGlobal); 
                }

                // Redibujar la escena
                glControl1.Invalidate();
            
        }

        // Método para calcular el centro de masa o centro de los objetos seleccionados
        private UncPunto CalcularCentroDeSeleccion()
        {
            if (objetosSeleccionados.Count == 0)
            {
                return new UncPunto(0, 0, 0); 
            }

           
            double xProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().X);
            double yProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().Y);
            double zProm = objetosSeleccionados.Average(obj => obj.CalcularCentroDeMasa().Z);

            return new UncPunto(xProm, yProm, zProm);
        }
        


        private void AplicarEscalado(int deltaX, int deltaY)
        {
            // Definir el factor de escalado basándonos en los movimientos del ratón
            double factor = 1.0 + deltaY * 0.01; 

            
            if (objetosSeleccionados.Count > 0)
            {
                
                UncPunto centroGlobal = CalcularCentroDeSeleccion();

     
                foreach (var objeto in objetosSeleccionados)
                {
                    // Paso 1: Trasladar el objeto al origen (respecto al centro de masa)
                    objeto.Trasladar(-centroGlobal.X, -centroGlobal.Y, -centroGlobal.Z);

                
                    objeto.Escalar(factor, new UncPunto(0, 0, 0)); 

                    
                    objeto.Trasladar(centroGlobal.X, centroGlobal.Y, centroGlobal.Z);
                }

                
                glControl1.Invalidate();
            }
        }





    }
}
