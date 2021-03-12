using _OLC2_Proyecto2.Ejecucion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Irony.Parsing;
using System.Collections;
using _OLC2_Proyecto2.Arbol;

namespace _OLC2_Proyecto2
{
    public partial class Form1 : Form
    {

        public static RichTextBox Consola;
        public static RichTextBox Salida;
        Analisis analisis = new Analisis();

        public Form1()
        {
            InitializeComponent();
            Consola = richTextConsola;
            Salida = richTxtSalida;
        }

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            richTextConsola.Clear();
            richTxtSalida.Clear();

            Boolean resultado = analisis.esCadenaValida(richTxtEntrada.Text);

            if (resultado)
            {
                //MessageBox.Show("Analisis Exitoso");
                Consola.AppendText("Analisis Exitoso!\n");
                
                analisis.EjecutarAccionesPerronas();
            }
            else
            {
                MessageBox.Show("Entrada contiene errores lexicos o sintacticos");
                ImprimirErroresLexicoSintactico();
            }
        }

        private void ImprimirErroresLexicoSintactico()
        {
            richTextConsola.AppendText("================== ERRORES LEXICOS Y SINTACTICOS ==================" + "\n");
            richTextConsola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t" + "Descripcion" + "\n");
            foreach (TokenError error in Analizar.lista_errores)
            {
                richTextConsola.AppendText(error.linea + "\t" + error.columna + "\t" + error.tipo + "\t" + error.descripcion + "\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("click!!!!!!!!!!!!!");
        }
    }
}
