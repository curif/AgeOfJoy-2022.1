using System.Collections.Generic;

/// <summary>
/// A simple pool of recyclable BasicValue instances for AGEBasic expression evaluation.
/// AGEBasic is single-threaded, so no locking is needed.
/// Arrays are never pooled because they own child references that would corrupt on reuse.
/// </summary>
public static class BasicValuePool
{
    private static readonly Stack<BasicValue> pool = new Stack<BasicValue>(128);

    /// <summary>Returns a BasicValue from the pool, or allocates a new one if the pool is empty.</summary>
    public static BasicValue Get() => pool.Count > 0 ? pool.Pop() : new BasicValue();

    /// <summary>Returns a pooled BasicValue pre-initialized as a shallow copy of <paramref name="source"/>.</summary>
    public static BasicValue GetCopy(BasicValue source)
    {
        BasicValue v = Get();
        v.SetValue(source);
        return v;
    }

    /// <summary>Returns a BasicValue to the pool. Arrays are skipped (they own child references).</summary>
    public static void Return(BasicValue val)
    {
        if (val == null || val.IsArray()) return;
        pool.Push(val);
    }
}
