using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalculatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Встановлюємо інваріантну культуру для коректної роботи з крапкою у числах
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            Console.WriteLine("=== Калькулятор виразів ===");
            Console.WriteLine("Введіть математичний вираз (наприклад: 5 + 5 * 10 + 4 / 2 - 3):");
            Console.Write("> ");
            
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Помилка: введено порожній рядок.");
                return;
            }

            try
            {
                double result = EvaluateExpression(input);
                Console.WriteLine($"Відповідь програми: {result}");
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час обчислення: {ex.Message}");
            }
        }

        // Головний метод обчислення рядка
        static double EvaluateExpression(string expression)
        {
            var tokens = Tokenize(expression);
            var rpn = ConvertToRPN(tokens);
            return CalculateRPN(rpn);
        }

        // 1. Розбиття рядка на токени (числа та знаки)
        static List<string> Tokenize(string expr)
        {
            List<string> tokens = new List<string>();
            int i = 0;

            while (i < expr.Length)
            {
                char c = expr[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                // Якщо цифра або роздільник дробу
                if (char.IsDigit(c) || c == '.')
                {
                    string number = "";
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                    {
                        number += expr[i];
                        i++;
                    }
                    tokens.Add(number);
                    continue;
                }

                // Оператори та дужки
                if ("+-*/()".Contains(c))
                {
                    tokens.Add(c.ToString());
                    i++;
                    continue;
                }

                throw new FormatException($"Невідомий символ: '{c}'");
            }

            return tokens;
        }

        // 2. Алгоритм Дейкстри: переведення в зворотну польську нотацію (RPN)
        static List<string> ConvertToRPN(List<string> tokens)
        {
            List<string> output = new List<string>();
            Stack<string> operators = new Stack<string>();

            foreach (var token in tokens)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    output.Add(token);
                }
                else if (token == "(")
                {
                    operators.Push(token);
                }
                else if (token == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")
                    {
                        output.Add(operators.Pop());
                    }

                    if (operators.Count == 0)
                        throw new FormatException("Помилка: невідповідність круглих дужок.");

                    operators.Pop(); // Видаляємо '('
                }
                else if (IsOperator(token))
                {
                    while (operators.Count > 0 && IsOperator(operators.Peek()) &&
                           GetPriority(operators.Peek()) >= GetPriority(token))
                    {
                        output.Add(operators.Pop());
                    }
                    operators.Push(token);
                }
            }

            while (operators.Count > 0)
            {
                var op = operators.Pop();
                if (op == "(" || op == ")")
                    throw new FormatException("Помилка: невідповідність круглих дужок.");
                output.Add(op);
            }

            return output;
        }

        // 3. Обчислення значення за зворотною польською нотацією
        static double CalculateRPN(List<string> rpnTokens)
        {
            Stack<double> stack = new Stack<double>();

            foreach (var token in rpnTokens)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    stack.Push(num);
                }
                else if (IsOperator(token))
                {
                    if (stack.Count < 2)
                        throw new InvalidOperationException("Некоректний синтаксис виразу.");

                    double b = stack.Pop();
                    double a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/":
                            if (Math.Abs(b) < 1e-9)
                                throw new DivideByZeroException("Ділення на нуль неможливе.");
                            stack.Push(a / b);
                            break;
                    }
                }
            }

            if (stack.Count != 1)
                throw new InvalidOperationException("Не вдалося обчислити результат.");

            return stack.Pop();
        }

        static bool IsOperator(string s) => s == "+" || s == "-" || s == "*" || s == "/";

        static int GetPriority(string op)
        {
            if (op == "*" || op == "/") return 2;
            if (op == "+" || op == "-") return 1;
            return 0;
        }
    }
}