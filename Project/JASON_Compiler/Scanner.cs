using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;


//updates:  add Comma and main 
public enum Token_Class
{
    Int, Float, String, Read, Write, Repeat, Until, If, ElseIf, Else, Then, Return, End, Endl, 
    PlusOp, MinusOp, MultiplyOp, DivideOp, EqualOp, AssignEqualOp, LessThanOp,
    GreaterThanOp, NotEqualOp, EqualEqualOp, AndOP, OrOp, Idenifier, Constant, Undefined, Semicolon,
    LeftParenthesis, RightParenthesis, LeftCurlyBracket, RightCurlyBracket, Colon, main, Comma, EndOfFile,
}
 
namespace JASON_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("main", Token_Class.main);
            ReservedWords.Add("endl", Token_Class.Endl);


            Operators.Add("==", Token_Class.EqualEqualOp);
            Operators.Add("||", Token_Class.OrOp);
            Operators.Add("&&", Token_Class.AndOP);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add(":=", Token_Class.AssignEqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add("(", Token_Class.LeftParenthesis);
            Operators.Add(")", Token_Class.RightParenthesis);
            Operators.Add("{", Token_Class.LeftCurlyBracket);
            Operators.Add("}", Token_Class.RightCurlyBracket);
            Operators.Add(":", Token_Class.Colon);
            //updates:  add Comma
            Operators.Add(",", Token_Class.Comma);


        }



        public void StartScanning(string SourceCode)
        {
            int i = 0;
            int linenumber = 1; //count lines

            while (i < SourceCode.Length)
            {
                char CurrentChar = SourceCode[i];
                if (i + 1 < SourceCode.Length)
                {
                    string twoCharOp = SourceCode.Substring(i, 2);
                    if (Operators.ContainsKey(twoCharOp))
                    {
                        FindTokenClass(twoCharOp); 
                        i += 2; 
                        continue; 
                    }
                }
                // ignore spaces and new lines
                if (char.IsWhiteSpace(CurrentChar))
                {
                    if (CurrentChar == '\n')
                    {
                        linenumber++;
                    }
                    i++;
                    continue;
                }

                string CurrentLexeme = "";

                // ----- Identifiers or Reserved Words -----
                if (char.IsLetter(CurrentChar) || CurrentChar == '_')
                {
                    CurrentLexeme = "";
                    while (i < SourceCode.Length && (char.IsLetterOrDigit(SourceCode[i]) || SourceCode[i] == '_'))
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }

                    if (ReservedWords.ContainsKey(CurrentLexeme))
                        FindTokenClass(CurrentLexeme);
                    else
                    {
                        Regex validId = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");
                        if (validId.IsMatch(CurrentLexeme))
                            FindTokenClass(CurrentLexeme);
                        else
                            Errors.Error_List.Add($"Invalid identifier '{CurrentLexeme}' in line {linenumber}");
                    }
                    continue;
                }

                //---Numbers----//
                else if (char.IsDigit(CurrentChar))
                {
                    CurrentLexeme = "";
                    while (i < SourceCode.Length && (char.IsDigit(SourceCode[i]) || SourceCode[i] == '.'))
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }

                    int dotCount = CurrentLexeme.Count(c => c == '.');
                    if (dotCount > 1 || CurrentLexeme.EndsWith(".") || CurrentLexeme.StartsWith("."))
                    {
                        Errors.Error_List.Add($"Invalid numeric constant '{CurrentLexeme}' at line {linenumber}");
                        continue;
                    }

                    if (i < SourceCode.Length && char.IsLetter(SourceCode[i]))
                    {
                        string invalid = "";
                        while (i < SourceCode.Length && !char.IsWhiteSpace(SourceCode[i]) &&
                               !Operators.ContainsKey(SourceCode[i].ToString()) &&
                               SourceCode[i] != ';' && SourceCode[i] != '(' && SourceCode[i] != ')' &&
                               SourceCode[i] != '{' && SourceCode[i] != '}' && SourceCode[i] != ':')
                        {
                            invalid += SourceCode[i];
                            i++;
                        }
                        Errors.Error_List.Add($"Invalid identifier '{CurrentLexeme + invalid}' in line {linenumber}");
                        continue;
                    }

                    FindTokenClass(CurrentLexeme.Trim());
                    continue;
                }
                else if (CurrentChar == '.' && i + 1 < SourceCode.Length && char.IsDigit(SourceCode[i + 1]))
                {
                    string num = ".";
                    i++;
                    while (i < SourceCode.Length && char.IsDigit(SourceCode[i]))
                    {
                        num += SourceCode[i];
                        i++;
                    }
                    Errors.Error_List.Add($"Invalid numeric constant '{num}' at line {linenumber}");
                    continue;
                }


                else if (!char.IsLetterOrDigit(CurrentChar)
                         && CurrentChar != '_'
                         && !Operators.ContainsKey(CurrentChar.ToString())
                         && !(CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                         && CurrentChar != '"' && !char.IsWhiteSpace(CurrentChar))
                {
                    string invalidId = "";

                    while (i < SourceCode.Length &&
                           !char.IsWhiteSpace(SourceCode[i]) &&
                           !Operators.ContainsKey(SourceCode[i].ToString()) &&
                           SourceCode[i] != ';' && SourceCode[i] != '(' && SourceCode[i] != ')' &&
                           SourceCode[i] != '{' && SourceCode[i] != '}' && SourceCode[i] != ':')
                    {
                        invalidId += SourceCode[i];
                        i++;
                    }

                    if (invalidId == "") { invalidId += CurrentChar; i++; } // safety

                    if (invalidId.Any(char.IsLetter))
                        Errors.Error_List.Add($"Invalid identifier '{invalidId}' in line {linenumber}");
                    else
                        Errors.Error_List.Add($"Unrecognized token: {invalidId} at line {linenumber}");

                    continue;
                }

                // ---- Comments ----
                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    i += 2; // skip /*
                    bool closed = false;
                    int commentstartline = linenumber;

                    while (i < SourceCode.Length)
                    {

                        if (SourceCode[i] == '*')
                        {
                            if (i + 1 >= SourceCode.Length)
                            {
                                //error will appear in error list
                                //unclosed comment
                                i = SourceCode.Length;
                                break;
                            }
                            else if (SourceCode[i + 1] == '/')
                            {

                                closed = true;
                                i += 2; // skip */           
                                break;
                            }
                            else
                            {
                                //lw * inside comment move on
                                i++;
                            }
                        }

                        else if (SourceCode[i] == '\n') //count lines
                        {
                            linenumber++;
                            i++;
                        }

                        else
                        {
                            i++;
                        }
                    }

                    if (!closed)
                    {
                        Errors.Error_List.Add($"unclosed comment starting at line {commentstartline} and ending at {linenumber}");
                    }
                    continue;

                }

                // ---- Strings ----
                else if (CurrentChar == '"')
                {
                    string s = ""; //empty
                    s += SourceCode[i];
                    int stringStartLine = linenumber;
                    i++; // skip "
                    bool flag = false; //found closing "
                    bool hasError = false;

                    while (i < SourceCode.Length)
                    {
                        if (SourceCode[i] == '"')
                        {

                            s += SourceCode[i];
                            i++; // skip "
                            flag = true;
                            break;
                        }
                        //handling error case 5 invalid escape sequence
                        if (SourceCode[i] == '\\')
                        {
                            i++; //take one char

                            if (i >= SourceCode.Length)
                            {
                                Errors.Error_List.Add($"unexpected end of file while parsing string starting at line {stringStartLine}");
                                hasError = true;
                                break;
                            }

                            char c = SourceCode[i];

                            if (!(c == 'n' || c == 't' || c == 'r' || c == '"' || c == '\'' || c == '\\')) //only those escapes accepted 
                            {
                                Errors.Error_List.Add($"invalid escape sequence '\\{c}' at line {linenumber}");
                                hasError = true;
                            }

                            s += '\\';
                            s += c;
                            i++; //skip
                            continue;
                        }

                        //if new line and not closed qoutes       
                        if (SourceCode[i] == '\n' || SourceCode[i] == '\r')
                        {
                            Errors.Error_List.Add($"Unclosed quotes at {linenumber}");
                            hasError = true;
                            break;
                        }

                        s += SourceCode[i]; //other
                        i++; //skip
                    }
                    if (!flag && !hasError)
                    {
                        Errors.Error_List.Add($"Unexpected end of file while parsing string starting at line {stringStartLine}");
                        hasError = true;
                    }

                    if (!hasError)
                        FindTokenClass(s);

                    continue;
                }

                // ---- Operators ----
                else
                {
                    string twoCharOp = "";
                    string oneCharOp = CurrentChar.ToString();

                    if (i + 1 < SourceCode.Length)
                        twoCharOp = oneCharOp + SourceCode[i + 1];

                    if (Operators.ContainsKey(twoCharOp))
                    {
                        FindTokenClass(twoCharOp);
                        i += 2;
                        continue;
                    }
                    else if (Operators.ContainsKey(oneCharOp))
                    {
                        FindTokenClass(oneCharOp);
                        i++;
                        continue;
                    }
                    else
                    {
                        FindTokenClass(oneCharOp);
                        i++;
                        continue;
                    }
                }
            }
            string str = SourceCode.TrimEnd();
            if (str.EndsWith(":") && !str.EndsWith(":="))
            {
                Errors.Error_List.Add("Unexpected end of file while parsing token ':='");
            }

            JASON_Compiler.TokenStream = Tokens;
        }



        void FindTokenClass(string Lex)
        {
            //Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }

            //Is it an string?
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.String;
                Tokens.Add(Tok);
            }

            //Is it a Constant?
            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
                Tokens.Add(Tok);
            }

            //Is it an identifier?
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Idenifier;
                Tokens.Add(Tok);
            }


            //Is it an operator?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }

            //Is it an undefined?
            else
            {
                Errors.Error_List.Add($"Unrecognized token: {Lex}");
            }
        }


        bool isIdentifier(string lex)
        {
            // Check if the lex is an identifier or not.
            Regex reg = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$");
            if (reg.IsMatch(lex))
            {
                return true;
            }
            return false;
        }
        bool isConstant(string lex)
        {
            // Check if the lex is a constant (Number) or not.
            Regex reg = new Regex(@"^[0-9]+(\.[0-9]+)?$"); //$ was forgotten indicates end
            if (reg.IsMatch(lex))
            {
                return true;
            }
            return false;
        }
        bool isString(string lex)
        {
            //Check if the lex is an str or not.
            Regex reg = new Regex(@"^\""(?:[^""\\]|\\.)*\""$");
            if (reg.IsMatch(lex))
            {
                return true;
            }
            return false;
        }
    }
}
