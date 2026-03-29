# Compilador Go-like en C#

Interprete de recorrido de arbol (tree-walking interpreter) para un lenguaje con sintaxis inspirada en Go, implementado en C# con ANTLR4. La interfaz grafica esta construida con el framework Avalonia siguiendo el patron MVVM.

## Descripcion del Proyecto

El interprete toma codigo fuente en el lenguaje definido, lo analiza con un lexer/parser generado por ANTLR4, y ejecuta el arbol de parseo directamente mediante el patron Visitor. El resultado de la ejecucion (salidas de `fmt.Println`) y los errores de sintaxis se muestran en tiempo real en la interfaz grafica.

---

## Arquitectura

```
Entrada (codigo fuente en la UI)
       |
  [gramatica.g4]  --ANTLR4--> Generado/ (lexer + parser en C#)
       |
  Visitor.cs              (tree-walking interpreter + verificacion de tipos)
       |
  ScopeManager.cs         (pila de ambitos para alcance lexico)
       |
  MainViewModel.cs        (MVVM: enlaza la UI con el interprete)
       |
  MainWindow.axaml        (Avalonia UI: editor de codigo + consola de salida)
```

### Analisis Lexico y Sintactico (`Generado/`, `gramatica.g4`)

El lexer y parser son generados automaticamente por ANTLR4 a partir de la gramatica `gramatica.g4`. El `ErrorListener.cs` reemplaza el listener por defecto de ANTLR para acumular errores sintacticos en la coleccion observable del ViewModel.

### Visitor / Interprete (`Visitor.cs`)

Extiende `gramaticaBaseVisitor<object>` e implementa todos los metodos `Visit*` para recorrer y ejecutar el arbol. Maneja:
- Declaracion y asignacion de variables con verificacion de tipos y conversion implicita `int <-> float64`.
- Registro y llamada de funciones con parametros y tipos de retorno.
- Evaluacion de expresiones: aritmeticas, relacionales, logicas y unarias.
- Estructuras de control: `if/else if/else`, `for` clasico, `for` estilo Go, `switch/case/default`.
- Slices: declaracion literal, acceso por indice, asignacion por indice.
- Funciones embebidas: `fmt.Println`, `strconv.Atoi`, `strconv.ParseFloat`, `reflect.TypeOf`, `slices.Index`, `strings.Join`, `append`.

### Gestion de Ambitos (`ScopeManager.cs`)

Implementa una pila de diccionarios (`Stack<Dictionary<string, (object valor, string tipo)>>`). Cada bloque o funcion crea un nuevo ambito (`pushScope`) que se elimina al salir (`PopScope`). Las busquedas recorren la pila desde el ambito mas interno hacia el global.

### Interfaz Grafica (`MainWindow.axaml`, `MainViewModel.cs`)

- **Avalonia 11** con tema Fluent.
- **ReactiveUI**: el ViewModel expone comandos reactivos (`AnalyzeCommand`) y colecciones observables para salida y errores.
- **OutputListConverter**: convertidor XAML que transforma `ObservableCollection<string>` en texto multi-linea para los TextBox de solo lectura.

---

## Lenguaje Soportado

### Tipos de datos

| Tipo | Descripcion |
|---|---|
| `int` | Entero con signo |
| `float64` | Numero flotante |
| `string` | Cadena de caracteres |
| `bool` | Valor booleano (`true` / `false`) |
| `rune` | Caracter individual |
| `[]tipo` | Slice (arreglo dinamico) |

### Declaracion de variables

```go
var nombre int = 5         // con tipo y valor
nombre := 5                // inferencia de tipo
var nombre int             // sin valor inicial (usa valor por defecto)
```

### Funciones

```go
func nombre(param1 int, param2 string) int {
    return param1
}
```

### Estructuras de control

```go
// If
if condicion {
    // ...
} else if condicion2 {
    // ...
} else {
    // ...
}

// Switch
switch expresion {
case valor1:
    // ...
default:
    // ...
}

// For clasico
for i := 0; i < 10; i++ {
    // ...
}

// For estilo Go (while)
for condicion {
    // ...
}
```

### Funciones embebidas

| Llamada | Descripcion |
|---|---|
| `fmt.Println(...)` | Imprime en la consola de la UI |
| `strconv.Atoi(s)` | Convierte string a int |
| `strconv.ParseFloat(s)` | Convierte string a float64 |
| `reflect.TypeOf(x)` | Retorna el tipo del valor como string |
| `append(slice, val)` | Agrega un elemento al slice |
| `slices.Index(slice, val)` | Retorna el indice del valor en el slice (-1 si no existe) |
| `strings.Join(slice, sep)` | Concatena los elementos del slice con el separador |

### Comentarios

```go
// Comentario de una linea
/* Comentario de multiples lineas */
```

---

## Dependencias

- **.NET 8.0**
- **ANTLR4 Runtime** (`Antlr4.Runtime.Standard` v4.13.1)
- **Avalonia 11.2.5** (UI cross-platform)
- **ReactiveUI** / **System.Reactive** (MVVM)
- **Newtonsoft.Json** (serialización para depuracion del AST)

---

## Compilacion y Ejecucion

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

---

## Estructura del Repositorio

```
Compilador2/
+-- Visitor.cs                  # Interprete principal (patron Visitor)
+-- ScopeManager.cs             # Pila de ambitos de ejecucion
+-- SymbolTable.cs              # Tabla de simbolos auxiliar
+-- ErrorListener.cs            # Listener de errores ANTLR para la UI
+-- AstNode.cs                  # Estructura de nodo AST (referencia)
+-- MainViewModel.cs            # ViewModel MVVM (ReactiveUI)
+-- OutputListConverter.cs      # Convertidor XAML para listas de salida
+-- MainWindow.axaml            # Vista principal (Avalonia XAML)
+-- MainWindows.axaml.cs        # Code-behind de la ventana principal
+-- App.axaml / App.axaml.cs    # Aplicacion Avalonia y recursos globales
+-- Program.cs                  # Punto de entrada
+-- gramatica.g4                # Gramatica ANTLR4 del lenguaje
+-- Generado/                   # Lexer y parser generados por ANTLR4 (C#)
+-- pruebas/                    # Archivos de prueba del lenguaje
+-- entrada.txt                 # Ejemplo de codigo de entrada
+-- Proyecto1Compi2025.csproj   # Proyecto .NET
+-- .gitignore
+-- README.md
```
