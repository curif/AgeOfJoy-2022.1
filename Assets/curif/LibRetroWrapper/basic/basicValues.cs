using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


public class BasicValue : IEnumerable<BasicValue>
{
    public enum BasicValueType
    {
        Number,
        String,
        Array, // New type for arrays
        empty // Represents an uninitialized or null-like value
    }

    private string str = "";
    private double number = 0;
    private BasicValue[] arrayValues; // Stores array elements in a flat structure
    private int[] dimensions; // Stores the dimensions of a multidimensional array (e.g., [2, 3] for DIM array[2,3])
    private int[] strides;    // Stores pre-calculated strides for flat index calculation (e.g., [1, D0, D0*D1, ...] for column-major)
    private BasicValueType type = BasicValueType.empty;

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
    public BasicValue() => SetValue(0f);

    // Overloaded constructors for various data types
    public BasicValue(double number) => SetValue(number);
    public BasicValue(int number) => SetValue((double)number);
    public BasicValue(float number) => SetValue((double)number);
    public BasicValue(bool boolean) => SetValue(boolean ? 1.0 : 0.0);
    public BasicValue(BasicValue val) => SetValue(val);
    public BasicValue(string[] val) => SetValue(val);

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

    /// <summary>
    /// Dimensions the BasicValue instance as an N-dimensional array with the specified dimensions.
    /// The total size of the internal flat array is the product of all dimensions.
    /// Each element is initialized to a BasicValue with a numeric value of 0.
    /// Strides are pre-calculated for column-major order.
    /// </summary>
    /// <param name="dimensions">An array of integers specifying the size of each dimension. Must be non-null, non-empty, and all elements must be positive.</param>
    /// <returns>The current BasicValue instance, now dimensioned as an array.</returns>
    public BasicValue Dim(params int[] dimensions)
    {
        if (dimensions == null || dimensions.Length == 0)
        {
            throw new ArgumentNullException(nameof(dimensions), "Array dimensions cannot be null or empty.");
        }

        long totalSize = 1;
        foreach (int dim in dimensions)
        {
            if (dim <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dim), "Array dimensions must be positive integers.");
            }
            totalSize *= dim;
            if (totalSize > int.MaxValue) // Prevent overflow for very large arrays
            {
                throw new ArgumentOutOfRangeException(nameof(dimensions), $"Total array size ({totalSize}) exceeds maximum supported integer size ({int.MaxValue}).");
            }
        }

        this.type = BasicValueType.Array;
        this.dimensions = (int[])dimensions.Clone(); // Store a copy of the dimensions

        // Calculate strides for column-major order:
        // stride[k] = product of dimensions[0]...dimensions[k-1]
        // E.g., for dimensions [D0, D1, D2]:
        // strides[0] = 1
        // strides[1] = D0
        // strides[2] = D0 * D1
        this.strides = new int[dimensions.Length];
        int currentStride = 1;
        for (int i = 0; i < dimensions.Length; i++)
        {
            this.strides[i] = currentStride;
            currentStride *= dimensions[i];
        }

        this.arrayValues = new BasicValue[(int)totalSize];
        // Initialize array elements to default BasicValue (e.g., 0)
        for (int i = 0; i < totalSize; i++)
        {
            this.arrayValues[i] = new BasicValue(0); // Initialize with 0
        }
        this.number = 0; // Clear other types' data
        this.str = "";
        return this; // Allow chaining
    }
    /// <summary>
    /// Dimensions the BasicValue instance as an N-dimensional array using BasicValue instances for dimensions.
    /// Converts BasicValue dimensions to integers. The total size of the internal flat array is the product of all dimensions.
    /// Each element is initialized to a BasicValue with a numeric value of 0.
    /// Strides are pre-calculated for column-major order.
    /// </summary>
    /// <param name="dimensionsBasicValues">An array of BasicValue instances specifying the size of each dimension. Must be non-null, non-empty, and all elements must be convertible to positive integers.</param>
    /// <returns>The current BasicValue instance, now dimensioned as an array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if dimensionsBasicValues array is null or empty.</exception>
    /// <exception cref="InvalidCastException">Thrown if any BasicValue in dimensionsBasicValues cannot be converted to an integer.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if any dimension is non-positive or if the total array size exceeds Int32.MaxValue.</exception>
    public BasicValue Dim(params BasicValue[] dimensionsBasicValues)
    {
        if (dimensionsBasicValues == null || dimensionsBasicValues.Length == 0)
        {
            throw new ArgumentNullException(nameof(dimensionsBasicValues), "Array dimensions cannot be null or empty.");
        }

        // Convert BasicValue dimensions to int array
        int[] intDimensions = new int[dimensionsBasicValues.Length];
        for (int i = 0; i < dimensionsBasicValues.Length; i++)
        {
            if (dimensionsBasicValues[i].Type() != BasicValueType.Number && dimensionsBasicValues[i].Type() != BasicValueType.empty)
            {
                throw new InvalidCastException($"Array dimension must be a number or empty, but got type {dimensionsBasicValues[i].Type()} at dimension position {i}.");
            }
            intDimensions[i] = dimensionsBasicValues[i].GetInt(); // GetInt() handles implicit empty->0 conversion

            // Basic validation for individual dimensions (already covered by the core Dim, but good to check early)
            if (intDimensions[i] <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimensionsBasicValues), $"Array dimension {i} (value: {intDimensions[i]}) must be a positive integer.");
            }
        }

        // Call the core Dim method which handles the actual dimensioning, total size calculation, strides, and initialization
        return Dim(intDimensions);
    }
    /// <summary>
    /// Provides array-like access to elements for the underlying flat array using a single flat index.
    /// This is provided for consistency with `DIM array[N]` as a 1D array.
    /// </summary>
    /// <param name="index">The zero-based flat index of the element to get or set.</param>
    /// <returns>The BasicValue at the specified flat index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if the flat index is out of the bounds of the array.</exception>
    public BasicValue this[int index]
    {
        get
        {
            if (type != BasicValueType.Array)
            {
                throw new InvalidOperationException("Cannot access array elements on a non-array BasicValue type.");
            }
            if (arrayValues == null || index < 0 || index >= arrayValues.Length)
            {
                throw new IndexOutOfRangeException($"Flat array index {index} is out of bounds for total size {arrayValues?.Length ?? 0}.");
            }
            return arrayValues[index];
        }
        set
        {
            if (type != BasicValueType.Array)
            {
                throw new InvalidOperationException("Cannot set array elements on a non-array BasicValue type.");
            }
            if (arrayValues == null || index < 0 || index >= arrayValues.Length)
            {
                throw new IndexOutOfRangeException($"Flat array index {index} is out of bounds for total size {arrayValues?.Length ?? 0}.");
            }
            arrayValues[index] = value;
        }
    }

    /// <summary>
    /// Provides array-like access to elements for multidimensional arrays using multiple indices.
    /// Calculates the flat index from the provided multidimensional indices using pre-calculated strides (column-major order).
    /// Supports flexible indexing where fewer indices than declared dimensions default to 0 for trailing dimensions
    /// (e.g., `array[0]` on a `DIM array[2,2,2]` acts like `array[0,0,0]`).
    /// </summary>
    /// <param name="indices">An array of zero-based indices for each dimension.</param>
    /// <returns>The BasicValue at the specified multidimensional position.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if indices array is null.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if more indices are provided than declared dimensions, or any index is out of bounds for its respective dimension.</exception>
    public BasicValue this[params int[] indices]
    {
        get
        {
            int flatIndex = CalculateFlatIndex(indices); // This method handles all validation
            return arrayValues[flatIndex];
        }
        set
        {
            int flatIndex = CalculateFlatIndex(indices); // This method handles all validation
            arrayValues[flatIndex] = value;
        }
    }
    /// <summary>
    /// Provides array-like access to elements for multidimensional arrays using multiple BasicValue indices.
    /// Converts BasicValue indices to integers and calculates the flat index from them using pre-calculated strides (column-major order).
    /// Supports flexible indexing where fewer BasicValue indices than declared dimensions default to 0 for trailing dimensions
    /// (e.g., `array[0]` on a `DIM array[2,2,2]` acts like `array[0,0,0]`).
    /// </summary>
    /// <param name="indicesBasicValues">An array of zero-based BasicValue indices for each dimension.</param>
    /// <returns>The BasicValue at the specified multidimensional position.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if indicesBasicValues array is null.</exception>
    /// <exception cref="InvalidCastException">Thrown if any BasicValue in indicesBasicValues cannot be converted to an integer.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if more indices are provided than declared dimensions, or any index is out of bounds for its respective dimension.</exception>
    public BasicValue this[params BasicValue[] indicesBasicValues]
    {
        get
        {
            int flatIndex = CalculateFlatIndex(indicesBasicValues); // This method handles all validation and conversion
            return arrayValues[flatIndex];
        }
        set
        {
            int flatIndex = CalculateFlatIndex(indicesBasicValues); // This method handles all validation and conversion
            arrayValues[flatIndex] = value;
        }
    }

    /// <summary>
    /// Calculates the one-dimensional (flat) index from a multidimensional index using pre-calculated strides (column-major order).
    /// </summary>
    /// <param name="indices">The zero-based indices for each dimension. Can have fewer elements than `dimensions.Length`.</param>
    /// <returns>The calculated flat index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type or not properly dimensioned.</exception>
    /// <exception cref="ArgumentNullException">Thrown if indices array is null.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if more indices are provided than declared dimensions, or any provided index is out of bounds.</exception>
    private int CalculateFlatIndex(params int[] indices)
    {
        if (type != BasicValueType.Array || dimensions == null || strides == null || arrayValues == null)
        {
            throw new InvalidOperationException("Cannot access array elements. BasicValue is not an array or not properly dimensioned.");
        }
        if (indices == null)
        {
            throw new ArgumentNullException(nameof(indices), "Indices cannot be null when accessing an array.");
        }

        // Allow fewer indices than dimensions (trailing dimensions implicitly 0), but not more.
        if (indices.Length > dimensions.Length)
        {
            throw new IndexOutOfRangeException($"Too many indices provided ({indices.Length}). This array has {dimensions.Length} dimensions.");
        }

        int flatIndex = 0;
        for (int i = 0; i < indices.Length; i++) // Iterate only through provided indices
        {
            if (indices[i] < 0 || indices[i] >= dimensions[i])
            {
                throw new IndexOutOfRangeException($"Index {indices[i]} is out of bounds for dimension {i} (size {dimensions[i]}).");
            }
            flatIndex += indices[i] * strides[i];
        }

        // Final bounds check on the calculated flat index
        if (flatIndex < 0 || flatIndex >= arrayValues.Length)
        {
            // This should ideally not happen if dimension-wise checks are correct, but serves as a safeguard.
            throw new IndexOutOfRangeException($"Calculated flat index ({flatIndex}) is out of bounds for total array size ({arrayValues.Length}).");
        }

        return flatIndex;
    }
    /// <summary>
    /// Calculates the one-dimensional (flat) index from a multidimensional index represented by BasicValue array.
    /// It converts BasicValue indices to integers and uses pre-calculated strides (column-major order).
    /// </summary>
    /// <param name="indicesBasicValues">The zero-based BasicValue indices for each dimension. Can have fewer elements than `dimensions.Length`.</param>
    /// <returns>The calculated flat index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type or not properly dimensioned.</exception>
    /// <exception cref="ArgumentNullException">Thrown if indicesBasicValues array is null.</exception>
    /// <exception cref="InvalidCastException">Thrown if any BasicValue in indicesBasicValues cannot be converted to an integer.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if more indices are provided than declared dimensions, or any provided index is out of bounds.</exception>
    private int CalculateFlatIndex(BasicValue[] indicesBasicValues)
    {
        if (type != BasicValueType.Array || dimensions == null || strides == null || arrayValues == null)
        {
            throw new InvalidOperationException("Cannot access array elements. BasicValue is not an array or not properly dimensioned.");
        }
        if (indicesBasicValues == null)
        {
            throw new ArgumentNullException(nameof(indicesBasicValues), "Indices cannot be null when accessing an array.");
        }

        // Convert BasicValue indices to int array for processing
        int[] indices = new int[indicesBasicValues.Length];
        for (int i = 0; i < indicesBasicValues.Length; i++)
        {
            if (indicesBasicValues[i].Type() != BasicValueType.Number && indicesBasicValues[i].Type() != BasicValueType.empty)
            {
                throw new InvalidCastException($"Array index must be a number or empty, but got type {indicesBasicValues[i].Type()} at index position {i}.");
            }
            indices[i] = indicesBasicValues[i].GetInt(); // GetInt() handles implicit empty->0 conversion
        }

        // Now, proceed with the existing int-based flat index calculation logic
        // This allows fewer indices than dimensions (trailing dimensions implicitly 0), but not more.
        if (indices.Length > dimensions.Length)
        {
            throw new IndexOutOfRangeException($"Too many indices provided ({indices.Length}). This array has {dimensions.Length} dimensions.");
        }

        int flatIndex = 0;
        for (int i = 0; i < indices.Length; i++) // Iterate only through provided indices
        {
            if (indices[i] < 0 || indices[i] >= dimensions[i])
            {
                throw new IndexOutOfRangeException($"Index {indices[i]} is out of bounds for dimension {i} (size {dimensions[i]}).");
            }
            flatIndex += indices[i] * strides[i];
        }

        // Final bounds check on the calculated flat index
        if (flatIndex < 0 || flatIndex >= arrayValues.Length)
        {
            // This should ideally not happen if dimension-wise checks are correct, but serves as a safeguard.
            throw new IndexOutOfRangeException($"Calculated flat index ({flatIndex}) is out of bounds for total array size ({arrayValues.Length}).");
        }

        return flatIndex;
    }

    public BasicValue SetValue(BasicValue val)
    {
        this.type = val.type;
        this.number = val.number;
        this.str = val.str;

        if (val.type == BasicValueType.Array && val.arrayValues != null)
        {
            // Deep copy the dimensions and strides
            this.dimensions = (val.dimensions != null) ? (int[])val.dimensions.Clone() : null;
            this.strides = (val.strides != null) ? (int[])val.strides.Clone() : null;

            // Deep copy the array elements to ensure independent array instances
            this.arrayValues = new BasicValue[val.arrayValues.Length];
            for (int i = 0; i < val.arrayValues.Length; i++)
            {
                // Recursively set value, handling nested BasicValues correctly
                this.arrayValues[i] = new BasicValue().SetValue(val.arrayValues[i]);
            }
        }
        else
        {
            this.arrayValues = null; // Ensure array data is null if not an array type
            this.dimensions = null; // Clear dimension info
            this.strides = null;    // Clear stride info
        }
        return this;
    }

    public BasicValue SetValue(double val)
    {
        this.number = val;
        type = BasicValueType.Number;
        this.str = ""; // Clear string data
        this.arrayValues = null; // Clear array data
        this.dimensions = null; // Clear dimensions
        this.strides = null;    // Clear stride info
        return this;
    }
    public BasicValue SetValue(int val)
    {
        this.number = (double)val;
        type = BasicValueType.Number;
        this.str = ""; // Clear string data
        this.arrayValues = null; // Clear array data
        this.dimensions = null; // Clear dimensions
        this.strides = null;    // Clear stride info
        return this;
    }
    public BasicValue SetValue(string str)
    {
        this.str = str;
        type = BasicValueType.String;
        this.number = 0; // Clear number data
        this.arrayValues = null; // Clear array data
        this.dimensions = null; // Clear dimensions
        this.strides = null;    // Clear stride info
        return this;
    }

    public double GetNumber()
    {
        // If it's a string that parses to a number, return it as number (implicit conversion)
        if (type == BasicValueType.String)
        {
            if (double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }
            throw new InvalidOperationException($"String value '{str}' cannot be implicitly converted to a number.");
        }
        if (type == BasicValueType.Number)
            return number;
        // If it's an empty value, treat it as 0 for numeric operations
        if (type == BasicValueType.empty)
            return 0;

        throw new InvalidOperationException($"Cannot get number from BasicValue of type {type}.");
    }

    public int GetInt()
    {
        return (int)GetNumber(); // Reuse GetNumber for implicit conversion logic
    }

    public bool GetBoolean()
    {
        if (type == BasicValueType.String)
            return str != null && str != BasicValue.EmptyString.str;
        if (type == BasicValueType.Number)
            return number != 0;
        if (type == BasicValueType.Array)
            return arrayValues != null && arrayValues.Length > 0; // An array is "true" if dimensioned and not empty
        return false; // For BasicValueType.empty
    }

    public string GetString()
    {
        // If it's a number, return it as string (implicit conversion)
        if (type == BasicValueType.Number)
        {
            return number.ToString(CultureInfo.InvariantCulture);
        }
        if (type == BasicValueType.String)
            return str;
        if (type == BasicValueType.empty)
            return ""; // Empty string for empty value

        throw new InvalidOperationException($"Cannot get string from BasicValue of type {type}.");
    }
    public string GetValueAsString()
    {
        return GetString();
    }
    public double GetValueAsNumber()
    {
        return GetNumber();
    }

    public BasicValueType Type()
    {
        return this.type;
    }

    public bool IsTrue()
    {
        return GetBoolean(); // Use GetBoolean for consistency
    }
    public bool IsFalse()
    {
        return !GetBoolean(); // Use GetBoolean for consistency
    }

    public BasicValue CastTo(BasicValueType targetType)
    {
        if (this.type == targetType) return this; // No-op if already target type

        if (this.type == BasicValueType.Number && targetType == BasicValueType.String)
        {
            this.str = number.ToString(CultureInfo.InvariantCulture);
            this.type = BasicValueType.String;
            this.arrayValues = null;
            this.dimensions = null; // Clear dimensions
            this.strides = null;    // Clear strides
        }
        else if (this.type == BasicValueType.String && targetType == BasicValueType.Number)
        {
            double valueDouble;
            bool isParsableToDouble = double.TryParse(this.str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out valueDouble);
            if (!isParsableToDouble)
                throw new InvalidCastException($"String value '{this.str}' can't be casted to double.");

            this.number = valueDouble;
            this.type = BasicValueType.Number;
            this.arrayValues = null;
            this.dimensions = null; // Clear dimensions
            this.strides = null;    // Clear strides
        }
        else if (this.type == BasicValueType.empty && (targetType == BasicValueType.Number || targetType == BasicValueType.String))
        {
            // An empty value can be cast to 0 or ""
            if (targetType == BasicValueType.Number)
            {
                this.SetValue(0);
            }
            else // targetType == BasicValueType.String
            {
                this.SetValue("");
            }
        }
        else
        {
            // Disallow casting to/from array implicitly, and other unsupported casts
            throw new InvalidCastException($"Cannot cast BasicValue of type {this.type} to {targetType}.");
        }
        return this;
    }

    public static bool operator ==(BasicValue obj1, BasicValue obj2)
    {
        if (ReferenceEquals(obj1, obj2))
            return true;

        if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            return false;

        // Use the Equals method which handles all types including Array
        return obj1.Equals(obj2);
    }

    public static bool operator !=(BasicValue obj1, BasicValue obj2)
    {
        return !(obj1 == obj2);
    }

    public static bool operator >(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Comparison operations (> < >= <=) are not supported directly on array types.");
        }

        if (obj1.type != obj2.type)
        {
            // Attempt implicit conversion if one is number and other is string
            if (obj1.type == BasicValueType.Number && obj2.type == BasicValueType.String)
            {
                return obj1.number > obj2.GetNumber(); // Try to convert string to number
            }
            else if (obj1.type == BasicValueType.String && obj2.type == BasicValueType.Number)
            {
                return obj1.GetNumber() > obj2.number; // Try to convert string to number
            }
            throw new InvalidOperationException($"Invalid comparison between {obj1.type} and {obj2.type}.");
        }

        if (obj1.type == BasicValueType.Number)
            return obj1.number > obj2.number;

        return string.Compare(obj1.str, obj2.str, StringComparison.Ordinal) > 0; // Use Ordinal for consistent string comparison
    }
    public static bool operator <(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Comparison operations (> < >= <=) are not supported directly on array types.");
        }

        if (obj1.type != obj2.type)
        {
            if (obj1.type == BasicValueType.Number && obj2.type == BasicValueType.String)
            {
                return obj1.number < obj2.GetNumber();
            }
            else if (obj1.type == BasicValueType.String && obj2.type == BasicValueType.Number)
            {
                return obj1.GetNumber() < obj2.number;
            }
            throw new InvalidOperationException($"Invalid comparison between {obj1.type} and {obj2.type}.");
        }

        if (obj1.type == BasicValueType.Number)
            return obj1.number < obj2.number;

        return string.Compare(obj1.str, obj2.str, StringComparison.Ordinal) < 0;
    }
    public static bool operator >=(BasicValue obj1, BasicValue obj2)
    {
        // Rely on overloaded operators for robustness
        return (obj1 > obj2) || (obj1 == obj2);
    }
    public static bool operator <=(BasicValue obj1, BasicValue obj2)
    {
        // Rely on overloaded operators for robustness
        return obj1 < obj2 || obj1 == obj2;
    }

    public static BasicValue operator +(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Arithmetic operation (+) not supported on array types.");
        }

        // If either operand is a string, perform string concatenation
        if (obj1.type == BasicValueType.String || obj2.type == BasicValueType.String)
        {
            // Convert numbers to strings for concatenation
            string s1 = (obj1.type == BasicValueType.Number) ? obj1.number.ToString(CultureInfo.InvariantCulture) : obj1.str;
            string s2 = (obj2.type == BasicValueType.Number) ? obj2.number.ToString(CultureInfo.InvariantCulture) : obj2.str;
            return new BasicValue(s1 + s2, forceType: BasicValueType.String);
        }

        // If both are numbers, perform numeric addition
        return new BasicValue(obj1.number + obj2.number);
    }

    public static BasicValue operator -(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Arithmetic operation (-) not supported on array types.");
        }
        // Attempt implicit conversion to number for both operands
        double n1 = obj1.GetNumber();
        double n2 = obj2.GetNumber();
        return new BasicValue(n1 - n2);
    }

    public static BasicValue operator *(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Arithmetic operation (*) not supported on array types.");
        }

        // String repetition (e.g., "abc" * 3 = "abcabcabc")
        if (obj1.type == BasicValueType.String && obj2.type == BasicValueType.Number)
            return new BasicValue(string.Concat(Enumerable.Repeat(obj1.str, (int)obj2.number)), forceType: BasicValueType.String);

        if (obj1.type == BasicValueType.Number && obj2.type == BasicValueType.String)
            return new BasicValue(string.Concat(Enumerable.Repeat(obj2.str, (int)obj1.number)), forceType: BasicValueType.String);

        // Regular multiplication if both can be treated as numbers
        double n1 = obj1.GetNumber();
        double n2 = obj2.GetNumber();
        return new BasicValue(n1 * n2);
    }

    public static BasicValue operator /(BasicValue obj1, BasicValue obj2)
    {
        if (obj1.type == BasicValueType.Array || obj2.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Arithmetic operation (/) not supported on array types.");
        }

        // Attempt implicit conversion to number for both operands
        double n1 = obj1.GetNumber();
        double n2 = obj2.GetNumber();

        if (n2 == 0)
            throw new DivideByZeroException("Divide by zero error.");

        return new BasicValue(n1 / n2);
    }

    // Logical AND
    public static BasicValue operator &(BasicValue obj1, BasicValue obj2)
    {
        bool left = obj1.GetBoolean();
        bool right = obj2.GetBoolean();
        if (left && right)
            return new BasicValue(1);
        return new BasicValue(0);
    }

    // Logical OR
    public static BasicValue operator |(BasicValue obj1, BasicValue obj2)
    {
        bool left = obj1.GetBoolean();
        bool right = obj2.GetBoolean();
        if (left || right)
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

        if (type == BasicValueType.Number)
            return number == other.number;

        if (type == BasicValueType.Array)
        {
            // Compare dimensions first
            if (dimensions == null && other.dimensions == null) { /* continue, both are null/empty */ }
            else if (dimensions == null || other.dimensions == null) return false;
            else if (!dimensions.SequenceEqual(other.dimensions)) return false; // Compare dimension arrays

            // Then compare strides (should be consistent with dimensions, but good for robust equality)
            if (strides == null && other.strides == null) { /* continue */ }
            else if (strides == null || other.strides == null) return false;
            else if (!strides.SequenceEqual(other.strides)) return false;

            // Then compare flat array values
            if (arrayValues == null && other.arrayValues == null) return true;
            if (arrayValues == null || other.arrayValues == null) return false;
            if (arrayValues.Length != other.arrayValues.Length) return false;

            for (int i = 0; i < arrayValues.Length; i++)
            {
                // Recursive call to Equals for elements
                if (!arrayValues[i].Equals(other.arrayValues[i]))
                {
                    return false;
                }
            }
            return true;
        }

        // For BasicValueType.empty, they are equal only if both are empty
        return type == BasicValueType.empty;
    }

    public override int GetHashCode()
    {
        if (type == BasicValueType.String)
            return str.GetHashCode();
        if (type == BasicValueType.Number)
            return number.GetHashCode();
        if (type == BasicValueType.Array)
        {
            if (arrayValues == null) return 0;
            unchecked // Allow overflow for hash combining
            {
                int hash = 17;
                hash = hash * 23 + type.GetHashCode();
                hash = hash * 23 + arrayValues.Length.GetHashCode();

                // Incorporate dimensions into hash code
                if (dimensions != null)
                {
                    foreach (int dim in dimensions)
                    {
                        hash = hash * 23 + dim.GetHashCode();
                    }
                }
                // Incorporate strides into hash code for robustness
                if (strides != null)
                {
                    foreach (int stride in strides)
                    {
                        hash = hash * 23 + stride.GetHashCode();
                    }
                }

                // Combine hashes of a few elements for performance.
                int elementsToHash = Math.Min(arrayValues.Length, 5); // Hash up to first 5 elements
                for (int i = 0; i < elementsToHash; i++)
                {
                    hash = hash * 23 + (arrayValues[i] != null ? arrayValues[i].GetHashCode() : 0);
                }
                return hash;
            }
        }
        return 0; // For BasicValueType.empty
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
        return true; // Any double is a valid number value
    }
    public bool IsNumber()
    {
        return type == BasicValueType.Number;
    }
    public bool IsString()
    {
        return type == BasicValueType.String;
    }

    /// <summary>
    /// Indicates if the BasicValue is of type Array.
    /// </summary>
    public bool IsArray()
    {
        return type == BasicValueType.Array;
    }

    public static bool IsValidString(string str)
    {
        return str.StartsWith("\"") && str.EndsWith("\"");

    }

    /// <summary>
    /// Returns the total number of elements in the array if the BasicValue is an Array type (flat length).
    /// </summary>
    /// <returns>The total flat length of the array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    public int GetArrayLength()
    {
        if (type != BasicValueType.Array)
        {
            throw new InvalidOperationException("Cannot get length of a non-array BasicValue type.");
        }
        return arrayValues?.Length ?? 0;
    }

    /// <summary>
    /// Returns the array's dimensions if the BasicValue is an Array type.
    /// </summary>
    /// <returns>A copy of the array's dimensions (e.g., [2,2] for a 2x2 array).</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    public int[] GetArrayDimensions()
    {
        if (type != BasicValueType.Array)
        {
            throw new InvalidOperationException("Cannot get dimensions of a non-array BasicValue type.");
        }
        return (dimensions != null) ? (int[])dimensions.Clone() : new int[0];
    }


    public BasicValue Operate(BasicValue bval, BasicValue operation)
    {
        // Convert operation BasicValue to string to use in switch
        string op = operation.ToString();

        switch (op)
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
            default:
                throw new Exception($"Operator unknown: [{op}], allowed values are [{string.Join(", ", validOperations)}]...");
        }
    }

    public override string ToString()
    {
        if (type == BasicValueType.Number)
            return number.ToString(CultureInfo.InvariantCulture); // Use InvariantCulture for consistent number string representation
        else if (type == BasicValueType.String)
            return str;
        else if (type == BasicValueType.Array)
        {
            // If the array has defined dimensions, show them. Otherwise, just the flat length.
            if (dimensions != null && dimensions.Length > 0)
            {
                string dimStr = string.Join(",", dimensions.Select(d => d.ToString()));
                return $"Array[{dimStr}] (flat length: {arrayValues?.Length ?? 0})";
            }
            else // Should ideally not happen if Dim is always used with dimensions
            {
                return $"Array[] (flat length: {arrayValues?.Length ?? 0})";
            }
        }
        else if (type == BasicValueType.empty)
            return "EMPTY"; // More descriptive than "UNKNOWN" for 'empty'
        return "UNKNOWN";
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
    // New methods for basic operations (modifying the current instance)

    /// <summary>
    /// Adds a double value to the current BasicValue instance.
    /// If the current value is numeric, performs numeric addition.
    /// If the current value is a string, converts both to string and performs concatenation.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Add(double value)
    {
        return Add(new BasicValue(value));
    }

    /// <summary>
    /// Adds an int value to the current BasicValue instance.
    /// Behaves like Add(double value).
    /// </summary>
    public BasicValue Add(int value)
    {
        return Add((double)value);
    }

    /// <summary>
    /// Concatenates a string value to the current BasicValue instance.
    /// Converts the current value to string if it's a number, then concatenates.
    /// Mutates the current instance and sets its type to String.
    /// </summary>
    public BasicValue Add(string value)
    {
        // Force the input to be treated as a string, preventing numeric parsing
        return Add(new BasicValue(value, BasicValueType.String));
    }

    /// <summary>
    /// Performs addition or string concatenation with another BasicValue instance.
    /// If either operand is a string, performs string concatenation. Otherwise, performs numeric addition.
    /// Mutates the current instance and returns it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if operation is attempted on an array type.</exception>
    public BasicValue Add(BasicValue other)
    {
        if (this.type == BasicValueType.Array || other.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Operation 'Add' not supported on array types.");
        }

        // If either operand is a string, perform string concatenation
        if (this.type == BasicValueType.String || other.type == BasicValueType.String)
        {
            string s1 = this.GetString(); // Handles implicit number to string conversion
            string s2 = other.GetString(); // Handles implicit number to string conversion
            SetValue(s1 + s2); // Mutate this instance, type becomes String
        }
        else // Both are numbers or empty (empty defaults to 0 for numeric ops)
        {
            double n1 = this.GetNumber(); // Handles empty -> 0
            double n2 = other.GetNumber();
            SetValue(n1 + n2); // Mutate this instance, type becomes Number
        }
        return this;
    }

    /// <summary>
    /// Subtracts a double value from the current BasicValue instance.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Subtract(double value)
    {
        return Subtract(new BasicValue(value));
    }

    /// <summary>
    /// Subtracts an int value from the current BasicValue instance.
    /// Behaves like Subtract(double value).
    /// </summary>
    public BasicValue Subtract(int value)
    {
        return Subtract((double)value);
    }

    /// <summary>
    /// Performs subtraction with another BasicValue instance.
    /// Mutates the current instance and returns it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if operation is attempted on an array type.</exception>
    public BasicValue Subtract(BasicValue other)
    {
        if (this.type == BasicValueType.Array || other.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Operation 'Subtract' not supported on array types.");
        }
        double n1 = this.GetNumber();
        double n2 = other.GetNumber();
        SetValue(n1 - n2); // Mutate this instance, type becomes Number
        return this;
    }

    /// <summary>
    /// Multiplies the current BasicValue instance by a double value.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Multiply(double value)
    {
        return Multiply(new BasicValue(value));
    }

    /// <summary>
    /// Multiplies the current BasicValue instance by an int value.
    /// Behaves like Multiply(double value).
    /// </summary>
    public BasicValue Multiply(int value)
    {
        return Multiply((double)value);
    }

    /// <summary>
    /// Multiplies the current BasicValue instance by another BasicValue instance.
    /// Handles numeric multiplication or string repetition (e.g., "abc" * 3).
    /// Mutates the current instance and returns it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if operation is attempted on an array type.</exception>
    public BasicValue Multiply(BasicValue other)
    {
        if (this.type == BasicValueType.Array || other.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Operation 'Multiply' not supported on array types.");
        }

        // String repetition: "abc" * 3
        if (this.type == BasicValueType.String && other.type == BasicValueType.Number)
        {
            SetValue(string.Concat(Enumerable.Repeat(this.str, (int)other.number)));
        }
        // String repetition: 3 * "abc" (reverse order)
        else if (this.type == BasicValueType.Number && other.type == BasicValueType.String)
        {
            SetValue(string.Concat(Enumerable.Repeat(other.str, (int)this.number)));
        }
        else // Regular multiplication if both can be treated as numbers
        {
            double n1 = this.GetNumber();
            double n2 = other.GetNumber();
            SetValue(n1 * n2);
        }
        return this;
    }

    /// <summary>
    /// Divides the current BasicValue instance by a double value.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Divide(double value)
    {
        return Divide(new BasicValue(value));
    }

    /// <summary>
    /// Divides the current BasicValue instance by an int value.
    /// Behaves like Divide(double value).
    /// </summary>
    public BasicValue Divide(int value)
    {
        return Divide((double)value);
    }

    /// <summary>
    /// Performs division with another BasicValue instance.
    /// Mutates the current instance and returns it.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if operation is attempted on an array type.</exception>
    /// <exception cref="DivideByZeroException">Thrown if the divisor is zero.</exception>
    public BasicValue Divide(BasicValue other)
    {
        if (this.type == BasicValueType.Array || other.type == BasicValueType.Array)
        {
            throw new InvalidOperationException("Operation 'Divide' not supported on array types.");
        }
        double n1 = this.GetNumber();
        double n2 = other.GetNumber();

        if (n2 == 0)
            throw new DivideByZeroException("Divide by zero error.");

        SetValue(n1 / n2);
        return this;
    }

    /// <summary>
    /// Sets the current BasicValue instance to the value of another BasicValue, mutating itself.
    /// This is an alias for SetValue(BasicValue value) to fit the requested pattern.
    /// </summary>
    public BasicValue Assign(BasicValue value)
    {
        return SetValue(value);
    }

    /// <summary>
    /// Sets the current BasicValue instance to a boolean value, mutating itself.
    /// True maps to 1, False maps to 0, and the type becomes Number.
    /// </summary>
    public BasicValue SetBoolean(bool value)
    {
        SetValue(value ? 1.0 : 0.0);
        return this;
    }

    /// <summary>
    /// Performs a comparison to check if the current BasicValue is equal to another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsEqualTo(BasicValue other)
    {
        SetBoolean(this.Equals(other)); // Use Equals method, which handles all types
        return this;
    }

    /// <summary>
    /// Performs a comparison to check if the current BasicValue is NOT equal to another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsNotEqualTo(BasicValue other)
    {
        SetBoolean(!this.Equals(other));
        return this;
    }

    /// <summary>
    /// Performs a greater-than comparison with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsGreaterThan(BasicValue other)
    {
        SetBoolean(this > other); // Uses the overloaded operator for consistency
        return this;
    }

    /// <summary>
    /// Performs a less-than comparison with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsLessThan(BasicValue other)
    {
        SetBoolean(this < other); // Uses the overloaded operator for consistency
        return this;
    }

    /// <summary>
    /// Performs a greater-than-or-equal-to comparison with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsGreaterThanOrEqualTo(BasicValue other)
    {
        SetBoolean(this >= other); // Uses the overloaded operator for consistency
        return this;
    }

    /// <summary>
    /// Performs a less-than-or-equal-to comparison with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the result.
    /// </summary>
    public BasicValue IsLessThanOrEqualTo(BasicValue other)
    {
        SetBoolean(this <= other); // Uses the overloaded operator for consistency
        return this;
    }

    /// <summary>
    /// Performs a logical AND operation with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the boolean result.
    /// </summary>
    public BasicValue LogicalAnd(BasicValue other)
    {
        SetBoolean(this.GetBoolean() && other.GetBoolean());
        return this;
    }

    /// <summary>
    /// Performs a logical OR operation with another BasicValue,
    /// and sets the current instance to 1 (True) or 0 (False) based on the boolean result.
    /// </summary>
    public BasicValue LogicalOr(BasicValue other)
    {
        SetBoolean(this.GetBoolean() || other.GetBoolean());
        return this;
    }

    /// <summary>
    /// Negates the numeric value of the current BasicValue instance.
    /// Throws InvalidOperationException if the current value is not numeric or empty.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Negate()
    {
        // GetNumber handles implicit conversion of empty to 0
        if (this.type != BasicValueType.Number && this.type != BasicValueType.empty)
        {
            throw new InvalidOperationException("Unary negation not supported on non-numeric or array types.");
        }
        SetValue(-this.GetNumber());
        return this;
    }

    /// <summary>
    /// Performs a logical NOT operation on the current BasicValue instance.
    /// Sets the current instance to 1 (True) or 0 (False) based on the boolean result.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Not()
    {
        SetBoolean(!this.GetBoolean());
        return this;
    }

    /// <summary>
    /// Increments the numeric value of the current BasicValue instance by 1.
    /// Throws InvalidOperationException if the current value is not numeric or empty.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Increment()
    {
        // GetNumber handles implicit conversion of empty to 0
        if (this.type != BasicValueType.Number && this.type != BasicValueType.empty)
        {
            throw new InvalidOperationException("Increment operation not supported on non-numeric or array types.");
        }
        SetValue(this.GetNumber() + 1);
        return this;
    }

    /// <summary>
    /// Decrements the numeric value of the current BasicValue instance by 1.
    /// Throws InvalidOperationException if the current value is not numeric or empty.
    /// Mutates the current instance and returns it.
    /// </summary>
    public BasicValue Decrement()
    {
        // GetNumber handles implicit conversion of empty to 0
        if (this.type != BasicValueType.Number && this.type != BasicValueType.empty)
        {
            throw new InvalidOperationException("Decrement operation not supported on non-numeric or array types.");
        }
        SetValue(this.GetNumber() - 1);
        return this;
    }
    /// <summary>
    /// Sets the current BasicValue instance to an array of strings, mutating itself.
    /// Each string in the input array will be converted to a BasicValue and stored.
    /// The BasicValue will be dimensioned as a 1-dimensional array.
    /// </summary>
    /// <param name="val">The string array to assign.</param>
    /// <returns>The current BasicValue instance, now representing the string array.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the input string array is null.</exception>
    public BasicValue SetValue(string[] val)
    {
        if (val == null)
        {
            throw new ArgumentNullException(nameof(val), "Cannot set BasicValue array from a null string array.");
        }

        this.type = BasicValueType.Array;
        this.number = 0; // Clear number data
        this.str = "";   // Clear string data

        this.dimensions = new int[] { val.Length }; // 1-dimensional array
        this.strides = new int[] { 1 };             // Stride for 1D array is 1

        this.arrayValues = new BasicValue[val.Length];
        for (int i = 0; i < val.Length; i++)
        {
            // Convert each string to a BasicValue.
            // Using new BasicValue(string) constructor will parse numbers if applicable,
            // or store as string if not.
            this.arrayValues[i] = new BasicValue(val[i]);
        }
        return this;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the elements of the BasicValue array.
    /// This allows the BasicValue instance to be used in a foreach loop if it is an array.
    /// </summary>
    /// <returns>An IEnumerator<BasicValue> that can be used to iterate through the array elements.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BasicValue is not an Array type.</exception>
    public IEnumerator<BasicValue> GetEnumerator()
    {
        if (type != BasicValueType.Array)
        {
            throw new InvalidOperationException("Cannot iterate over a non-array BasicValue type. Use IsArray() to check type before iterating.");
        }
        if (arrayValues == null)
        {
            // An empty array (e.g., Dim(0)) might have arrayValues == null or length 0.
            // In either case, the loop won't run, which is correct.
            // If it's a truly uninitialized array, this is handled by the type check above.
            yield break; // Return an empty enumerator
        }

        foreach (BasicValue val in arrayValues)
        {
            yield return val;
        }
    }

    /// <summary>
    /// Returns a non-generic enumerator that iterates through the elements of the BasicValue array.
    /// This is an explicit implementation of the non-generic IEnumerable.GetEnumerator.
    /// </summary>
    /// <returns>A non-generic IEnumerator that can be used to iterate through the array elements.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator(); // Simply calls the generic GetEnumerator()
    }

    // Convenience overloads for comparison methods to accept primitive types directly
    public BasicValue IsEqualTo(double value) { return IsEqualTo(new BasicValue(value)); }
    public BasicValue IsNotEqualTo(double value) { return IsNotEqualTo(new BasicValue(value)); }
    public BasicValue IsGreaterThan(double value) { return IsGreaterThan(new BasicValue(value)); }
    public BasicValue IsLessThan(double value) { return IsLessThan(new BasicValue(value)); }
    public BasicValue IsGreaterThanOrEqualTo(double value) { return IsGreaterThanOrEqualTo(new BasicValue(value)); }
    public BasicValue IsLessThanOrEqualTo(double value) { return IsLessThanOrEqualTo(new BasicValue(value)); }

    public BasicValue IsEqualTo(string value) { return IsEqualTo(new BasicValue(value, BasicValueType.String)); }
    public BasicValue IsNotEqualTo(string value) { return IsNotEqualTo(new BasicValue(value, BasicValueType.String)); }

    public BasicValue LogicalAnd(bool value) { return LogicalAnd(new BasicValue(value)); }
    public BasicValue LogicalOr(bool value) { return LogicalOr(new BasicValue(value)); }
}