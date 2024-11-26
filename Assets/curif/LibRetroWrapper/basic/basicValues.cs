using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class BasicValue
{
    public enum BasicValueType
    {
        Number,
        String,
        empty
    }
    string str = "";
    double number = 0;
    BasicValueType type = BasicValueType.empty;

    public static BasicValue True = new BasicValue(1);
    public static BasicValue False = new BasicValue(0);
    public static BasicValue EmptyString = new BasicValue("");

    static List<string> validOperations = new List<string> {
         "+", "-", "/", "*", "=", "<>", "!=", ">", "<", "<=", ">=",
         "||", "&&"
         };

    public static Dictionary<string, int> OperatorPrecedence =
        new Dictionary<string, int>
            {
                { "&&", 1 },
                { "||", 2 },
                { "=", 3 },
                { "!=", 3 },
                { "<>", 3 },
                { "<", 3 },
                { "<=", 3 },
                { ">=", 3 },
                { ">", 3 },
                { "+", 4 },
                { "-", 4 },
                { "*", 5 },
                { "/", 5 }
            };

    // Default constructor sets value to 0
    public BasicValue() => SetValue(0);

    // Overloaded constructors for various data types
    public BasicValue(double number) => SetValue(number);
    public BasicValue(int number) => SetValue((double)number);
    public BasicValue(float number) => SetValue((double)number);
    public BasicValue(bool boolean) => SetValue(boolean ? 1.0 : 0.0);
    public BasicValue(BasicValue val) => SetValue(val);

    // String constructor with optional forced type
    public BasicValue(string str, BasicValueType forceType = BasicValueType.empty)
    {
        if (forceType == BasicValueType.String ||
            (str.Length >= 2 && str[0] == '"' && str[^1] == '"'))
        {
            if (str.Length >= 2 && str[0] == '"' && str[^1] == '"')
            {
                str = str[1..^1]; // More efficient substring removal
            }
            SetValue(str);
        }
        else if (str.Length > 1 && str[0] == '&') // Hexadecimal format
        {
            SetValue(FunctionHelper.HexStringToDecimal(str[1..])); // Efficiently skip '&' character
        }
        else if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value))
        {
            SetValue(value);
        }
        else
        {
            SetValue(str); // Fallback for unquoted non-numeric strings
        }
    }


    public BasicValue SetValue(BasicValue val)
    {
        this.type = val.type;
        this.number = val.number;
        this.str = val.str;
        return this;
    }
    public BasicValue SetValue(double val)
    {
        this.number = val;
        type = BasicValueType.Number;
        return this;
    }
    public BasicValue SetValue(int val)
    {
        this.number = (double)val;
        type = BasicValueType.Number;
        return this;
    }
    public BasicValue SetValue(string str)
    {
        this.str = str;
        type = BasicValueType.String;
        return this;
    }

    public double GetValueAsNumber()
    {
        if (type != BasicValueType.Number)
            throw new Exception("Value can't be used as a number");
        return number;
    }

    public string GetValueAsString()
    {
        if (type != BasicValueType.String)
            throw new Exception("Value can't be used as a string");
        return str;
    }

    public double GetNumber()
    {
        return number;
    }

    public int GetInt()
    {
        return (int)number;
    }

    public bool GetBoolean()
    {
        if (type == BasicValueType.String && str != null)
            return true;
        if (type == BasicValueType.Number && number != 0)
            return true;
        return false;
    }

    public string GetString()
    {
        return str;
    }

    public BasicValueType Type()
    {
        return this.type;
    }

    public BasicValue CastTo(BasicValueType type)
    {
        if (this.type == BasicValueType.Number && type == BasicValueType.String)
        {
            this.str = number.ToString();
            this.type = BasicValueType.String;
        }
        else if (this.type == BasicValueType.String && type == BasicValueType.Number)
        {
            double valueDouble;
            bool isParsableToDouble = double.TryParse(this.str, out valueDouble);
            if (!isParsableToDouble)
                throw new Exception($" string value {this.str} can't be casted to double");

            this.number = valueDouble;
            this.type = BasicValueType.Number;
        }
        return this;
    }

    public static bool operator ==(BasicValue obj1, BasicValue obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            return false;

        if (obj1.type != obj2.type)
            return false;

        if (obj1.type == BasicValueType.Number)
            return obj1.number == obj2.number;

        return obj1.str == obj2.str;
    }

    public static bool operator !=(BasicValue obj1, BasicValue obj2)
    {
        return !(obj1 == obj2);
    }

    public static bool operator >(BasicValue obj1, BasicValue obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return false;

        if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            return true;

        if (obj1.type != obj2.type)
            return true;

        if (obj1.type == BasicValueType.Number)
            return obj1.number > obj2.number;

        return string.Compare(obj1.str, obj2.str) > 0;
    }
    public static bool operator <(BasicValue obj1, BasicValue obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return false;

        if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            return true;

        if (obj1.type != obj2.type)
            return true;

        if (obj1.type == BasicValueType.Number)
            return obj1.number < obj2.number;

        return string.Compare(obj1.str, obj2.str) < 0;
    }
    public static bool operator >=(BasicValue obj1, BasicValue obj2)
    {
        return (obj1 > obj2) || (obj1 == obj2);
    }
    public static bool operator <=(BasicValue obj1, BasicValue obj2)
    {
        return obj1 < obj2 || obj1 == obj2;
    }

    public static BasicValue operator +(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Number && obj2.type == BasicValueType.String)
            obj1.CastTo(BasicValueType.String);
        else if (obj2.type == BasicValueType.Number && obj1.type == BasicValueType.String)
            obj2.CastTo(BasicValueType.String);
        
        if (obj1.type == BasicValueType.String)
            return new BasicValue(obj1.str + obj2.str, forceType: BasicValueType.String);
        
        return new BasicValue(obj1.number + obj2.number);
    }

    public static BasicValue operator -(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type != obj2.type)
            throw new Exception($"Invalid operator - (minus) between {obj1.type} and {obj2.type}");
        if (obj1.type == BasicValueType.String)
            throw new Exception($"Invalid operator - (minus) can't substract strings");
        return new BasicValue(obj1.number - obj2.number);
    }

    public static BasicValue operator *(BasicValue obj1, BasicValue obj2)
    {
       
        if (obj1.type == BasicValueType.String && obj2.type == BasicValueType.Number)
            return new BasicValue(string.Concat(Enumerable.Repeat(obj1.str, (int)obj2.number)), forceType: BasicValueType.String);

        if (obj1.type == BasicValueType.Number && obj2.type == BasicValueType.String)
            return new BasicValue(string.Concat(Enumerable.Repeat(obj2.str, (int)obj1.number)), forceType: BasicValueType.String);

        if (obj1.type == BasicValueType.String)
            throw new Exception("Invalid operation: String cannot be multiplied by another string.");

        return new BasicValue(obj1.number * obj2.number);
    }

    public static BasicValue operator /(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type != obj2.type)
            throw new Exception($"Invalid operator / (divide) between {obj1.type} and {obj2.type}");

        if (obj1.type == BasicValueType.String)
            throw new Exception("Invalid operation: String cannot be divided.");

        if (obj2.number == 0)
            throw new Exception("Divide by zero error.");

        return new BasicValue(obj1.number / obj2.number);
    }

    public bool IsTrue()
    {
        return (type == BasicValueType.String ?
                    this != BasicValue.EmptyString :
                    this != BasicValue.False);
    }

    public bool IsFalse()
    {
        return !IsTrue();
    }

    public static BasicValue operator &(BasicValue obj1, BasicValue obj2)
    {
        bool left = obj1.IsTrue();
        bool rigth = obj2.IsTrue();
        if (left && rigth)
            return new BasicValue(1);
        return new BasicValue(0);
    }


    public static BasicValue operator |(BasicValue obj1, BasicValue obj2)
    {
        bool left = obj1.IsTrue();
        bool rigth = obj2.IsTrue();
        if (left || rigth)
            return new BasicValue(1);
        return new BasicValue(0);
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        BasicValue other = (BasicValue)obj;

        if (type != other.type)
            return false;

        if (type == BasicValueType.String)
            return str == other.str;

        return number == other.number;
    }

    public override int GetHashCode()
    {
        if (type == BasicValueType.String)
            return str.GetHashCode();

        return number.GetHashCode();
    }

    public static bool IsValidOperation(string op)
    {
        return validOperations.Contains(op);
    }
    public static bool IsValidHexNumber(string hex)
    {
        if (!hex.StartsWith("&"))
            return false;
        // Check if all characters in the string are valid hex digits
        foreach (char c in hex.Substring(1))
        {
            if (!Uri.IsHexDigit(c))
            {
                return false;
            }
        }
        return true;
    }
    public static bool IsValidNumber(string str)
    {
        return double.TryParse(str, out _);
    }
    public static bool IsValidNumber(double val)
    {
        return true;
    }
    public bool IsNumber()
    {
        return type == BasicValueType.Number;
    }
    public bool IsString()
    {
        return type == BasicValueType.String;
    }
    public static bool IsValidString(string str)
    {
        return str.StartsWith("\"") && str.EndsWith("\"");

    }


    public BasicValue Operate(BasicValue bval, BasicValue operation)
    {
        switch (operation.ToString())
        {

            case "+":
                return this + bval;
            case "-":
                return this - bval;
            case "*":
                return this * bval;
            case "/":
                return this / bval;
            case "=":
                return new BasicValue(bval == this ? 1 : 0);
            case "!=":
            case "<>":
                return new BasicValue(this != bval ? 1 : 0);
            case ">":
                return new BasicValue(this > bval ? 1 : 0);
            case "<":
                return new BasicValue(this < bval ? 1 : 0);
            case "<=":
                return new BasicValue(this <= bval ? 1 : 0);
            case ">=":
                return new BasicValue(this >= bval ? 1 : 0);
            case "&&":
                return this & bval;
            case "||":
                return this | bval;
        }
        throw new Exception($"Operator unknown: [{operation}], allowed values are [{string.Join(", ", validOperations)}]...");
    }

    public override string ToString()
    {
        if (type == BasicValueType.Number)
            return number.ToString();
        else if (type == BasicValueType.String)
            return str;

        return " UNKNOWN ";
    }

    public static bool PrecedenceIsLess(BasicValue left, BasicValue right)
    {
        string opLeft = left.ToString();
        string opRight = right.ToString();
        if (!OperatorPrecedence.ContainsKey(opLeft))
            throw new Exception($"{opLeft} isn't an operator");

        if (!OperatorPrecedence.ContainsKey(opRight))
            throw new Exception($"{opRight} isn't an operator");

        return OperatorPrecedence[opLeft] <= OperatorPrecedence[opRight];
    }

}