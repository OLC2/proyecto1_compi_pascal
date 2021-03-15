using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Ast;
using Irony.Parsing;
using System.Threading.Tasks;
using _OLC2_Proyecto2;
using System.Diagnostics;
using _OLC2_Proyecto2.Ejecucion;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace _OLC2_Proyecto2.Arbol
{
    class Analisis : Grammar
    {
        public static List<TokenError> lista_errores = new List<TokenError>();
        public static List<Error> lstErrorS1 = new List<Error>();
        public static List<Error> lstErrorS2 = new List<Error>();

        //Para analizar
        public static ParseTree arbol = null;

        //Arbol
        private static ParseTreeNode raiz = null;

        //Entornos para ejecucion
        private List<Entorno> entorno;

        public Boolean esCadenaValida(string cadena)
        {
            raiz = null;
            arbol = null;

            Gramatica gramatica = new Gramatica();
            LanguageData language = new LanguageData(gramatica);
            Parser parser = new Parser(language);
            arbol = parser.Parse(cadena);
            raiz = arbol.Root;

            lista_errores.Clear();

            //================================================== PARA ERRORES LEXICOS Y SINTACTICOS ====================================================================
            int cont = 0;

            for (int i = 0; i < arbol.ParserMessages.Count(); i++)
            {
                if (arbol.ParserMessages.ElementAt(i).Message.Contains("Invalid character"))
                {
                    cont = 1;
                    TokenError newError = new TokenError();
                    newError = new TokenError(
                    arbol.ParserMessages.ElementAt(i).Location.Line,
                    arbol.ParserMessages.ElementAt(i).Location.Column,
                    "Error Lexico",
                    arbol.ParserMessages.ElementAt(i).Message.Replace("Invalid character:", "Caracter No Reconocido: ")
                    );
                    lista_errores.Add(newError);

                }

                else if (arbol.ParserMessages.ElementAt(i).Message.Contains("Syntax error"))
                {
                    if (!(arbol.ParserMessages.ElementAt(i).Message.Contains(",, $")))

                    {
                        cont = 1;
                        TokenError newError = new TokenError();
                        newError = new TokenError(
                        arbol.ParserMessages.ElementAt(i).Location.Line,
                        arbol.ParserMessages.ElementAt(i).Location.Column,
                        "Error Sintactico",
                        arbol.ParserMessages.ElementAt(i).Message.Replace("Syntax error, expected:", "Se esperaba: ")
                        );
                        lista_errores.Add(newError);
                    }
                }
            }
            //======================================================================================================================

            if (raiz == null)
            {
                return false;
            }

            return true;
        }

        public void EjecutarAccionesPerronas()
        {
            AST ast = new AST();
            
            entorno = ast.CrearArbol(raiz);

            if (entorno != null)
            {
                Form1.Consola.AppendText("Analizando entornos \n");
                Ejecucion ejecucion = new Ejecucion(entorno);
                ejecucion.Procedure();
                lstErrorS1 = ast.getErroresSemanticos();
                lstErrorS2 = ejecucion.getErroresSemanticos();
                ImprimirErrores();
            }
            else
            {
                Form1.Consola.AppendText("Error -> Arbol de entornos nulo. \n");
            }
        }

        private void ImprimirErrores()
        {
            Form1.Consola.AppendText("================== ERRORES SEMANTICOS STRUCTS ==================" + "\n");
            Form1.Consola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t\t" + "Descripcion" + "\n");
            foreach (Error error in lstErrorS1)
            {
                Form1.Consola.AppendText(error.Linea + "\t" + error.Columna + "\t" + error.Tipo + "\t" + error.Descripcion + "\n");
            }
            Form1.Consola.AppendText("================== ERRORES SEMANTICOS EXEC ==================" + "\n");
            Form1.Consola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t\t" + "Descripcion" + "\n");
            foreach (Error error in lstErrorS2)
            {
                Form1.Consola.AppendText(error.Linea + "\t" + error.Columna + "\t" + error.Tipo + "\t" + error.Descripcion + "\n");
            }
            Form1.Consola.AppendText("** Finalizo errores **" + "\n");
        }

        public ParseTreeNode getArbol ()
        {
            return raiz;
        }

        public List<TokenError> getErrores()
        {
            return lista_errores;
        }

    }
}
