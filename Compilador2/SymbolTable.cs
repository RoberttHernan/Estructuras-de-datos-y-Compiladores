using System;

public class SymbolTable
{
    private Dictionary<string, (object valor, string tipo)> variables = new Dictionary<string, (object, string)>();


/*
Funcion para declarar una variable en la tabla de simbolos
@param nombre: nombre de la variable    
@param valor: valor de la variable

*/
    public void DeclararVariable(string nombre, object valor, string? tipo = null)
    {
        if (variables.ContainsKey(nombre))
        {
            throw new Exception($"Variable '{nombre}' ya existe");
        }else if (valor == null)
        {
            throw new Exception($"Variable '{nombre}' no puede ser null");
        }else {
            variables[nombre] = (valor, tipo?? InferirTipo(valor));

        }
        
    }

/*
funcion para asignacion de valores a variables ya existentes 
@param nombre: nombre de la variable    
@param valor: valor de la variable

*/
   /* public void asignarVariable(string nombre, object valor)
    {
        if (!variables.ContainsKey (nombre)){
            throw new Exception($"Variable '{nombre}' no declarada");
        }

        var tipoActual = variables[nombre].tipo;
        var tipoNuevo = InferirTipo (valor);

        if (tipoActual!= tipoNuevo){
            valor = convertirTipo (valor,tipoActual);
        }
        variables [nombre] = (valor, tipoActual); 



    }*/

    public object GetVariable(string nombre)
    { 
        if (!variables.ContainsKey(nombre))
        {
        throw new Exception($"Variable '{nombre}' no existe");
            
        }
        return variables[nombre].valor;
    }
    /*public bool ExisteVariable(string nombre)
    {
        return variables.ContainsKey(nombre);
    }*/

    public string getTipoVariable (string nombre){
        if (!variables.ContainsKey (nombre)){
            throw new Exception ("Variable no declarada"); 
        }
        return variables[nombre].tipo;
    }

    public void ImprimirTabla (){
        Console.WriteLine ("Tabla de Simbolos:");

        foreach (var item in variables)
        {
            Console.WriteLine ($"{item.Key}: {item.Value}");
        }
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