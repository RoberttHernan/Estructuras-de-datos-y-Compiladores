using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Reactive;
using System.Text.Json.Serialization;
using Antlr4.Runtime;
using ReactiveUI;
using Newtonsoft.Json; 


namespace Compilador2
{

/// <summary>
/// ViewModel principal de la aplicacion (patron MVVM con ReactiveUI).
/// Expone las propiedades enlazadas en la vista XAML:
///   - InputText: codigo fuente escrito por el usuario.
///   - OutputList: lineas de salida producidas por fmt.Println.
///   - ErrorList: errores sintacticos o de ejecucion.
///
/// El comando AnalyzeCommand ejecuta el pipeline completo:
/// AntlrInputStream -> gramaticaLexer -> gramaticaParser -> Visitor.
/// </summary>
public class MainViewModel : ReactiveObject
{
    private string _inputText;
    public  string InputText{
        get => _inputText;
        set => this.RaiseAndSetIfChanged(ref _inputText, value);
    }


    private ObservableCollection<string> _outputList;
    public ObservableCollection<string> OutputList{
        get => _outputList;
        set => this.RaiseAndSetIfChanged(ref _outputList, value);
    }

    private ObservableCollection<string> _errorList;
     public ObservableCollection<string> ErrorList{
        get => _errorList;
        set => this.RaiseAndSetIfChanged(ref _errorList, value);
    }

    public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }

    public MainViewModel()
    {
        OutputList = new ObservableCollection<string>();
        ErrorList = new ObservableCollection<string>();

        
        AnalyzeCommand = ReactiveCommand.Create(Analyze);
    }

    private void Analyze(){
        OutputList.Clear();
        ErrorList.Clear();


        try{

            var inputStream = new AntlrInputStream(InputText);
        var lexer = new gramaticaLexer(inputStream);
        var tokens = new CommonTokenStream(lexer);
        var parser = new gramaticaParser(tokens);
        var visitor = new Visitor();
        var tree = parser.prog();
        visitor.Visit(tree);


        var astrooot = visitor.Visit(tree);
        foreach (var item in visitor.listaSalida)
        {
            Console.WriteLine(item);
            //OutputList.Add((string)item);
        }

        var astreport = JsonConvert.SerializeObject(astrooot, Formatting.Indented);
        Console.WriteLine(astreport);
        }catch (Exception e){
            Console.WriteLine(e.Message);
           // ErrorList.Add(e.Message);
        }
        
        
       // parser.RemoveErrorListeners();
        

}}}