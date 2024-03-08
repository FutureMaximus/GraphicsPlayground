using System.Text;

namespace GraphicsPlayground.Util;

/// <summary>
/// L-Systems allows for the generation of complex structures from a simple axiom.
/// Every 2 characters represents a command that can be executed.
/// https://www.cgjennings.ca/articles/l-systems/#applet
/// </summary>
public class LSystem
{
    public string Axiom;
    public int Iterations;

    private readonly Dictionary<char, string> _rules = [];

    public LSystem(string axiom, int iterations = 1)
    {
        if (axiom.Length % 2 != 0) throw new ArgumentException("Axiom must be even in length as commands are 2 chars.");
        Axiom = axiom;
        Iterations = iterations;
    }

    public string Iterate()
    {
        string result = Axiom;
        for (int i = 0; i < Iterations; i++)
        {
            result = Iterate(result);
        }
        result = result.Replace("[", "");
        result = result.Replace("]", "");
        return result;
    }

    public void AddRule(char rule, string result)
    {
        if (result.Length % 2 != 0) throw new ArgumentException("Result must be even in length as commands are 2 chars.");
        _rules.Add(rule, result);
    }

    public string Iterate(string input)
    {
        StringBuilder result = new();
        Stack<State> stateStack = new();

        for (int i = 0; i < input.Length; i += 2)
        {
            string command = input.Substring(i, 2);
            if (command.Contains('['))
            {
                // Push variables after [ and ending at ] onto the stack.
                int end = input.IndexOf(']', i);
                string variables = input.Substring(i + 2, end - i - 2);
                State state = new(variables);
                stateStack.Push(state);
            }
            else if (command.Contains(']'))
            {
                State state = stateStack.Pop();
                result = new StringBuilder(state.Result + result.ToString());
            }
            else if (_rules.TryGetValue(command[0], out string? value))
            {
                result.Append(value);
            }
            else
            {
                result.Append(command);
            }
        }

        return result.ToString();
    }

    private readonly struct State(string result)
    {
        public string Result { get; } = result;
    }
}
