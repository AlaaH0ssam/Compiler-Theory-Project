# Compiler Theory Project

This project is an implementation of a simple compiler for a Tiny-like programming language.  
It was developed as part of a **Compiler Theory** course and covers the main phases of compilation, starting from lexical analysis up to syntax analysis.

---

## 🧠 Project Components

### 1️⃣ Context Free Grammar (CFG)
- Definition of the language syntax using formal grammar rules.
- Covers:
  - Declarations
  - Assignments
  - Expressions and equations
  - Conditions and control structures (if, repeat)
  - Function declarations and calls

---

### 2️⃣ Deterministic Finite Automata (DFA)
- DFA used to recognize:
  - Identifiers
  - Numbers (integers and floats)
  - Strings
  - Operators and delimiters
- Each token type is validated through DFA transitions.

---

### 3️⃣ Scanner (Lexical Analyzer)
- Converts source code into a stream of tokens.
- Handles:
  - Reserved keywords
  - Identifiers
  - Constants
  - Operators
  - Comments
- Reports lexical errors such as unrecognized tokens.

---

### 4️⃣ Parser (Syntax Analyzer)
- Implemented using **Recursive Descent Parsing**.
- Builds a **Parse Tree** based on the defined CFG.
- Supports:
  - Arithmetic expressions with correct operator precedence
  - Conditional statements
  - Repeat-until loops
  - Function declarations and calls
- Reports syntax errors with clear messages.

---

## 🛠️ Technologies Used
- **C#**
- **.NET Framework**
- Windows Forms (for displaying parse trees)

---

## 📌 Features
- Full lexical and syntax analysis
- Operator precedence handling
- Error detection and reporting
- Parse Tree visualization

---

## 📂 Project Structure
├── CFG
├── DFA
├── Scanner
├── Parser
├── Parse Tree
└── Sample Programs


---

## ▶️ How to Run
1. Open the project in **Visual Studio**.
2. Run the application.
3. Enter a source code program.
4. View the generated tokens and parse tree.

---

## 🎯 Sample Supported Code
```c
int main()
{
    int x;
    read x;
    if x > 0 then
        x := x - 1;
    end
    return 0;
}
