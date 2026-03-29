using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Atn;
using System.Collections.ObjectModel;

namespace Compilador2
{
    /// <summary>
    /// Listener de errores sintacticos para ANTLR4.
    /// Reemplaza el DefaultErrorListener para acumular los mensajes de error
    /// en la coleccion observable del ViewModel en lugar de imprimirlos en consola.
    /// </summary>
    public class ErrorListener : BaseErrorListener
    {
        private readonly ObservableCollection<string> _errorList;

        public ErrorListener(ObservableCollection<string> errorList)
        {
            _errorList = errorList;
        }

        /// <summary>
        /// Invocado automaticamente por ANTLR cuando detecta un error sintactico.
        /// Formatea el mensaje con linea y columna y lo agrega a la lista de errores.
        /// </summary>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            _errorList.Add($"Error de sintaxis en linea {line}:{charPositionInLine} - {msg}");
        }
    }
}

