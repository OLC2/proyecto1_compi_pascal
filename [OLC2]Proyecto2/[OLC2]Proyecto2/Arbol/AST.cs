using _OLC2_Proyecto2.Ejecucion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;
using System.Diagnostics;

namespace _OLC2_Proyecto2.Arbol
{
    class AST
    {
        private List<Entorno> entornos;

        //private Entorno ent;

        private string ambito = "raiz";
        //private Boolean activo; //Esto me ayuda para hacer la busqueda de variables globales

        public AST()
        {
            this.entornos = new List<Entorno>();
        }

        public List<Entorno> CrearArbol(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto
                        Entorno ent = new Entorno(Nodo, ambito);
                        ent.CrearArbol();
                        entornos.Add(ent);
                        break;
                    default:
                        Form1.Consola.AppendText("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado"+"\n");
                        break;
                }
            }
            return entornos;
        }

    }
}
