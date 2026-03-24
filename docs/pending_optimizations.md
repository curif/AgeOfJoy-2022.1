# Pending AGEBasic Optimizations

This document outlines the remaining architectural optimizations identified to improve the performance of the AGEBasic interpreter, specifically targeting execution speed and Garbage Collection (GC) pressure.

> **All optimizations listed below have been implemented as of 2026-03-23.**

---

## ~~1. Fast Line Lookups (`GOTO`, `GOSUB`, `FOR...NEXT`)~~ ✅ DONE

**The Bottleneck:**
Currently, when the interpreter needs to jump to a specific line number (due to a `GOTO`, `GOSUB`, or the end of a `FOR` loop), it uses the `getNext()` method in `basicAGEProgram.cs`. 
This method iterates through the `SortedDictionary<double, ICommandBase> lines` from the beginning using a `while` loop until it finds a line number greater than or equal to the target `nextLineToExecute`. 

```csharp
// Current implementation in getNext()
IEnumerator<KeyValuePair<double, ICommandBase>> etor = lines.GetEnumerator();
while (etor.MoveNext()) {
    if (etor.Current.Key >= nextLineToExecute) {
        // ... found it
    }
}
```
If a program jumps from line 10,000 back to line 9,000, it performs 9,000 loop iterations *just to find the code to execute*. This is an $O(N)$ operation that severely degrades performance in tight loops.

**The Solution: Binary Search Array**
1. During the `Parse` phase, populate a flat `List<double> parsedLineNumbers` alongside the dictionary.
2. In `getNext()`, when a jump is requested (`nextLineToExecute >= 0`), use `parsedLineNumbers.BinarySearch(nextLineToExecute)`.
3. A binary search is $O(log N)$. Finding line 9,000 out of 10,000 lines will take ~13 operations instead of 9,000.
4. Once the index is found, set the enumerator directly or refactor `getNext()` to use integer indexing rather than dictionary enumeration.

---

## ~~2. Eliminate GC Pressure from `BasicValue` Allocations~~ ✅ DONE (Option B — object pool)

**The Bottleneck:**
`BasicValue` is defined as a `public class BasicValue`. Classes in C# are reference types allocated on the managed heap. 
Every time a math operation occurs, intermediate `BasicValue` objects are instantiated and then immediately discarded.
```csharp
// Example of hidden allocation
public static BasicValue operator +(BasicValue obj1, BasicValue obj2) {
    return new BasicValue(obj1.number + obj2.number); // Allocates to heap
}
```
In a fast loop (e.g., executing 500 lines per frame), this generates thousands of dead objects per second, forcing Unity's Garbage Collector to pause the game (GC Spikes), which causes stuttering in VR.

**The Solution:**
There are two paths to resolve this:

*   **Option A: Convert to `struct` (Highest Performance, Hardest Refactor)**
    Change `public class BasicValue` to `public struct BasicValue`. Structs are allocated on the stack and immediately destroyed when out of scope, producing zero garbage. 
    *Challenge:* Structs cannot inherit from `IEnumerable`, and arrays must still be handled via internal reference pointers. It requires a significant rewrite of how `BasicValue` is passed around (using `ref` or `in` keywords to avoid copying large structs).

*   **Option B: Implement an Object Pool (Easier Refactor)**
    Create a static `BasicValuePool`. Instead of calling `new BasicValue()`, operations request a clean object from the pool: `BasicValue result = BasicValuePool.Get()`. 
    When an expression finishes executing, all intermediate `BasicValues` used during that expression are returned to the pool: `BasicValuePool.Return(result)`. This keeps the heap size completely static.

---

## ~~3. Integer-Based Variable Lookups~~ ✅ DONE

**The Bottleneck:**
In `basicVars.cs`, variables are stored in a `Dictionary<string, BasicVar>`. Every time an expression evaluates a variable, it performs a string-based hash lookup.
```csharp
// Current implementation
vars["MYVAR".ToUpper()].BasicValue = val;
```
String hashing and dictionary lookups are relatively slow compared to flat array access.

**The Solution:**
1. During the parsing phase, build a global `Dictionary<string, int> VariableSymbolTable`.
2. When the parser encounters a variable like `SCORE`, assign it an integer ID (e.g., `0`). 
3. Replace the `Dictionary<string, BasicVar>` in `basicVars.cs` with a flat array `BasicVar[] variables`.
4. The `CommandExpression` (RPN elements) should store the *integer ID* of the variable, not its string name.
5. At runtime, evaluating a variable becomes a direct array access: `return vars[0].BasicValue;`, which is instantaneous.