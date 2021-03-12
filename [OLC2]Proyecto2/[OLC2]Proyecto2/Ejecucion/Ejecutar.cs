﻿using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Ejecutar
    {
        public TablaFunciones tablafunciones = new TablaFunciones();

        public Stack<TablaSimbolos> pilaSimbolos;
        private TablaSimbolos cima;                     //Esto tiene el ambito actual
        ////public static TablaSimbolos cimaM;            //Esto tiene el ambito de la funcion actual, incluyendo al Main
        private TablaSimbolos cimaG;                    //Esto tiene el ambito de las variables globales

        private List<Parametro> lstParametros;          //Para los parametros de las funciones
        private List<Celda> arreglo;                    //Para los arreglos
        public List<String> lstPrint = new List<String>();
        public List<Error> lstError = new List<Error>();

        private Boolean BanderaCaso = false; //Controla que los casos no repitan sus condiciones

        //private Boolean isRetornoG = false;
        //private Retorno RetornoG;

        //*********************** MINUTOS ANTES DE JODERME XD ------------------------------------------
        //**************************************

        //private int ContadorParams = 0;
        private int nivelActual = 1; //Este controla el nivel que se estara consultando para crear, buscar y modificar las variables locales dentro de metodos, condiciones, ciclos, etc...

        public void IniciarEjecucion(ParseTreeNode Nodo)
        {
            pilaSimbolos = new Stack<TablaSimbolos>();

            //isRetornoG = false;

            TablaSimbolos varg = new TablaSimbolos(0, Reservada.varGlobal, false, false, false);
            pilaSimbolos.Push(varg);
            cimaG = varg;
            Console.WriteLine("ejecutando... Program Init");

            InitialProgram(Nodo);
        }

        private void InitialProgram(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto
                        if (Nodo.ChildNodes.Count == 8)
                        {
                            Estructura(Nodo);
                        }
                        break;
                    default:
                        Console.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado");
                        break;
                }
            }
        }
        private void Estructura(ParseTreeNode Nodo)
        {
            /*
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE | BLOQUE

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        | Empty //Vacio
            */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Estructura(hijo);
                        }
                        break;
                    case "ESTRUCTURA":
                        #region
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> ESTRUCTURA
                            Estructura(Nodo.ChildNodes[1]); // ChildNodes[1] --> BLOQUE
                        }
                        else
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> BLOQUE
                        }
                        #endregion
                        break;
                    case "BLOQUE":
                        #region                        
                        switch (Nodo.ChildNodes[0].Term.Name)
                        {
                            case "VARYTYPE":
                                Variable_y_type(Nodo.ChildNodes[0]); // ChildNodes[0] --> VARYTYPE
                                break;
                            case "FUNCIONES":
                                // ChildNodes[0] --> FUNCIONES
                                Console.WriteLine("** Accion FUNCIONES no funcional");
                                break;
                            case "PROCEDIMIENTO":
                                // ChildNodes[0] --> PROCEDIMIENTO
                                Console.WriteLine("** Accion PROCEDIMIENTO no funcional");
                                break;
                            default:
                                Console.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " es empty/null");
                                break;
                        }
                        #endregion
                        break;
                    case "SENTENCIAS":
                        Console.WriteLine("SENTENCIAS!");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Error AST-->Nodo en funcion Estructura no existente/detectado/null");
            }
        }

        private void Variable_y_type(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "type":
                    Console.WriteLine("** Accion type no funcional");
                    break;
                case "var": //ToTerm("var") + LSTVARS
                    LstVars(Reservada.var, Nodo.ChildNodes[1]);
                    break;
                case "const":
                    Console.WriteLine("** Accion const no funcional");
                    break;
                case "id":
                    Console.WriteLine("** Accion id no funcional");
                    break;
                default:
                    Console.WriteLine("Error AST-->Nodo en funcion Variable_y_type no existente/detectado");
                    break;
            }
        }

        private void LstVars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "LSTVARS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        LstVars(tipoObj, hijo);
                    }
                    break;
                case "VARS":
                    Vars(tipoObj, Nodo);
                    break;
                default:
                    Console.WriteLine("Error AST-->Nodo en funcion LstVars no existente/detectado");
                    break;
            }
        }

        private void Vars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "VARS":
                    //LSTID + dospuntos + TIPODATO + puntocoma
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = new Retorno(td, getInicialDato(td), getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    //LSTID + dospuntos + TIPODATO + ToTerm("=") + CONDICION + puntocoma
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = Condicion(Nodo.ChildNodes[4]);
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    break;
                default:
                    Console.WriteLine("Error AST-->Nodo en funcion Vars no existente/detectado");
                    break;
            }
        }

        private void DeclaracionAsignacionData(string tipoObj, String tipodato, Retorno ret, ParseTreeNode Nodo)
        {
            //METODO PARA LA DECLARACION DE VARIABLES
            /*
             LSTID.Rule = LSTID + coma + id
                        | id
             */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "LSTID":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionAsignacionData(tipoObj, tipodato, ret, hijo);
                        }
                        break;

                    case "id":
                        String id = Nodo.Token.Value.ToString();

                        Console.WriteLine("LLEGO A RECONOCER LAS VARIABLES A DECLARAR PAPU");
                        Console.WriteLine("nombre variable: " + id);
                        Console.WriteLine("tipo objeto: " + tipoObj);
                        Console.WriteLine("tipo dato: " + tipodato);
                        Console.WriteLine("Valor asignable: " + ret.Valor.ToString());

                        if (!ExisteSimbolo(id))
                        {
                            if (ret != null)
                            {
                                if (ret.Tipo.Equals(tipodato)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                {
                                    Console.WriteLine("Se creo variable: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");
                                    //cima.addSimbolo(Reservada.varLocal, id, ret.Valor, tipodato, Reservada.var, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]), true, null);
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico-->Variable ya existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Variable ya existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                        }

                        break;

                    case ",": //No hace nada
                        break;

                    default:
                        Console.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado");
                        break;
                }
            }
            else
                Console.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado/null");
        }

        private Retorno Condicion(ParseTreeNode Nodo)
        {
            /*
            CONDICION.Rule = CONDICION + ToTerm("and") + COND1
                            | COND1
            COND1.Rule = COND1 + ToTerm("or") + COND2
                        | COND2
            COND2.Rule = ToTerm("not") + COND3
                        | COND3
            COND3.Rule = COND3 + ToTerm("<=") + COND4
                        | COND4
            COND4.Rule = COND4 + ToTerm(">=") + COND5
                        | COND5
            COND5.Rule = COND5 + ToTerm("<") + COND6
                        | COND6
            COND6.Rule = COND6 + ToTerm(">") + COND7
                        | COND7
            COND7.Rule = COND7 + ToTerm("=") + COND8
                        | COND8
            COND8.Rule = COND8 + ToTerm("<>") + EXPRESION
                        | EXPRESION
            */

            if (Nodo.ChildNodes.Count == 3)
            {
                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "CONDICION": // MakePlusRule(CONDICION, ToTerm("and"), COND1);
                        #region
                        Retorno condB1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condB2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condB1 != null) && (condB2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condB1.Tipo.Equals(Reservada.Booleano) && condB2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                if (condB1.Valor.Equals("True") && condB2.Valor.Equals("True")) // si ambos son true 
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else
                            {
                                Console.WriteLine("Imposible evaluar condicion AND con valores no booleanos");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion AND");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND1": // MakePlusRule(COND1, ToTerm("or"), COND2);
                        #region
                        Retorno condA1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condA2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condA1 != null) && (condA2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condA1.Tipo.Equals(Reservada.Booleano) && condA2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                if (condA1.Valor.Equals("False") && condA2.Valor.Equals("False")) // si ambos son false 
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                            }
                            else
                            {
                                Console.WriteLine("Imposible evaluar condicion OR con valores no booleanos");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion OR");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND3": // MakePlusRule(COND3, ToTerm("<="), COND4);
                        #region
                        Retorno condC1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condC2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condC1 != null) && (condC2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                double val1 = double.Parse(condC1.Valor);
                                double val2 = double.Parse(condC2.Valor);

                                if (val1 <= val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condC1.Tipo.Equals(Reservada.Cadena) && condC2.Tipo.Equals(Reservada.Cadena)))    //Si ambos son String
                            {
                                int v1 = getCantAscii(condC1.Valor);
                                int v2 = getCantAscii(condC2.Valor);

                                if (v1 <= v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion <= con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion <=");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND4": // MakePlusRule(COND4, ToTerm(">="), COND5);
                        #region
                        Retorno condD1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condD2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condD1 != null) && (condD2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                double val1 = double.Parse(condD1.Valor);
                                double val2 = double.Parse(condD2.Valor);

                                if (val1 >= val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condD1.Tipo.Equals(Reservada.Cadena) && condD2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condD1.Valor);
                                int v2 = getCantAscii(condD2.Valor);

                                if (v1 >= v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion >= con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion >=");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND5": // MakePlusRule(COND5, ToTerm("<"), COND6);
                        #region
                        Retorno condE1 = Condicion(Nodo.ChildNodes[0]);//COND6
                        Retorno condE2 = Condicion(Nodo.ChildNodes[2]);//COND7

                        if ((condE1 != null) && (condE2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condE1.Valor);
                                double val2 = double.Parse(condE2.Valor);

                                if (val1 < val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condE1.Tipo.Equals(Reservada.Cadena) && condE2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condE1.Valor);
                                int v2 = getCantAscii(condE2.Valor);

                                if (v1 < v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion < con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion < con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion <");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND6": // MakePlusRule(COND6, ToTerm(">"), COND7);
                        #region
                        Retorno condF1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condF2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condF1 != null) && (condF2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condF1.Valor);
                                double val2 = double.Parse(condF2.Valor);

                                if (val1 > val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno False
                                }
                            }
                            else if ((condF1.Tipo.Equals(Reservada.Cadena) && condF2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                int v1 = getCantAscii(condF1.Valor);
                                int v2 = getCantAscii(condF2.Valor);

                                if (v1 > v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion > con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion > con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion >");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND7": // MakePlusRule(COND7, ToTerm("="), COND8);
                        #region
                        Retorno condG1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condG2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condG1 != null) && (condG2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condG1.Valor);
                                double val2 = double.Parse(condG2.Valor);

                                if (val1 == val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else if ((condG1.Tipo.Equals(Reservada.Cadena) && condG2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condG1.Tipo.Equals(Reservada.Booleano) && condG2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condG1.Valor);
                                int v2 = getCantAscii(condG2.Valor);

                                if (v1 == v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion = con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion = con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion =");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion =", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND8": // MakePlusRule(COND8, ToTerm("<>"), EXPRESION);
                        #region
                        Retorno condH1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condH2 = Expresion(Nodo.ChildNodes[2]);

                        if ((condH1 != null) && (condH2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                double val1 = double.Parse(condH1.Valor);
                                double val2 = double.Parse(condH2.Valor);

                                if (val1 != val2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno True
                                }
                            }
                            else if ((condH1.Tipo.Equals(Reservada.Cadena) && condH2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condH1.Tipo.Equals(Reservada.Booleano) && condH2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                int v1 = getCantAscii(condH1.Valor);
                                int v2 = getCantAscii(condH2.Valor);

                                if (v1 != v2)
                                {
                                    return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno true
                                }
                                else
                                {
                                    return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])); //retorno false
                                }
                            }
                            else // valores no numericos
                            {
                                Console.WriteLine("Imposible evaluar condicion <> con valores diferentes");
                                Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <> con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Imposible evaluar condicion <>");
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <>", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                }
            }
            // ToTerm("not") + COND3
            else if (Nodo.ChildNodes.Count == 2)
            {
                #region
                Retorno condB1 = Condicion(Nodo.ChildNodes[1]);

                if (condB1 != null)
                {
                    if (condB1.Tipo.Equals(Reservada.Booleano)) // si es booleano
                    {
                        if (condB1.Tipo.Equals("True")) // si es true 
                        {
                            return new Retorno(Reservada.Booleano, "False", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno False
                        }
                        else
                        {
                            return new Retorno(Reservada.Booleano, "True", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])); //retorno True
                        }
                    }
                    else
                    {
                        Console.WriteLine("Imposible evaluar condicion NOT con valores no booleanos");
                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT con valores no booleanos", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Imposible evaluar condicion NOT");
                    Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                    lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                    return null;
                }
                #endregion
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("EXPRESION")) //EXPRESION
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
                else // COND1, COND2, COND3, COND4.... CONDX
                {
                    return Condicion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Expresion(ParseTreeNode Nodo)
        {
            /*
            EXPRESION.Rule = EXPRESION + mas + EXP1
                            | EXP1
            EXP1.Rule = EXP1 + menos + EXP2 
                        | EXP2
            EXP2.Rule = EXP2 + por + EXP3
                        | EXP3
            EXP3.Rule = EXP3 + division + EXP4
                        | EXP4
            EXP4.Rule = EXP4 + modulo + TERMINALES
                        | TERMINALES
            */
            if (Nodo.ChildNodes.Count == 3)
            {
                Retorno ra1 = Expresion(Nodo.ChildNodes[0]);
                Retorno ra2 = Expresion(Nodo.ChildNodes[2]);
                String linea1 = getLinea(Nodo.ChildNodes[1]);
                String colum1 = getColumna(Nodo.ChildNodes[1]);

                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "EXPRESION": // EXPRESION + mas + EXP1
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if (ra1.Tipo.Equals(Reservada.Cadena) || ra2.Tipo.Equals(Reservada.Cadena)) // Si alguno es String concateno
                            {
                                String concat = "";
                                //if (ra1.Tipo.Equals(Reservada.Cadena))
                                //{
                                //    concat = ra1.Valor + GetOperable(ra2);
                                //}
                                //else
                                //{
                                //    concat = GetOperable(ra1) + ra2.Valor;
                                //}
                                concat = GetOperable(ra1).Valor + GetOperable(ra2).Valor;
                                return new Retorno(Reservada.Cadena, concat, linea1, colum1);
                            }
                            else if (ra1.Tipo.Equals(ra2.Tipo) && !ra1.Tipo.Equals(Reservada.Cadena)) // Si ambos son del mismo tipo y distinto de Cadena
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);

                                if (ra1.Tipo.Equals(Reservada.Booleano))
                                {
                                    return new Retorno(Reservada.Booleano, suma + "", linea1, colum1);
                                }
                                else
                                {
                                    return new Retorno(ra1.Tipo, suma + "", linea1, colum1);
                                }
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real)) || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double suma = double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, suma + "", linea1, colum1);
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para suma linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para suma", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            if (ra2 == null)
                            {

                            }
                            Console.WriteLine("Error Semantico-->Expresion no operable(null) linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable(null)", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP1": // EXP1 + menos + EXP2
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double resta = double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero)) //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double resta = double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Console.WriteLine("Error Semantico--> Error al restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP2": // EXP2 + por + EXP3
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double mul = double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))     //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mul = double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Entero, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP3": // EXP3 + division + EXP4
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double div = double.Parse(GetOperable(ra1).Valor) / double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, div + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP4": // EXP4 + modulo + TERMINALES 
                        #region
                        ra2 = Terminales(Nodo.ChildNodes[2]); //Aca vuelvo a asignar a ra2 el valor del TERMINAL

                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mod = double.Parse(GetOperable(ra1).Valor) % double.Parse(GetOperable(ra2).Valor);
                                return new Retorno(Reservada.Real, mod + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Console.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                }
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("TERMINALES"))
                {
                    return Terminales(Nodo.ChildNodes[0]);
                }
                else
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Terminales(ParseTreeNode Nodo)
        {
            /*
             TERMINALES.Rule = numero
                            | real
                            | menos + numero // Especial coco a esto
                            | menos + real // Especial coco a esto
                            | cadena
                            | ToTerm("true")
                            | ToTerm("false")
                            | id // Esto puede ser un ARREGLO, OBJECT o ?
                            | id + parentA + parentC // Invocacion funcion sin parametros
                            | id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                            | id + corchA + ASIGNAR_PARAMETRO + corchC // Obteniendo valor de un array, condicion debe ser entero para acceder a esa posicion del arreglo
                            | parentA + CONDICION + parentC
             */
            switch (Nodo.ChildNodes.Count)
            {
                case 1:
                    //| numero
                    //| real
                    //| cadena
                    //| ToTerm("true")
                    //| ToTerm("false")
                    //| id // Esto puede ser un ARREGLO, OBJECT o ?
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "numero":
                            return new Retorno(Reservada.Entero, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "real":
                            return new Retorno(Reservada.Real, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "cadena":
                            return new Retorno(Reservada.Cadena, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "true":
                            return new Retorno(Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "false":
                            return new Retorno(Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "id": //Esto puede ser una VARIABLE o un ARREGLO

                            #region
                            String id = Nodo.ChildNodes[0].Token.Value.ToString();
                            Simbolo sim = RetornarSimbolo(id);

                            if (sim == null) //Si no existe en mi nivel actual busco en las globales
                            {
                                sim = cimaG.RetornarSimbolo(id);
                                Console.WriteLine(">>> Se busco en las globales <<<");
                            }

                            if (sim != null)
                            {
                                Console.WriteLine("alto ahi sr, esto esta comentado!");
                                return new Retorno(Reservada.Real, "5.5", "5", "5"); //BORRAR ESTE RETURN LUEGO
                                /*
                                if (sim.TipoObjeto.Equals(Reservada.arreglo))
                                {
                                    return new Retorno(Reservada.arreglo, id, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {
                                    return new Retorno(sim.Tipo, sim.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }*/
                            }
                            else
                            {
                                Console.WriteLine("Error Semantico-->Variable " + id + " no Existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                return null;
                            }
                            #endregion

                    }
                    #endregion
                    break;
                case 2:
                    //| menos + numero // Especial coco a esto
                    //| menos + real // Especial coco a esto
                    #region
                    switch (Nodo.ChildNodes[1].Term.Name)
                    {
                        case "numero":
                            Console.WriteLine(Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString() + " <<================= SE ESTA RETORNANDO UN NEGATIVO BIEN PRRON");
                            return new Retorno(Reservada.Entero, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                        case "real":
                            return new Retorno(Reservada.Real, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                    }
                    #endregion
                    break;
                case 3:
                    //| id + parentA + parentC // Invocacion funcion sin parametros
                    //| parentA + CONDICION + parentC
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "id": //| id + parentA + parentC // Invocacion funcion si parametros
                            /*
                            String id3 = Nodo.ChildNodes[0].Token.Value.ToString();
                            //Funciones func3 = tablafunciones.RetornarFuncion(id3);
                            Funciones func3 = tablafunciones.RetornarFuncionEvaluandoSobrecargaVoid(id3);

                            if (func3 != null)
                            {
                                if (!func3.getTipo().Equals(Reservada.Void)) //Si el metodo es de tipo VOID no retorna nada, ERROR
                                {
                                    nivelActual++; //Aumentamos el nivel actual ya que accedemos a otro metodo
                                    TablaSimbolos metodo4 = new TablaSimbolos(nivelActual, Reservada.Metodo, true, false, true);
                                    pilaSimbolos.Push(metodo4);
                                    cima = metodo4; //Estableciendo la tabla de simbolos cima

                                    Retorno reto = EjecutarFuncion(func3);

                                    nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                    cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                    return reto;// ORIGINALMENTE DEVOLVER ESTO PERRO
                                    //return new Retorno(Reservada.Cadena, nameUsuario, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Funcion no retorna valor linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no retorna valor", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;*/
                            return null; //BORRAR ESTHO!!!

                        case "(": //| parentA + CONDICION + parentC

                            Retorno ret = Condicion(Nodo.ChildNodes[1]);

                            if (ret != null)
                            {
                                return new Retorno(ret.Tipo, ret.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                            }
                            else
                            {
                                Console.WriteLine("Error Semantico-->Retornno de parentesis mala linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Retornno de parentesis mala", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;

                    }
                    #endregion
                    break;
                case 4:
                    //| id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                    //| id + corchA + CONDICION + corchC // Obteniendo valor de un arreglo, condicion debe ser entero para acceder a esa posicion del arreglo

                    break;

                default:
                    return null;

            }
            return null;
        }

        private Retorno GetOperable(Retorno retornable)
        {
            switch (retornable.Tipo)
            {
                case "Char": //Cambia a ascii
                    retornable.Valor = GetAscii(retornable.Valor) + "";
                    return retornable;

                case "Boolean": //Cambia a 0 o 1
                    if (retornable.Valor.Equals("True"))
                    {
                        retornable.Valor = "1";
                        return retornable;
                    }
                    else
                    {
                        retornable.Valor = "0";
                        return retornable;
                    }

                default:
                    return retornable;
            }
        }

        private string getInicialDato(string tipodato)
        {
            if (tipodato.Equals(Reservada.Entero))
            {
                return "0";
            }
            else if (tipodato.Equals(Reservada.Real))
            {
                return "0.0";
            }
            else if (tipodato.Equals(Reservada.Cadena))
            {
                return "\"\"";
            }
            else if (tipodato.Equals(Reservada.Booleano))
            {
                return "False";
            }
            return "";
        }

        private string getTipoDato(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "Integer":
                    return "Integer";

                case "Real":
                    return "Real";

                case "Boolean":
                    return "Boolean";

                case "String":
                    return "String";

                default:
                    return "null";

            }
        }

        private int getCantAscii(String cadena)
        {
            Char[] Caracter = cadena.ToCharArray();

            int SumaAscii = 0;

            for (int i = 0; i < Caracter.Length; i++)
            {
                SumaAscii += GetAscii(Caracter[i] + "");
            }
            return SumaAscii;
        }

        private int GetAscii(String caracter)
        {
            return Encoding.ASCII.GetBytes(caracter)[0];
        }

        private string getLinea(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Line + 1) + "";
        }

        private string getColumna(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Column + 1) + "";
        }

        //====================================================================================================== BUSQUEDAS AVANZADAS ===================================================================================

        private Boolean ExisteSimbolo(string nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        //foreach (Simbolo simbolo in ts.getTS())
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private Simbolo RetornarSimbolo(String nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return simbolo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private Boolean Vacio()
        {
            if (!pilaSimbolos.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
