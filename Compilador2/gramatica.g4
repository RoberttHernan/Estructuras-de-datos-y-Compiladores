grammar gramatica;

//--palabras reservadas-------------------

RESFUNC : 'func';
RESMAIN : 'main';


RESVAR : 'var';
RESINT : 'int';
RESFLOAT : 'float64';
RESSTRING : 'string';

RESBOOL :   'bool';
RESRUNE : 'rune';
RESSLICE : 'slices';
RESINDEX : 'Index'; 
RESSTRINGS : 'strings';
RESJOIN: 'Join'; 
RESAPPEND: 'append';
RESTRUE : 'true';
RESFALSE : 'false';
RESSTRUCT : 'struct';

RESIF : 'if';
RESELSE : 'else';
RESFOR : 'for';
RESSWITCH : 'switch';
RESCASE : 'case';
RESDEFAULT : 'default';
RESRANGE : 'range';
RESRETURN : 'return';
RESBREAK : 'break';
RESCONTINUE : 'continue';

RESFMT : 'fmt';
RESPRINTLN : 'Println';
RESSTR: 'strconv';
RESATOI : 'Atoi';
RESPARSEFLOAT : 'ParseFloat';
RESREFLECT : 'reflect';
RESTYPEOF : 'TypeOf';



//-------tokens para lexer----------------

IDENTIFICADOR : ('_')?[a-zA-Z_][a-zA-Z_0-9]*;
INT :  [0-9]+;
FLOAT : [0-9]+'.'[0-9]+;
CADENA : '"' (~["\r\n])* '"';
RUNE :  '\'' . '\'';


ESPACIO : [ \t\r\n]+ -> skip;
COMENTARIO : ('//' ~[\r\n]* 
            | '/*' .*? '*/') 
            -> skip;





//-----tokens para signos-------------------
PARABRE : '(';
PARCIERRA : ')';
LLAVEABRE : '{';
LLAVECIERRA : '}';
CORABRE :'[';
CORCIERRA :']';
PUNTO_COMA : ';';
DOS_PUNTOS : ':';
SIGNO_MULTI : '*';
SIGNO_DIV : '/';
SIGNO_ASIGNACION_INCREMENTO : '+=';
SIGNO_ASIGNACION_DECREMENTO : '-=';
SIGNO_MAS : '+';
SIGNO_MENOS : '-';
SIGNO_AND : '&&';
SIGNO_OR : '||';
SIGNO_NO_IGUAL : '!=';
SIGNO_NOT : '!';
SIGNO_IGUALDAD : '==';
SIGNO_ASIGNACION : '=';
SIGNO_MENOSQUE_IGUAL: '<=';
SIGNO_MENOSQUE : '<';
SIGNO_MASQUE_IGUAL : '>=';
SIGNO_MASQUE : '>';
SIGNO_COMA : ',';
SIGNO_MODULO : '%';
PUNTO : '.';








//--inicio del programa

prog : RESFUNC RESMAIN PARABRE  PARCIERRA   lista_instrucciones EOF;


///////// ------------------------------------------------Declaracion de variables ------------------------------------
lista_declaracion : declaracion_variable  
                    | declaracion_funcion 
                    ;

declaracion_variable : RESVAR IDENTIFICADOR tipo  ( SIGNO_ASIGNACION expresion)? (PUNTO_COMA)? #declaracionVariable
                    | IDENTIFICADOR DOS_PUNTOS SIGNO_ASIGNACION expresion (PUNTO_COMA)? #declaracionVariable 
                    ;

declaracion_funcion : RESFUNC IDENTIFICADOR PARABRE (lista_parametros)? PARCIERRA tipo?  lista_instrucciones
#declaracionFuncion
 ;

//bloque de sentencias y declaracion agrupadas por {}
lista_instrucciones : LLAVEABRE (lista_sentencia|lista_declaracion)* LLAVECIERRA;


//----------------------------------------------------sentencias y asignaciones----------------

lista_sentencia : sentencia_asignacion #sentenciaAsignacion
               | sentencia_if #sentenciaIf
                 | sentencia_for #sentenciaFor
                | sentencia_switch #sentenciaSwitch
                | sentecia_return #sentenciaReturn
                |llamada_funcion #llamadaFuncion
                |sentencia_continue #sentenciaContinue
                |sentencia_break #sentenciaBrea
                |LLAVEABRE (lista_sentencia|lista_declaracion)* LLAVECIERRA #senteciaAux
                ;

sentencia_asignacion : IDENTIFICADOR (SIGNO_ASIGNACION_INCREMENTO | SIGNO_ASIGNACION_DECREMENTO)  expresion (PUNTO_COMA)? #sentenciaAsigacionconValor
                    | IDENTIFICADOR SIGNO_ASIGNACION  expresion (PUNTO_COMA)? #sentenciaAsignacionComun
                    | IDENTIFICADOR (op=SIGNO_MAS SIGNO_MAS |op=SIGNO_MENOS SIGNO_MENOS) PUNTO_COMA? #sentenciaIncremento
                    |IDENTIFICADOR CORABRE expresion CORCIERRA SIGNO_ASIGNACION expresion (PUNTO_COMA)? #sentenciaAsignacionSlice
                    ;



//------------------------------------------Sentencias de control--------------------------------------------
sentencia_if : bloque_if (bloque_if_else)* (bloque_else)?
; 

bloque_if : RESIF (PARABRE expresion PARCIERRA | expresion) (lista_instrucciones);

bloque_if_else : RESELSE RESIF (PARABRE expresion PARCIERRA | expresion) lista_instrucciones;

bloque_else : RESELSE lista_instrucciones 
;

sentencia_for : RESFOR PARABRE? (declaracion_variable | sentencia_asignacion | expresion)? PUNTO_COMA (expresion)? PUNTO_COMA (sentencia_asignacion)? PARCIERRA? lista_instrucciones #sentenciaForComun
             | RESFOR expresion lista_instrucciones #sentenciaForEstiloGo
             ;


/*
sentencia_for : RESFOR PARABRE (declaracion_variable | sentencia_asignacion | expresion)? PUNTO_COMA (expresion)? PUNTO_COMA (declaracion_variable | sentencia_asignacion | expresion)? PARCIERRA lista_instrucciones
             | RESFOR expresion lista_instrucciones; // Sentencia for estilo Go
 */

sentencia_switch : RESSWITCH  expresion  LLAVEABRE (bloque_caso)* (bloque_default)? LLAVECIERRA
;

bloque_caso : RESCASE expresion DOS_PUNTOS ((lista_declaracion|lista_sentencia))*;

bloque_default : RESDEFAULT DOS_PUNTOS (lista_instrucciones)*  #bloqueDefault  
;

sentecia_return : RESRETURN expresion (PUNTO_COMA)?;

sentencia_break : RESBREAK (PUNTO_COMA)?;

sentencia_continue : RESCONTINUE (PUNTO_COMA)?;










//------funciones----------------
 // llamada a funcion

llamada_funcion : (IDENTIFICADOR|funcion_embedida) PARABRE (lista_argumentos)? PARCIERRA (PUNTO_COMA)? #llamadaFuncionCompleta

;

funcion_embedida : RESFMT PUNTO RESPRINTLN #funcionPrintln
                | RESSTR PUNTO (RESATOI|RESPARSEFLOAT) #funcionAtoi
                | RESREFLECT PUNTO RESTYPEOF #funcionTypeOf
                |RESSLICE PUNTO RESINDEX #funcionSlicesIndex
                |RESSTRINGS PUNTO RESJOIN #funcionStringsJoin
                |RESAPPEND #funcionAppend
                ;
 

 lista_argumentos : expresion (SIGNO_COMA expresion)* #listaArgumentos
 ;

 lista_parametros : parametro (',' parametro)* #listaParametros
 ;

parametro :  IDENTIFICADOR tipo
 ;
 //----------------------------estructuras de datos
// [] int {elemento1,elemento2,...,elementon}
 declaracionSlice : slice_tipo LLAVEABRE lista_slice? LLAVECIERRA;

lista_slice :expresion (SIGNO_COMA expresion)* ;


slice_tipo: CORABRE CORCIERRA tipo;


//------expresiones-----

expresion : PARABRE expresion PARCIERRA #expresionParentesis
                |op=(SIGNO_NOT|SIGNO_MENOS) expresion #expresionNegacion
                |left=expresion (op=SIGNO_DIV | op=SIGNO_MODULO |op=SIGNO_MULTI ) right=expresion #expresionMultiDiv
                |left=expresion op=(SIGNO_MAS|SIGNO_MENOS) expresion #expresionSumaResta
                | left=expresion (op=SIGNO_MENOSQUE | op=SIGNO_MENOSQUE_IGUAL | op=SIGNO_MASQUE_IGUAL |op=SIGNO_MASQUE)right=expresion #expresionRelacional
                | left=expresion (op=SIGNO_IGUALDAD | op=SIGNO_NO_IGUAL ) right=expresion #expresionIgualdad
                | left=expresion (op=SIGNO_AND | op=SIGNO_OR )right=expresion #expresionLogica
                |declaracionSlice #expresion_slice
                |llamada_funcion #expresionLlamadafuncion
                |IDENTIFICADOR CORABRE expresion CORCIERRA #expresionAccesoSlice

                | IDENTIFICADOR #expresionIdentificador
                | INT #expresionInt
                | FLOAT #expresionFloat
                | CADENA #expresionCadena
                | RESTRUE #expresionTrue
                | RESFALSE #expresionFalse
                | RUNE #expresionRune

             
                ;
/* 
expresion : simpleExpresion (op=SIGNO_COMA simpleExpresion)* #expresionComa
            | SIGNO_MENOS simpleExpresion #expresionNegacion
            | left=simpleExpresion (op=SIGNO_AND | op=SIGNO_OR )right=simpleExpresion #expresionLogicaAndOr
            | left=simpleExpresion (op=SIGNO_ASIGNACION SIGNO_ASIGNACION | op=SIGNO_NOT SIGNO_ASIGNACION | op=SIGNO_MASQUE | op=SIGNO_MENOSQUE SIGNO_ASIGNACION |op=SIGNO_MENOSQUE )right=simpleExpresion #xpresionRelacional
            | left=simpleExpresion (op=SIGNO_MENOSQUE | op=SIGNO_MENOSQUE SIGNO_ASIGNACION | op=SIGNO_MASQUE SIGNO_ASIGNACION )right=simpleExpresion #expresionRelacional
            | llamada_funcion #expresionLlamadaFuncion
            |IDENTIFICADOR (op=SIGNO_MAS SIGNO_MAS | op=SIGNO_MENOS SIGNO_MENOS) #expresionIncrementoDecremento
           ;


                        

simpleExpresion : PARABRE expresion PARCIERRA #expresionParentesis
                | IDENTIFICADOR #expresionIdentificador
                | INT #expresionInt
                | FLOAT #expresionFloat
                | CADENA #expresionCadena
                | RESTRUE #expresionTrue
                | RESFALSE #expresionFalse
                | RUNE #expresionRune
                ;
*/

// tipos de variables

tipo : RESINT 
    |RESFLOAT 
    |RESSTRING 
    |RESBOOL                                                      
    |RESRUNE 
    |RESSLICE 
    |IDENTIFICADOR 
    |slice_tipo
    ;

