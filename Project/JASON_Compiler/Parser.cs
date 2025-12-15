using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        //SOME OF  DONE METHODS FROM CFG WRITTEN BY ME
        //Program → Function_list Main_Function
        //Function_list → Function_Statement  F | ε
        Node Program()
        {
            SkipComment();
            Node program = new Node("Program");
            program.Children.Add(Function_list());
            program.Children.Add(Main_Function());
            MessageBox.Show("Success");
            return program;
        }



        Token_Class Peek(int k)
        {
            if (InputPointer + k >= TokenStream.Count)
                return Token_Class.Undefined;
            return TokenStream[InputPointer + k].token_type;
        }

        bool NextIsFunctionDeclaration()
        {
            //Datatype FunctionName “(“ paramlist “)” 
            //int\float\string  Idenifier LeftParenthesis
            if (InputPointer + 2 >= TokenStream.Count) return false;
            Token_Class t0 = Peek(0);
            Token_Class t1 = Peek(1);
            Token_Class t2 = Peek(2);
            bool first = (t0 == Token_Class.Int || t0 == Token_Class.Float || t0 == Token_Class.String);
            bool second = (t1 == Token_Class.Idenifier) && (t2 == Token_Class.LeftParenthesis);
            bool res = (first && second);
            return res;
        }


        //Function_list →Function_Statement Function_list | ε
        Node Function_list()
        {
            Node f = new Node("Function_list");
            //starts with Function_Declaration 
            //Function_Declaration -> Datatype FunctionName “(“ paramlist “)” 
            //Datatype -> int | float | String
            while (NextIsFunctionDeclaration())
            {
                SkipComment();
                Node fs = Function_Statement();
                if (fs == null)
                {
                    Errors.Error_List.Add("Parsing Error: Expected Function_Statement ");
                    break;
                }
                f.Children.Add(fs);

            }

            return f;
        }
        Node Main_Function()
        {
            //     Node MainFun = new Node("Main_Function");

            Node main = new Node("Main_Function");
            main.Children.Add(match(Token_Class.Int));                    // int
            main.Children.Add(match(Token_Class.main));                   // main
            main.Children.Add(match(Token_Class.LeftParenthesis));       // (
            main.Children.Add(match(Token_Class.RightParenthesis));      // )
            main.Children.Add(Function_Body());
            return main;
        }

        // FunctionCall -> Identifier "(" ArgList ")"
        Node FunctionCall()
        {
            Node func = new Node("FunctionCall");

            // Identifier
            func.Children.Add(match(Token_Class.Idenifier));

            // "("
            func.Children.Add(match(Token_Class.LeftParenthesis));

            // ArgList (call once, always returns a Node)
            Node args = ArgList();
            if (args.Children.Count > 0)    // add only if not epsilon
                func.Children.Add(args);

            // ")"
            func.Children.Add(match(Token_Class.RightParenthesis));

            return func;
        }

        // ArgList -> Identifier ArgTail | ε
        Node ArgList()
        {
            Node argList = new Node("ArgList");

            // epsilon case: next token not identifier -> empty arglist
            if (InputPointer >= TokenStream.Count || TokenStream[InputPointer].token_type != Token_Class.Idenifier)
                return argList; // empty (ε)

            // Identifier
            argList.Children.Add(match(Token_Class.Idenifier));

            // ArgTail 
            Node tail = ArgTail();
            if (tail.Children.Count > 0)
                argList.Children.Add(tail);

            return argList;
        }

        // ArgTail -> "," Identifier ArgTail | ε
        Node ArgTail()
        {
            Node tail = new Node("ArgTail");

            // epsilon case lw no comma
            if (InputPointer >= TokenStream.Count || TokenStream[InputPointer].token_type != Token_Class.Comma)
                return tail;

            // ","  
            tail.Children.Add(match(Token_Class.Comma));

            // Identifier
            tail.Children.Add(match(Token_Class.Idenifier));

            // recursive tail
            Node more = ArgTail();
            if (more.Children.Count > 0)
                tail.Children.Add(more);

            return tail;
        }


        //2) Term -> Constant | Identifier | FunctionCall
        /* Node Term()
          {
              Node term = new Node("Term");

              if (InputPointer >= TokenStream.Count)
                  return term;

              Token_Class tk = TokenStream[InputPointer].token_type;

              // FunctionCall case start with (Identifier and LeftCurlyBracket...etc)
              if (tk == Token_Class.Idenifier && InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.LeftCurlyBracket)
              {
                  term.Children.Add(FunctionCall());
                  return term;
              }

              // Identifier case
              else if (tk == Token_Class.Idenifier)
              {
                  term.Children.Add(match(Token_Class.Idenifier));
                  return term;
              }

              // Constant case
              else if (tk == Token_Class.Constant)
              {
                  term.Children.Add(match(Token_Class.Constant));
                  return term;
              }

              // Error case
              Errors.Error_List.Add("Parsing Error in Term: Expected Identifier, Constant, or FunctionCall but found " + tk.ToString() + "\r\n");
              return term;
          }
*/

         // Read_Statement -> read Identifier;
         Node Read_Statement()
          {
              Node read = new Node("Read_Statement");

              // read
              read.Children.Add(match(Token_Class.Read));

              // Identifier
              read.Children.Add(match(Token_Class.Idenifier));

              // ;
              read.Children.Add(match(Token_Class.Semicolon));

              return read;
          }


          //4)Return_Statement -> return Expression ;
          Node Return_Statement()
          {
              Node ret = new Node("Return_Statement");

              // return
              ret.Children.Add(match(Token_Class.Return));

              // Expression
              ret.Children.Add(Expression());

              // ;
              ret.Children.Add(match(Token_Class.Semicolon));

              return ret;
          }

          // Condition_Statement -> Condition Condition_Statement_Tail
          Node Condition_Statement()
          {
              Node cs = new Node("Condition_Statement");

              // Condition
              cs.Children.Add(Condition());

              // tail
              Node tail = Condition_Statement_Tail();
              if (tail != null && tail.Children.Count() > 0)
                  cs.Children.Add(tail);

              return cs;
          }


          // Condition_Statement_Tail -> Boolean_Operator Condition Condition_Statement_Tail | ε
          Node Condition_Statement_Tail()
          {
              // if next token is not a boolean operator
              if (!(InputPointer < TokenStream.Count &&
                   (TokenStream[InputPointer].token_type == Token_Class.AndOP ||
                    TokenStream[InputPointer].token_type == Token_Class.OrOp)))
              {
                  return null; // ε
              }

              // otherwise: we have BooleanOperator Condition Tail
              Node tail = new Node("Condition_Statement_Tail");

              // Boolean operator
              Node bo = BooleanOperator();
              if (bo == null)
              {
                  // if BooleanOperator failed 
                  Errors.Error_List.Add("Parsing Error: Expected boolean operator (&& or ||)\r\n");
                  return null;
              }
              tail.Children.Add(bo);

              // Condition
              Node cond = Condition();
              if (cond == null)
              {
                  Errors.Error_List.Add("Parsing Error: Expected condition after boolean operator\r\n");
                  return tail;
              }
              tail.Children.Add(cond);

              // recursive tail
              Node rest = Condition_Statement_Tail();
              if (rest != null && rest.Children.Count > 0)
                  tail.Children.Add(rest);

              return tail;
          }


          // Parameter -> Datatype Identifier
          Node Parameter()
          {
              Node par = new Node("Parameter");

              // datatype
              par.Children.Add(Datatype());

              // identifier
              par.Children.Add(match(Token_Class.Idenifier));

              return par;
          }

          // Datatype -> int | float | string
          Node Datatype()
          {
              Node dataType = new Node("DataType");
              if (InputPointer < TokenStream.Count &&
                  (TokenStream[InputPointer].token_type == Token_Class.Int ||
                   TokenStream[InputPointer].token_type == Token_Class.Float ||
                   TokenStream[InputPointer].token_type == Token_Class.String))
              {
                  dataType.Children.Add(match(TokenStream[InputPointer].token_type));
              }
              return dataType;
          }

          // FunctionName -> Identifier
          Node FunctionName()
          {
              Node f = new Node("FunctionName");
              f.Children.Add(match(Token_Class.Idenifier));
              return f;
          }

          // Function_Declaration -> Datatype FunctionName "(" paramlist ")"
          Node Function_Declaration()
          {
              Node decl = new Node("Function_Declaration");

              // Datatype
              decl.Children.Add(Datatype());

              // FunctionName 
              decl.Children.Add(FunctionName());

              // (
              decl.Children.Add(match(Token_Class.LeftParenthesis));

              // paramlist (could be empty)
              decl.Children.Add(ParamList());

              // )
              decl.Children.Add(match(Token_Class.RightParenthesis));

              return decl;
          }

          //paramlist →Parameter paramtail | ε
          Node ParamList()
          {
              Node list = new Node("ParamList");

              // if Parameter -> Datatype Identifier then first part
              if (InputPointer < TokenStream.Count &&
                 (TokenStream[InputPointer].token_type == Token_Class.Int ||
                  TokenStream[InputPointer].token_type == Token_Class.Float ||
                  TokenStream[InputPointer].token_type == Token_Class.String))
              {
                  list.Children.Add(Parameter());
                  list.Children.Add(ParamTail());
                  return list;
              }
              else
                  return null;

          }

          // paramtail -> , Parameter paramtail | ε
          Node ParamTail()
          {
              Node tail = new Node("ParamTail");

              // if comma so  first part
              if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
              {
                  tail.Children.Add(match(Token_Class.Comma));
                  tail.Children.Add(Parameter());
                  tail.Children.Add(ParamTail()); // recursive as grammar specified
                  return tail;
              }

              else
                  return null;

          }

          // ) Function_Statement → Function_Declaration Function_Body
          Node Function_Statement()
          {
              Node FuncStat = new Node("Function_Statement");

              //Function_Declaration
              FuncStat.Children.Add(Function_Declaration());

              // Function_Body 
              FuncStat.Children.Add(Function_Body());

              return FuncStat;
          }

          // Function_Body -> "{" Statements Return_Statement "}"

          Node Function_Body()
          {
              Node fb = new Node("Function_Body");
              fb.Children.Add(match(Token_Class.LeftCurlyBracket));   
              fb.Children.Add(Statements());
             if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Return)
             {
                 fb.Children.Add(Return_Statement());
             }
              fb.Children.Add(match(Token_Class.RightCurlyBracket));  
              return fb;
          }
        //============================NOT DONE====================
        //========================================================
        //========================================================
        //========================================================
        //============================NOT DONE====================

        //  Equation → factor Equation_Tail
        // Equation -> Factor Equation_Tail
        Node Equation()
        {
            Node eq = new Node("Equation");

            eq.Children.Add(Factor());

            Node tail = Equation_Tail();
            if (tail != null && tail.Children.Count > 0)
                eq.Children.Add(tail);

            return eq;
        }

        // Equation_Tail -> AddOp Factor Equation_Tail 
        //               | MulOp Factor Equation_Tail 
        //               | ε
        Node Equation_Tail()
        {
            Node tail = new Node("Equation_Tail");

            if (InputPointer >= TokenStream.Count)
                return tail; // ε

            Token_Class cur = TokenStream[InputPointer].token_type;

            if (cur == Token_Class.PlusOp || cur == Token_Class.MinusOp)
            {
                tail.Children.Add(AddOp());
                tail.Children.Add(Factor());

                Node more = Equation_Tail();
                if (more.Children.Count > 0)
                    tail.Children.Add(more);
            }
            else if (cur == Token_Class.MultiplyOp || cur == Token_Class.DivideOp)
            {
                tail.Children.Add(MultiOp());
                tail.Children.Add(Factor());

                Node more = Equation_Tail();
                if (more.Children.Count > 0)
                    tail.Children.Add(more);
            }

            return tail;
        }


        //2) Term -> Constant | Identifier | FunctionCall
        Node Term()
        {
            Node term = new Node("Term");

            if (InputPointer >= TokenStream.Count)
                return term;

            Token_Class tk = TokenStream[InputPointer].token_type;

            // FunctionCall case start with (Identifier and LeftCurlyBracket...etc)
            if (tk == Token_Class.Idenifier && InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.LeftCurlyBracket)
            {
                term.Children.Add(FunctionCall());
                return term;
            }

            // Identifier case
            else if (tk == Token_Class.Idenifier)
            {
                term.Children.Add(match(Token_Class.Idenifier));
                return term;
            }

            // Constant case
            else if (tk == Token_Class.Constant)
            {
                term.Children.Add(match(Token_Class.Constant));
                return term;
            }

            // Error case
            Errors.Error_List.Add("Parsing Error in Term: Expected Identifier, Constant, or FunctionCall but found " + tk.ToString() + "\r\n");
            return term;
        }

        // Term_Tail -> (*|/) Factor Term_Tail | ε
        Node Term_Tail()
        {
            Node tail = new Node("Term_Tail");

            if (InputPointer < TokenStream.Count &&
               (TokenStream[InputPointer].token_type == Token_Class.MultiplyOp ||
                TokenStream[InputPointer].token_type == Token_Class.DivideOp))
            {
                tail.Children.Add(MultiOp());
                tail.Children.Add(Factor());

                Node rest = Term_Tail();
                if (rest.Children.Count > 0)
                    tail.Children.Add(rest);
            }

            return tail; // ε
        }
        // Factor -> Term | "(" Equation ")"
        Node Factor()
        {
            Node factor = new Node("Factor");

            if (InputPointer >= TokenStream.Count)
                return factor;

            Token_Class cur = TokenStream[InputPointer].token_type;

            if (cur == Token_Class.LeftParenthesis)
            {
                factor.Children.Add(match(Token_Class.LeftParenthesis));
                factor.Children.Add(Equation());
                factor.Children.Add(match(Token_Class.RightParenthesis));
            }
            else
            {
                factor.Children.Add(Term());
            }

            return factor;
        }


        // Expression → “string” | Equation
        Node Expression()
         {
             Node exp = new Node("Expression");

             if (InputPointer < TokenStream.Count &&
                 TokenStream[InputPointer].token_type == Token_Class.String)
             {
                 exp.Children.Add(match(Token_Class.String));
             }
             else
             {
                 exp.Children.Add(Equation());
             }

             return exp;
         }
         //Write_Statement → write WC;
         // WC → Expression | Endl
         Node Write_Statement()
          {
              Node write = new Node("Write_Statement");

              write.Children.Add(match(Token_Class.Write));
             if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Endl)
             {
                 write.Children.Add(match(Token_Class.Endl));
             }
             else
             {
                 write.Children.Add(Expression());
             }
             write.Children.Add(match(Token_Class.Semicolon));

              return write;
          }

        Node If_Statement()
        {
            Node ifNode = new Node("If_Statement");

            ifNode.Children.Add(match(Token_Class.If));
            ifNode.Children.Add(Condition_Statement());
            ifNode.Children.Add(match(Token_Class.Then));
            ifNode.Children.Add(Statements());

            if (InputPointer < TokenStream.Count &&
                TokenStream[InputPointer].token_type == Token_Class.End)
            {
                ifNode.Children.Add(match(Token_Class.End));
            }

            return ifNode;
        }
        Node ElseIf_Statement_Tail()
         {
             if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIf)
             {
                 Node elseIfNode = new Node("ElseIf_Statement");
                 elseIfNode.Children.Add(match(Token_Class.ElseIf));         // elseif
                 elseIfNode.Children.Add(Condition_Statement());            // condition
                 elseIfNode.Children.Add(match(Token_Class.Then));           // then
                 elseIfNode.Children.Add(Statements());                      // statements inside elseif

                 // recursive to allow multiple elseif
                 Node rest = ElseIf_Statement_Tail();
                 if (rest != null) elseIfNode.Children.Add(rest);

                 return elseIfNode;
             }
             return null; // ε
         }
         Node Else_Statement()
         {
             Node elseNode = new Node("Else_Statement");
             elseNode.Children.Add(match(Token_Class.Else));
             elseNode.Children.Add(Statements());   // statements inside else
             if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.End)
                 elseNode.Children.Add(match(Token_Class.End));
             return elseNode;
         }
         Node Repeat_Statement()
         {
             Node repeatNode = new Node("Repeat_Statement");
             repeatNode.Children.Add(match(Token_Class.Repeat));  // repeat
             repeatNode.Children.Add(Statements());               // statements inside repeat

             if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Until)
             {
                 repeatNode.Children.Add(match(Token_Class.Until));   // until
                 repeatNode.Children.Add(Condition_Statement());       // condition after until
             }
             else
             {
                 Errors.Error_List.Add("Parsing Error: Expected 'until' after repeat block");
             }

             return repeatNode;
         }
         Node Statement()
         {
             if (InputPointer >= TokenStream.Count) return null;
             Token_Class cur = TokenStream[InputPointer].token_type;
             if (cur == Token_Class.If) return If_Statement();//if statement
             if (cur == Token_Class.Repeat) return Repeat_Statement();
             if (IsDatatype(cur)) return Declaration_Statement();
             if (cur == Token_Class.Write) return Write_Statement();
             if (cur == Token_Class.Read) return Read_Statement();
             if (cur == Token_Class.Return) return Return_Statement();

             if (cur == Token_Class.Idenifier)
             {
                 if (InputPointer + 1 < TokenStream.Count)
                 {
                     var next = TokenStream[InputPointer + 1].token_type;
                     if (next == Token_Class.AssignEqualOp || next == Token_Class.EqualOp) return Assignment_Statement();
                     if (next == Token_Class.LeftParenthesis) return FunctionCall();

                 }
             }
            if (cur == Token_Class.Repeat)
            {
                Node rep = Repeat_Statement();
                rep.Children.Add(match(Token_Class.Semicolon)); 
                return rep;
            }

            return null;
         }

         bool IsDatatype(Token_Class t) => t == Token_Class.Int || t == Token_Class.Float || t == Token_Class.String;
         Node Statements()
         {
             Node stmts = new Node("Statements");

             while (InputPointer < TokenStream.Count)
             {
                 SkipComment();

                 if (InputPointer >= TokenStream.Count) break;

                 Token_Class cur = TokenStream[InputPointer].token_type;


                 if (cur == Token_Class.RightCurlyBracket || cur == Token_Class.EndOfFile || cur == Token_Class.Until || cur == Token_Class.End)
                     break; 

                 Node s = Statement();
                 if (s != null)
                     stmts.Children.Add(s);
                 else
                 {
                     Errors.Error_List.Add($"Parsing Error: Unexpected token {cur}");
                     break;
                 }
             }
             return stmts;
         }
         /* Declaration_Statement →Datatype Identifier Identifier_Optional_Assign Identifier_List_Cont “;”
              • Identifier_List_Cont → “,” Identifier Identifier_Optional_Assign Identifier_List_Cont | ε
              • Identifier_Optional_Assign → Assignment_Statement | ε*/
        Node Declaration_Statement()
        {
            Node decl = new Node("Declaration_Statement");

            decl.Children.Add(Datatype());

            decl.Children.Add(match(Token_Class.Idenifier));

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AssignEqualOp) // :=
            {
                decl.Children.Add(match(Token_Class.AssignEqualOp));
                decl.Children.Add(Expression());
            }

            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                decl.Children.Add(match(Token_Class.Comma));
                decl.Children.Add(match(Token_Class.Idenifier));

                if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AssignEqualOp)
                {
                    decl.Children.Add(match(Token_Class.AssignEqualOp));
                    decl.Children.Add(Expression());
                }
            }

            decl.Children.Add(match(Token_Class.Semicolon));

            return decl;
        }
        // Assignment_Statement → Identifier := Expression ;
        Node Assignment_Statement()
        {
            Node assign = new Node("Assignment_Statement");
            assign.Children.Add(match(Token_Class.Idenifier));
            assign.Children.Add(match(Token_Class.AssignEqualOp)); // := 
            assign.Children.Add(Expression());
            assign.Children.Add(match(Token_Class.Semicolon));
            return assign;
        }

        Node Condition()
        {
            Node con = new Node("Condition");
            con.Children.Add(match(Token_Class.Idenifier));

            // Condition Operator: < | > | = | <>
            Node op = ConditionOperator();
            if (op == null)
            {
                Errors.Error_List.Add("Missing condition operator after identifier");
                return con;
            }
            con.Children.Add(op);

            // Term (Number | Identifier | FunctionCall)

            Node term = Term();
            if (term != null)
                con.Children.Add(term);
            return con;
        }

        void SkipComment()
        {
            while (InputPointer < TokenStream.Count)
            {
                string lex = TokenStream[InputPointer].lex ?? "";

                if (lex == "/*")
                {
                    InputPointer++;
                    int startPosition = InputPointer;

                    while (InputPointer < TokenStream.Count)
                    {
                        if (TokenStream[InputPointer].lex == "*/")
                        {
                            InputPointer++;
                            break;
                        }
                        InputPointer++;

                        if (InputPointer - startPosition > 1000)
                        {
                            Errors.Error_List.Add("Parsing Error: Unclosed comment");
                            break;
                        }
                    }
                }
                else if (lex == "*/" && InputPointer > 0)
                {
                    Errors.Error_List.Add("Parsing Error: Unexpected comment closure");
                    InputPointer++;
                }
                else
                {
                    break;
                }
            }
        }
        Node ConditionOperator()
        {
            Node op = new Node("Condition_Operator");

            if (InputPointer >= TokenStream.Count)
            {
                Errors.Error_List.Add("Expected condition operator (<, >, =, <>)");
                return null;
            }

            Token_Class current = TokenStream[InputPointer].token_type;

            if (current == Token_Class.LessThanOp ||
                current == Token_Class.GreaterThanOp ||
                current == Token_Class.EqualOp ||      // = 
                current == Token_Class.NotEqualOp)     // <>  
            {
                op.Children.Add(match(current));
                return op;
            }

            Errors.Error_List.Add($"Invalid condition operator: {current}");
            return null;
        }


        //check ducument to complete.....

        //check doucoment to complete all


        //========================================================
        //========================================================
        //========================================================
        //========================================================
        //dont remove u may update if needed
        //========================================================

        //* | /
        Node MultiOp()
        {
            Node mulOp = null;
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MultiplyOp)
            {
                mulOp = match(Token_Class.MultiplyOp);
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.DivideOp)
            {
                mulOp = match(Token_Class.DivideOp);
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                    + Token_Class.MultiplyOp.ToString() + "or " + Token_Class.DivideOp.ToString()
                    + " and " + TokenStream[InputPointer].token_type.ToString()
                    + "  found\r\n");
                InputPointer++;
            }
            return mulOp;
        }


        //+ | -
        Node AddOp()
        {
            Node addOp = null;
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.PlusOp)
            {
                addOp = match(Token_Class.PlusOp);
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.MinusOp)
            {
                addOp = match(Token_Class.MinusOp);
            }

            return addOp;
        }

        //  && | ||
        Node BooleanOperator()
        {
            Node bo = new Node("BooleanOperator");

            if (InputPointer < TokenStream.Count &&
               TokenStream[InputPointer].token_type == Token_Class.AndOP)
            {
                bo.Children.Add(match(Token_Class.AndOP));
            }
            else if (InputPointer < TokenStream.Count &&
                     TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                bo.Children.Add(match(Token_Class.OrOp));
            }
            else
            {
                // not a boolean operator — return empty (caller should check)
                return null;
            }

            return bo;
        }

        public Node match(Token_Class ExpectedToken)
        {
            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("debug1 Error: Expected "+ ExpectedToken.ToString() + " and " + TokenStream[InputPointer].token_type.ToString() + " found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("debug2 Parsing Error: Expected " + ExpectedToken.ToString() + "\r\n");
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
     
    }
}