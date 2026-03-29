/// <summary>
/// Administra la cadena de ambitos de ejecucion (scope stack).
/// Cada bloque, funcion o estructura de control que introduce nuevas variables
/// llama a pushScope() al entrar y PopScope() al salir.
///
/// La pila contiene diccionarios de (valor, tipo) indexados por nombre de variable.
/// Las busquedas y asignaciones recorren la pila desde el tope (scope mas interno)
/// hacia abajo para implementar el alcance lexico.
/// </summary>
public class ScopeManager
{
    // Pila de ambitos: el tope es el ambito mas interno (local)
    private Stack<Dictionary<string, (object valor, string tipo)>> scopes;

    public ScopeManager()
    {
        scopes = new Stack<Dictionary<string, (object valor, string tipo)>>();
        scopes.Push (new Dictionary<string, (object valor, string tipo)>());
    }

    /// <summary>Crea un nuevo ambito local y lo empuja a la pila.</summary>
    public void pushScope()
    {
        scopes.Push (new Dictionary<string, (object valor, string tipo)>()); 
    }


    /// <summary>
    /// Elimina el ambito mas interno de la pila.
    /// Lanza una excepcion si se intenta eliminar el ambito global (pila con un solo elemento).
    /// </summary>
    public void PopScope()
    {
        if (scopes.Count > 1){
            scopes.Pop ();
        }else{
            throw new Exception ("Error al eliminar ambito global"); 
        }
    }

    /// <summary>
    /// Declara una variable en el ambito actual.
    /// Lanza una excepcion si el nombre ya existe en el mismo ambito.
    /// </summary>
    public void DeclararVariable(string nombre, object valor, string tipo)
    {
     var scope_actual = scopes.Peek ();

     if (scope_actual.ContainsKey (nombre)){
        throw new Exception ($"La variable '{nombre}' ya existe en el ambito en el que fue declarada"); 
     }
     scope_actual[nombre] = (valor, tipo); 
    }

    /// <summary>
    /// Asigna un nuevo valor a una variable ya declarada, buscandola en todos los ambitos.
    /// Convierte el valor al tipo de la variable si son int/float64 compatibles.
    /// </summary>
    public void asignarVariable(string nombre, object valor)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey (nombre)){
                var tipoActual = scope[nombre].tipo; 
                var tipoNuevo = InferirTipo (valor); 

                if (tipoActual != tipoNuevo){
                    valor = convertirTipo (valor,tipoActual);
                }
                scope [nombre ] = (valor,tipoActual); 
            }
        }
    }

    /// <summary>
    /// Retorna el valor de una variable buscando desde el ambito mas interno hacia el global.
    /// Lanza una excepcion si la variable no existe en ningun ambito.
    /// </summary>
    public object GetVariable(string nombre)
    {
        foreach (var scope in scopes){
            if (scope.ContainsKey (nombre)){
                return scope[nombre].valor; 
            }
            
        }
        throw new Exception ("Variable no declarada: " + nombre); 
    }

    /// <summary>Retorna true si la variable esta declarada en algun ambito de la cadena.</summary>
    public bool ExisteVariable(string nombre)
    {
            foreach (var scope in scopes)
            {
                if (scope.ContainsKey(nombre)){
                    return true; 
                }
                           
            }
            return false; 
    }

    public void PrintScopes (){
        Console.WriteLine ("======Scope manager ==========="); 
        int level = scopes.Count; 
        foreach (var scope in scopes)
        { 
            Console.WriteLine($"Ámbito (nivel {level}):");
            foreach (var kvp in scope)
            {
               
                Console.WriteLine($"  {kvp.Key} = {kvp.Value.valor} ({kvp.Value.tipo})");
            }
            level --;
        }
        

    }

    /// <summary>
    /// Retorna el tipo declarado de una variable buscando en todos los ambitos.
    /// Lanza una excepcion si la variable no existe.
    /// </summary>
    public object getTipoVariable(string nombre)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey (nombre)){
                return scope[nombre].tipo;
                
            }
            
        }
        throw new Exception ("Variable no declarada"); 

       
        
    }

    private string InferirTipo (object valor){
        if (valor is int)
        {
            return "int";
        }else if (valor is string)
        {
            return "string";
        }else if (valor is bool)
        {
            return "bool";
        }else if (valor is float || valor is double)
        {
            return "float64";
        }else if (valor is char)
        {
            return "char";
        }else {
            throw new Exception ("Tipo de inferencia no soportada symbol table");
        }

    }

    private object convertirTipo (object valor, string tipo){
        if (valor is int && tipo == "float64"){
            return Convert.ToSingle (valor);
        } else if (valor is float && tipo == "int"){
            return Convert.ToInt32(valor);
        }
        else{
            throw new Exception ("Tipo de conversion no valida para variable"); 
        }
    }

}