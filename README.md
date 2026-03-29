# Estructuras de Datos

Coleccion de proyectos que implementan estructuras de datos fundamentales en C++. Cada proyecto aplica multiples estructuras en un contexto practico y funcional, demostrando su uso real mas alla de implementaciones aisladas. El objetivo del repositorio es servir como portafolio tecnico que evidencie el dominio de estructuras lineales, no lineales y sus algoritmos asociados.

## Proyectos

### Compilador Go-like en C# (`Compilador2/`)

Interprete de recorrido de arbol para un lenguaje con sintaxis inspirada en Go, implementado en C# con ANTLR4 y Avalonia (UI cross-platform). Demuestra la construccion de un frontend completo de compilador con interfaz grafica.

Tecnologias y conceptos implementados:
- **ANTLR4**: generacion automatica del lexer y parser en C# desde una gramatica `.g4`.
- **Patron Visitor**: recorrido del arbol de parseo para ejecutar el codigo directamente.
- **Pila de ambitos** (`ScopeManager`): manejo de alcance lexico con verificacion y conversion de tipos.
- **Avalonia + ReactiveUI**: interfaz grafica MVVM con editor de codigo, consola de salida y lista de errores.
- Tipos soportados: `int`, `float64`, `string`, `bool`, `rune`, slices.
- Funciones embebidas: `fmt.Println`, `strconv`, `reflect`, `slices`, `strings`.

Lenguaje: C# .NET 8 | Dependencias: ANTLR4 Runtime, Avalonia 11, ReactiveUI

---

### Interprete V-Lang Cherry en Go (`Interprete Vlang Cherry/`)

Interprete de recorrido de arbol (tree-walking interpreter) para un lenguaje de programacion personalizado con sintaxis inspirada en V-Lang. Demuestra el pipeline completo de un compilador/interprete: analisis lexico, sintactico y semantico, construccion de AST y evaluacion.

Tecnologias y conceptos implementados:
- **ANTLR4**: generacion automatica del lexer y parser en Go a partir de una gramatica `.g4`.
- **Patron Visitor**: transformacion del CST de ANTLR a un AST personalizado.
- **Interprete de recorrido de arbol**: evaluacion del AST con alcance lexico estatico mediante cadena de entornos.
- **Generacion de codigo ARM64**: emision de ensamblador a partir del AST.
- **Interfaz grafica con Fyne**: editor de codigo integrado con consola y reportes visuales.
- **Reportes**: visualizacion del AST en Graphviz y tabla de simbolos en CSV/PDF.

Lenguaje: Go 1.22 | Dependencias: ANTLR4 runtime, Fyne v2, gofpdf

---

### Sistema de Archivos en C++ (`P2/`)

Simulador de sistema de archivos que opera sobre discos virtuales (archivos binarios). Implementa los formatos ext2 y ext3 con particiones primarias, extendidas y logicas administradas mediante MBR y EBR. Incluye un interprete de comandos propio con analizador lexico (maquina de estados finita) y parser.

Estructuras y conceptos implementados:
- **MBR / EBR**: structs serializados directamente en disco con `fwrite/fread`.
- **SuperBloque, Inodos y Bloques**: tabla de inodos, bloques de carpeta, archivo y apuntador.
- **Bitmaps**: asignacion de inodos y bloques con algoritmos First-Fit, Best-Fit y Worst-Fit.
- **Journaling**: registro de operaciones para particiones ext3.
- **Lista simplemente enlazada**: registro de particiones montadas en sesion.
- **Analizador lexico**: maquina de estados finita de 6 estados para tokenizar comandos.

Lenguaje: C++11 | Dependencias: Graphviz

---

### Scrabble en C++ (`Scrabble C++/`)

Implementacion del juego de mesa Scrabble para dos jugadores en consola. El proyecto integra cinco estructuras de datos personalizadas para representar cada componente del juego:

- **Matriz Dispersa** (lista ortogonal enlazada): tablero de juego NxN que solo almacena casillas con contenido.
- **Arbol Binario de Busqueda (ABB)**: registro de jugadores ordenado lexicograficamente por nombre de usuario.
- **Cola FIFO**: bolsa de fichas con la distribucion oficial del Scrabble en espanol (~95 fichas).
- **Lista Doblemente Enlazada Circular**: mano de cada jugador (7 fichas), con iteracion continua.
- **Lista Simple**: almacenamiento auxiliar para casillas especiales, jugadas en curso e historial de puntajes.

La configuracion del tablero (dimension, casillas especiales, diccionario) se carga desde un archivo JSON externo usando la libreria `nlohmann/json`. El sistema genera reportes graficos de las estructuras internas en formato Graphviz.

Lenguaje: C++11 | Dependencias: Graphviz, nlohmann/json (incluida)
