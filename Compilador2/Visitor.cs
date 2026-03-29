using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Newtonsoft.Json.Bson;
using ReactiveUI;



/// <summary>
/// Visitor principal: recorre el arbol de parseo generado por ANTLR4 e interpreta
/// el codigo fuente directamente (tree-walking interpreter).
///
/// Responsabilidades:
/// - Declaracion y asignacion de variables con verificacion de tipos.
/// - Registro y llamada de funciones definidas por el usuario.
/// - Evaluacion de expresiones aritmeticas, relacionales y logicas.
/// - Ejecucion de estructuras de control: if/else, for, switch.
/// - Manejo de slices: declaracion, acceso por indice, append.
/// - Funciones embebidas: fmt.Println, strconv.Atoi, strconv.ParseFloat,
///   reflect.TypeOf, slices.Index, strings.Join.
///
/// El estado de ejecucion se gestiona con ScopeManager (pila de ambitos).
/// Los resultados de fmt.Println se acumulan en listaSalida para mostrarlos en la UI.
/// </summary>
class Visitor : gramaticaBaseVisitor<object>
{
    private ScopeManager scopeManager = new ScopeManager(); // pila de ambitos de ejecucion

    public List<object> listaSalida { get; } = new List<object>(); // salidas de fmt.Println

    private bool bandera_break = false;    // senaliza un 'break' activo dentro de un ciclo
    private bool bandera_continue = false; // senaliza un 'continue' activo dentro de un ciclo

    // Registro de funciones declaradas por el usuario
    private readonly Dictionary<string, Funcion> funciones = new Dictionary<string, Funcion>();

    // Palabras reservadas del lenguaje: no se pueden usar como nombres de funcion
    private readonly HashSet<string> nombresReservados = new HashSet<string>
    {
        "func", "main", "var", "int", "float64", "string", "bool", "rune", "slices", "Index", "strings", "Join",
        "append", "true", "false", "struct", "if", "else", "for", "switch", "case", "default", "range", "return",
        "break", "continue", "fmt", "Println", "strconv", "Atoi", "ParseFloat", "reflect", "TypeOf"
    };

    //-----------------------------------inicio de programa-----------------------------------
    public override object VisitProg([NotNull] gramaticaParser.ProgContext context)
    {


        //scopeManager.PrintScopes (); 
        if (context.lista_instrucciones() != null)
        {
            Visit(context.lista_instrucciones());
        }

        return true;
        /*if (context.lista_declaracion() != null)
        {

            foreach (gramaticaParser.Lista_declaracionContext declaracion in context.lista_declaracion())
            {
                Visit(declaracion);
            }
            foreach (gramaticaParser.Lista_sentenciaContext funcion in context.lista_sentencia())
            {
                Visit(funcion);
            }

        }*/

    }

    public override object VisitLista_instrucciones([NotNull] gramaticaParser.Lista_instruccionesContext context)
    {
        scopeManager.pushScope();

        if (context.lista_declaracion() != null)
        {

            foreach (var declaracion in context.lista_declaracion())
            {
                Visit(declaracion);
            }
            foreach (var sentencia in context.lista_sentencia())
            {
                Visit(sentencia);
            }
            /*foreach (gramaticaParser.Lista_sentenciaContext funcion in context.lista_sentencia())
             {
                 Visit(funcion);
             }*/

        }

        //scopeManager.PrintScopes();
        scopeManager.PopScope();

        return -999;

    }

    //-----------------------------------Sentecia y asignaciones-----------------------------------
    public override object VisitDeclaracionVariable([NotNull] gramaticaParser.DeclaracionVariableContext context)
    {

        // Obtener el identificador de la declaración de variable
        string nombre = context.IDENTIFICADOR().GetText();
        object valor = context.expresion() != null ? Visit(context.expresion()) : ValorporDefecto(context.tipo().GetText());
        string tipo = "";
        //Console.WriteLine(nombre);
        ////Console.WriteLine(valor.GetType());
        ///

        if (context.tipo() != null)
        {
            tipo = context.tipo().GetText();
        }




        if (tipo == "int" && valor.GetType().ToString() == "System.Single")
        {
            valor = Convert.ToInt32(valor);
        }
        else if (tipo == "float64" && valor.GetType().ToString() == "System.Int32")
        {
            valor = Convert.ToSingle(valor);
        }

        if (context.tipo() != null)
        {// si el tipo no es nulo


            if (!tipo.Contains("["))
            {
                if (!validarTipo(valor, tipo))
                {

                    throw new Exception($"Error de tipo en la variable '{nombre}'");
                }
            }
            scopeManager.DeclararVariable(nombre, valor, tipo);


        }
        else
        {
            if (context.expresion().GetRuleContext<gramaticaParser.DeclaracionSliceContext>(0) != null)
            {
                scopeManager.DeclararVariable(nombre, valor, tipo);
                return -9999;
            }
            string tipoInferidoValor = tipoInferido(valor);
            scopeManager.DeclararVariable(nombre, valor, tipoInferido(valor));
        }




        //scopeManager.PrintScopes();
        return -9999;


    }


    public override object VisitDeclaracionFuncion([NotNull] gramaticaParser.DeclaracionFuncionContext context)
    {
        var nombre = context.IDENTIFICADOR().GetText();

        if (nombresReservados.Contains(nombre))
        {
            throw new Exception($"El nombre de la función: '{nombre}' es una palabra reservada reservado.");
           
        }

        if (funciones.ContainsKey(nombre))
        {
            throw new Exception($"La función: '{nombre}' ya ha sido declarada.");
        }

        var parametros = context.lista_parametros()?.GetRuleContexts<gramaticaParser.ParametroContext>().Select(p => new Parametro(p.IDENTIFICADOR().GetText(), p.tipo().GetText())).ToList() ?? new List<Parametro>();
        var tipo_retorno = context.tipo()?.GetText();
        var cuerpo = context.lista_instrucciones();

        var funcion = new Funcion(nombre, parametros, tipo_retorno ?? string.Empty, cuerpo);
        funciones[nombre] = funcion;

        if (tipo_retorno != null)
        {
            Visit(cuerpo);
            if (!funcion.TieneRetorno)
            {
                throw new Exception($"La función: '{nombre}' no tiene una sentencia de retorno.");
            }
        }
        return -999;
    }

    public override object VisitSentenciaAsignacionComun([NotNull] gramaticaParser.SentenciaAsignacionComunContext context)
    {
        string nombre = context.IDENTIFICADOR().GetText();
        object valor = Visit(context.expresion());

        if (scopeManager.ExisteVariable(nombre))
        {
            var tipo_existente = scopeManager.getTipoVariable(nombre);

            if (tipo_existente == "int" && valor.GetType().ToString() == "System.Single")
            {
                valor = Convert.ToInt32(valor);
            }
            else if (tipo_existente == "float64" && valor.GetType().ToString() == "System.Int32")
            {
                valor = Convert.ToSingle(valor);
            }


        }
        scopeManager.asignarVariable(nombre, valor);


        return true;

    }

    public override object VisitSentenciaAsigacionconValor([NotNull] gramaticaParser.SentenciaAsigacionconValorContext context)
    {
        string nombre = context.IDENTIFICADOR().GetText();

        object valor = Visit(context.expresion());
        string operador = context.GetChild(1).GetText();
        if (scopeManager.ExisteVariable(nombre))
        {
            if (operador == "+=")
            {
                scopeManager.asignarVariable(nombre, operarValores(scopeManager.GetVariable(nombre), valor, operador));
            }
            else if (operador == "-=")
            {
                scopeManager.asignarVariable(nombre, operarValores(scopeManager.GetVariable(nombre), valor, operador));
            }




        }
        return 0;
    }

    public override object VisitSenteciaAux([NotNull] gramaticaParser.SenteciaAuxContext context)
    {
        scopeManager.pushScope();
        foreach (var declaracion in context.lista_declaracion())
        {
            Visit(declaracion);
        }
        foreach (var sentencia in context.lista_sentencia())
        {
            Visit(sentencia);
        }
        // scopeManager.PrintScopes();

        scopeManager.PopScope();
        return -999;


    }

    public override object VisitSentenciaIncremento([NotNull] gramaticaParser.SentenciaIncrementoContext context)
    {
        string name = context.IDENTIFICADOR().GetText();

        var valor = (int)scopeManager.GetVariable(name);


        if (context.op.Type == gramaticaParser.SIGNO_MENOS)
        {
            scopeManager.asignarVariable(name, valor - 1);
        }
        else if (context.op.Type == gramaticaParser.SIGNO_MAS)
        {
            scopeManager.asignarVariable(name, valor + 1);
        }

        return -99999;
    }


    public override object VisitSentenciaAsignacionSlice([NotNull] gramaticaParser.SentenciaAsignacionSliceContext context)
    {
        string nombre_slice = context.IDENTIFICADOR().GetText();

        object indice_valor = Visit(context.expresion(0));

        if (!(indice_valor is int))
        {
            throw new Exception("El índice debe ser un valor entero.");
        }

        int indice_aux = (int)indice_valor;

        object valor_asignar = Visit(context.expresion(1));


        var object_slice = scopeManager.getTipoVariable(nombre_slice);

        if (object_slice is List<object> slice)
        {
            if (indice_aux < 0 || indice_aux >= slice.Count)
            {
                throw new Exception("Índice fuera del rango del slice.");
            }
            slice[indice_aux] = valor_asignar;
        }
        else
        {
            throw new Exception("La variable no es un slice.");
        }

        return -999;
    }

    public override object VisitLlamadaFuncion([NotNull] gramaticaParser.LlamadaFuncionContext context)
    {
        //Console.WriteLine("Visitando llamadaFuncion" + context.GetText());

        return base.VisitLlamadaFuncion(context);
    }

    public override object VisitLlamadaFuncionCompleta([NotNull] gramaticaParser.LlamadaFuncionCompletaContext context)
    {


        if (context.funcion_embedida()!= null){
            string nombre = context.funcion_embedida().GetText();

            if (nombre == "fmt.Println")
        {
            //Console.WriteLine("Visitando print" + context.GetText());
            List<string> salida = new List<string>();

            List<object> argumentos = (List<object>)Visit(context.lista_argumentos());


            foreach (var argumento in argumentos)
            {
                if (argumento is List<object> slice)
                {
                    string aux = "";
                    aux = "Slice: " + string.Join(",", slice);
                    listaSalida.Add(aux);
                    return true;
                }

                listaSalida.Add(argumento);
                ////Console.WriteLine(argumento + "añsjkdhf");
            }

        }
        else if (nombre == "slices.Index")
        {
            List<object> argumentos = (List<object>)Visit(context.lista_argumentos());

            if (argumentos.Count != 2)
            {
                throw new Exception("slices.Index requiere exactamente dos argumentos: el slice y el valor a buscar.");
            }

            if (argumentos[0] is List<object> slice)
            {
                object valor_buscado = argumentos[1];

                for (int i = 0; i < slice.Count; i++)
                {
                    if (slice[i].Equals(valor_buscado))
                    {
                        return i;
                    }
                }
                return -1;
            }
            else
            {
                throw new Exception("El primer argumento de slices.Index debe ser un slice.");
            }





        }
        else if (nombre == "strings.Join")
        {
            List<object> argumentos = (List<object>)Visit(context.lista_argumentos());

            if (argumentos.Count != 2)
            {
                throw new Exception("string.Join requiere exactamente dos argumentos: el slice y el concatenador.");
            }

            if (argumentos[0] is List<object> slice)
            {
                List<string> string_aux_slice = new();
                foreach (var item in slice)
                {
                    if (!(item is string))
                    {
                        throw new Exception("Todos lo elementos del slice deben de ser de tipo slice");
                    }
                    string_aux_slice.Add((string)item);
                }
                string separador = (string)argumentos[1];
                string resultado = string.Join(separador, string_aux_slice);
                return resultado;

            }
            else
            {
                throw new Exception("El primer argumento de slices.Index debe ser un slice.");
            }


        }
        else if (nombre == "append")
        {
            List<object> argumentos = (List<object>)Visit(context.lista_argumentos());
            object valor_agregar = argumentos[1];

            if (argumentos.Count != 2)
            {
                throw new Exception("append requiere exactamente dos argumentos: el slice y el concatenador.");
            }

            if (argumentos[0] is List<object> slice)
            {

                slice.Add(valor_agregar);
                return slice;

            }
            else
            {
                throw new Exception("El primer argumento de append no es un slice");
            }

        }
        }
        



        


        return true;
    }


    public override object VisitListaArgumentos([NotNull] gramaticaParser.ListaArgumentosContext context)
    {
        ////Console.WriteLine("Visitando listaArgumentos" + context.GetText());
        List<object> resultados = new List<object>();
        foreach (var expr in context.expresion())
        {

            resultados.Add(Visit(expr));
        }
        return resultados;
    }

    public override object VisitFuncionPrintln([NotNull] gramaticaParser.FuncionPrintlnContext context)
    {
        // ////Console.WriteLine("Visitando funcionPrintln" + context.GetText());

        return base.VisitFuncionPrintln(context);

    }


    //---------------------------------Sentecias de control ---------------------------------


    public override object VisitSentenciaForComun([NotNull] gramaticaParser.SentenciaForComunContext context)
    {


        scopeManager.pushScope();

        //declaracion clasica de for: for (inicialización; condición; post) { ... }
        //inicializacion que puede ser una declaracion o una asignacion 
        if (context.declaracion_variable() != null)
        {
            Visit(context.declaracion_variable());
        }
        else if (context.sentencia_asignacion() != null)
        {
            foreach (var asignacion in context.sentencia_asignacion())
            {
                Visit(asignacion);
            }
        }
        else if (context.expresion() != null)
        {
            Visit(context.expresion(0)); //en caso de que la inicializacion sea una expresion 
        }


        bool condicion()
        {
            //si hay inicializacion, asumimos que la condicion viene en la siguiente expresion
            //si hay declaracion de variable o asignacion visitamos expresion (1) de lo contrario no hay iniicializacion y visitamos expresion 0
            int pointer_condicion = (context.declaracion_variable() != null || context.sentencia_asignacion() != null) ? 1 : 0;
            if (context.expresion() != null)
            {
                var cond = Visit(context.expresion(0));
                if (cond is bool b)
                    return b;
                throw new Exception("La condicion debe de evaluar a un valor Booleano");
            }

            return true;
        }

        void postAction()
        {
            if (context.sentencia_asignacion() != null)
            {
                Visit(context.sentencia_asignacion(0));
            }
            else if (context.expresion() != null && context.expresion().Length > 1)
            {
                Visit(context.expresion(1));
            }
        }

        while (condicion())
        {
            Visit(context.lista_instrucciones());

            postAction();

            if (bandera_break)
            {
                bandera_break = false;
                break;
            }
            if (bandera_continue)
            {
                bandera_continue = false;
                continue;

            }
            //ciclando la condicion
            /* while (EvaluarCondicion(context.expresion(0)))
             {
                 Visit(context.lista_instrucciones());

                 if (bandera_break)
                 {
                     bandera_break = false;
                     break;
                 }
                 if (bandera_continue){
                     bandera_continue = false; 
                     continue; 
                 }
                 if (context.declaracion_variable()!= null){
                     Visit (context.declaracion_variable()); 
                 }else if (context.sentencia_asignacion(1)!= null){
                     Visit (context.sentencia_asignacion(1)); 
                 }
             }*/
        }

        scopeManager.PopScope();


        return -00000;
    }

    public override object VisitSentenciaForEstiloGo([NotNull] gramaticaParser.SentenciaForEstiloGoContext context)
    {
        scopeManager.pushScope();

        if (context.expresion() != null)
        {
            Visit(context.expresion());
        }


        bool condicion()
        {
            if (context.expresion() != null)
            {
                var cond = Visit(context.expresion());
                if (cond is bool b)
                {
                    return b;
                }
                throw new Exception("La condicion en for debe de evaluar un valor boleano");
            }
            return true;
        }

        while (condicion())
        {
            Visit(context.lista_instrucciones());

            if (bandera_break)
            {
                bandera_break = false;
                break;
            }
            if (bandera_continue)
            {
                bandera_continue = false;
                continue;
            }
        }
        scopeManager.PopScope();
        return base.VisitSentenciaForEstiloGo(context);

    }

    public override object VisitSentencia_if([NotNull] gramaticaParser.Sentencia_ifContext context)
    {
        //evaluando las condiciones del bloque if

        if (EvaluarCondicion(context.bloque_if().expresion()))
        {
            Visit(context.bloque_if().lista_instrucciones());

        }
        else
        {
            //evaluando las istrucciones de los bloques if_else
            foreach (var bloque__else_if in context.bloque_if_else())
            {
                if (EvaluarCondicion(bloque__else_if.expresion()))
                {
                    Visit(bloque__else_if.lista_instrucciones());
                    return true;
                }
            }


            if (context.bloque_else() != null)
            {
                Visit(context.bloque_else().lista_instrucciones());

            }

            return true;
        }

        return true;
    }

    public override object VisitBloque_if([NotNull] gramaticaParser.Bloque_ifContext context)
    {

        return base.VisitBloque_if(context);
    }
    private bool EvaluarCondicion(gramaticaParser.ExpresionContext expr)
    {

        var result = Visit(expr);

        if (result is bool v)
        {
            return v;
        }
        else
        {
            throw new Exception("Error en la condicion del if");
        }
    }



    public override object VisitSentencia_switch([NotNull] gramaticaParser.Sentencia_switchContext context)
    {
        var expresion_switch = Visit(context.expresion());
        foreach (var caso_bloque in context.bloque_caso())
        {
            var caso_expresion = Visit(caso_bloque.expresion());
            if (Equals(expresion_switch, caso_expresion))
            {
                scopeManager.pushScope();
                foreach (var bloque in context.bloque_caso())
                {
                    foreach (var declaracion in bloque.lista_declaracion())
                    {
                        Visit(declaracion);

                        if (bandera_break)
                        {
                            bandera_break = false;
                            return -99999;
                        }
                    }
                    foreach (var sentencia in bloque.lista_sentencia())
                    {
                        Visit(sentencia);
                        if (bandera_break)
                        {
                            bandera_break = false;
                            return -99999;
                        }
                    }



                    scopeManager.PopScope();
                    return -999;
                }

            }

            if (context.bloque_default != null)
            {
                scopeManager.pushScope();
                Visit(context.bloque_default());
                scopeManager.PopScope();
            }







        }
        return -99999;
    }
    public override object VisitBloque_caso([NotNull] gramaticaParser.Bloque_casoContext context)
    {

        return base.VisitBloque_caso(context);
    }
    public override object VisitSentencia_break([NotNull] gramaticaParser.Sentencia_breakContext context)
    {

        bandera_break = true;
        return -999999999999;
    }

    public override object VisitSentecia_return([NotNull] gramaticaParser.Sentecia_returnContext context)
    {
        var valor_retorno = Visit(context.expresion());
        listaSalida.Add(valor_retorno);
        Console.WriteLine(valor_retorno);
        return valor_retorno;
    }


    public override object VisitSentenciaReturn([NotNull] gramaticaParser.SentenciaReturnContext context)
    {
        return base.VisitSentenciaReturn(context);
    }
    public override object VisitSentenciaContinue([NotNull] gramaticaParser.SentenciaContinueContext context)
    {
        bandera_continue = true;

        return -999;
    }

    //------------------Estructuras de datos -----------------------------------------------




    public override object VisitDeclaracionSlice([NotNull] gramaticaParser.DeclaracionSliceContext context)
    {
        var literal = context.lista_slice();
        List<object> slice_elementos = new();


        if (context.lista_slice() != null)
        {
            foreach (var expr in context.lista_slice().expresion())
            {
                slice_elementos.Add(Visit(expr));
            }
        }



        return slice_elementos;

    }


    //-----------------------------------Expresiones-----------------------------------

    public object VisitexpresionIdentificador([NotNull] gramaticaParser.ExpresionIdentificadorContext context)
    {
        ////Console.WriteLine("Visitando expresionIdentificador" + context.GetText());
        string nombre = context.IDENTIFICADOR().GetText();
        return scopeManager.GetVariable(nombre);
    }

    public override object VisitExpresionAccesoSlice([NotNull] gramaticaParser.ExpresionAccesoSliceContext context)
    {
        string slice_nombre = context.IDENTIFICADOR().GetText();
        object indice_slice = Visit(context.expresion());

        if (!(indice_slice is int))
        {
            throw new Exception("El indice para acceder a un slice debe de ser de tipo int");
        }
        int indice_aux = (int)indice_slice;

        var valor_slice = scopeManager.GetVariable(slice_nombre);

        if ((valor_slice is List<object> slice))
        {
            if (indice_aux < 0 || indice_aux >= slice.Count)
            {
                throw new Exception("indice fuera del rango del slice ()");
            }

            return slice[indice_aux];
        }
        else
        {
            throw new Exception("La variable a la que intenta acceder no es un slice");
        }
    }
    public override object VisitExpresionParentesis([NotNull] gramaticaParser.ExpresionParentesisContext context)
    {
        //////Console.WriteLine("Visitando expresionParentesis" + context.GetText());
        return Visit(context.expresion());
    }
    public override object VisitExpresionSumaResta([NotNull] gramaticaParser.ExpresionSumaRestaContext context)
    {


        string operador = context.op.Text;
        dynamic izquierda = Visit(context.expresion(0));
        dynamic derecha = Visit(context.expresion(1));

        if (izquierda is string || derecha is string)
        {
            if (operador == "+")
            {
                return izquierda.ToString() + derecha.ToString();
            }
            else
            {
                throw new Exception("error al concatenar cadena");
            }
        }
        return operador switch
        {
            "+" => izquierda + derecha,////Console.WriteLine("Suma: " + (izquierda + derecha));
            "-" => izquierda - derecha,////Console.WriteLine("Resta: " + (izquierda - derecha));
            _ => -999999999,
        };
    }

    public override object VisitExpresionMultiDiv([NotNull] gramaticaParser.ExpresionMultiDivContext context)
    {
        string operador = context.op.Text;
        dynamic izquierda = Visit(context.expresion(0));
        dynamic derecha = Visit(context.expresion(1));

        return operador switch
        {
            "*" => izquierda * derecha,////Console.WriteLine("Multiplicacion: " + (izquierda * derecha));
            "/" => izquierda / derecha,////Console.WriteLine("Division: " + (izquierda / derecha));
            "%" => izquierda % derecha,////Console.WriteLine("Modulo: " + (izquierda % derecha));
            _ => -999999999,
        };
    }

    public override object VisitExpresionRelacional([NotNull] gramaticaParser.ExpresionRelacionalContext context)
    {
        bool resultado = true;

        string operador = context.op.Text;
        dynamic izquierda = Visit(context.expresion(0));
        dynamic derecha = Visit(context.expresion(1));

        if (operador == "<")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda < derecha));
            resultado = izquierda < derecha;
        }
        else if (operador == ">")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda > derecha));
            resultado = izquierda > derecha;

        }
        else if (operador == "<=")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda <= derecha));
            resultado = izquierda <= derecha;
        }
        else if (operador == ">=")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda >= derecha));
            resultado = izquierda >= derecha;
        }

        return resultado;
    }
    public override object VisitExpresionIgualdad([NotNull] gramaticaParser.ExpresionIgualdadContext context)
    {
        bool resultado = true;
        string operador = context.op.Text;
        dynamic izquierda = Visit(context.left);
        dynamic derecha = Visit(context.right);

        if (operador == "==")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda == derecha));
            resultado = izquierda == derecha;
        }
        else if (operador == "!=")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda != derecha));
            resultado = izquierda != derecha;
        }

        return resultado;
    }

    public override object VisitExpresionNegacion([NotNull] gramaticaParser.ExpresionNegacionContext context)
    {
        var operador = context.op.Text;

        if (operador == "-") // solo operadores aritmeticos
        {
            dynamic izquierda = Visit(context.expresion());
            //Console.WriteLine("Negacion: " + -izquierda);
            return -izquierda;
        }
        else if (operador == "!") // solo operadores boleanos
        {
            bool izquierda = (bool)Visit(context.expresion());
            //Console.WriteLine("Negacion: " + !izquierda);
            return !izquierda;
        }

        return -999999999;

    }

    public override object VisitExpresionLogica([NotNull] gramaticaParser.ExpresionLogicaContext context)
    {
        bool resultado = true;
        string operador = context.op.Text;
        bool izquierda = (bool)Visit(context.expresion(0));
        bool derecha = (bool)Visit(context.expresion(1));

        if (operador == "&&")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda && derecha));
            resultado = izquierda && derecha;
        }
        else if (operador == "||")
        {
            ////Console.WriteLine("Comparacion: " + (izquierda || derecha));
            resultado = izquierda || derecha;
        }

        return resultado;
    }
    public override object VisitExpresionInt([NotNull] gramaticaParser.ExpresionIntContext context)
    {
        return int.Parse(context.INT().GetText());
    }

    public override object VisitExpresionIdentificador([NotNull] gramaticaParser.ExpresionIdentificadorContext context)
    {
        string nombre = context.IDENTIFICADOR().GetText();
        return scopeManager.GetVariable(nombre);



    }

    public override object VisitExpresionCadena([NotNull] gramaticaParser.ExpresionCadenaContext context)
    {
        ////Console.WriteLine("Visitando expresionCadena" + context.GetText());
        return context.CADENA().GetText().Trim('"');
    }

    public override object VisitExpresionRune([NotNull] gramaticaParser.ExpresionRuneContext context)
    {
        return context.RUNE().GetText().Trim('\'')[0];
    }

    public override object VisitExpresionFloat([NotNull] gramaticaParser.ExpresionFloatContext context)
    {
        return float.Parse(context.FLOAT().GetText());
    }

    public override object VisitExpresionFalse([NotNull] gramaticaParser.ExpresionFalseContext context)
    {
        return false;
    }

    public override object VisitExpresionTrue([NotNull] gramaticaParser.ExpresionTrueContext context)
    {
        return true;
    }

    public object ValorporDefecto(string tipo)
    {
        return tipo switch
        {
            "int" => 0,
            "bool" => false,
            "float64" => 0.0,
            "string" => "",
            "rune" => ' ',
            _ => throw new Exception("Tipo no soportado"),
        };
    }


    //-----------------------------------------Funciones varias-----------------------------------------

    private bool validarTipo(object valor, string tipo)
    {
        return tipo switch
        {
            "int" => valor is int,
            "float64" => valor is float || valor is double,
            "string" => valor is string,
            "bool" => valor is bool,
            "rune" => valor is char,
            _ => throw new Exception("Tipo no soportado"),
        };
    }

    private string InferirTipoSlice(List<object> slice)
    {
        if (slice.Count == 0)
        {
            throw new Exception("no es posible inferir tipo de slice vacio");
        }
        string tipo_comun = tipoInferido(slice[0]);

        foreach (var item in slice)
        {
            string tipo_actual = tipoInferido(item);
            if (tipo_actual != tipo_comun)
            {
                throw new Exception("Los elemento en el slice no son del mismo valor");
            }
        }

        return tipo_comun;
    }
    private string tipoInferido(object valor)
    {

        if (valor is int)
        {
            return "int";
        }
        else if (valor is string)
        {
            return "string";
        }
        else if (valor is bool)
        {
            return "bool";
        }
        else if (valor is float || valor is double)
        {
            return "float64";
        }
        else if (valor is char)
        {
            return "rune";
        }
        else
        {
            throw new Exception("Tipo de inferencia no soportada");
        }

    }

    private object convertirTipo(object valor, string tipo)
    {
        if (valor is int && tipo == "float64")
        {
            return Convert.ToSingle(valor);
        }
        else if (valor is float && tipo == "int")
        {
            return Convert.ToInt32(valor);
        }
        else
        {
            throw new Exception("Tipo de conversion no valida para variable----------");
        }
    }

    private object operarValores(object izquierdo, object derecho, string operador)
    {
        if (operador == "+=")
        {

            if (izquierdo is int && derecho is int)
            {
                return (int)izquierdo + (int)derecho;
            }
            else if (izquierdo is int && derecho is float)
            {
                throw new Exception("No posible operar un entero con un flotante");
            }
            else if (izquierdo is float && derecho is float)
            {
                return (float)izquierdo + (float)derecho;
            }
            else if (izquierdo is float && derecho is int)
            {
                return (float)izquierdo + Convert.ToSingle(derecho);
            }
            else if (izquierdo is string && derecho is string)
            {
                return (string)izquierdo + (string)derecho;
            }
            else
            {
                throw new Exception("Tipos no compatibles para la suma");
            }
        }
        else if (operador == "-=")
        {
            if (izquierdo is int && derecho is int)
            {
                return (int)izquierdo - (int)derecho;
            }
            else if (izquierdo is int && derecho is float)
            {
                throw new Exception("No posible operar un entero con un flotante");
            }
            else if (izquierdo is float && derecho is float)
            {
                return (float)izquierdo - (float)derecho;
            }
            else if (izquierdo is float && derecho is int)
            {
                return (float)izquierdo - Convert.ToSingle(derecho);
            }
            else
            {
                throw new Exception("Tipos no compatibles para la resta");
            }
        }

        throw new Exception("Operador no soportado");
    }

    public ScopeManager getScopeManager()
    {
        return scopeManager;
    }


    public class Funcion (){
        public string Nombre {get; set;}
        public List<Parametro> Parametros {get;}
        public string TipoRetorno {get; }
        public gramaticaParser.Lista_instruccionesContext Cuerpo {get; }

        public bool TieneRetorno {get; private set;}

        public Funcion(string nombre, List<Parametro> parametros, string tipo_retorno, gramaticaParser.Lista_instruccionesContext cuerpo) : this() {
            Nombre = nombre;
            Parametros = parametros ?? new List<Parametro>();
            TipoRetorno = tipo_retorno;
            Cuerpo = cuerpo;
            
        }
        
    }

    public  class Parametro{
        public string Nombre {get; set;}
        public string Tipo {get; set;}

        public Parametro (string nombre, string tipo){
            Nombre = nombre;
            Tipo = tipo;
        }
    }
}