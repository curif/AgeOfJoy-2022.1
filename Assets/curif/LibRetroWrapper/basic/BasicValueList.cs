using System.Collections.Generic;
using System;

public class BasicValueList
{
    private List<BasicValue> values;
    private int currentIndex;

    public BasicValueList()
    {
        this.values = new();
        this.currentIndex = -1;
    }
    public void Add(BasicValue newValue)
    {
        this.values.Add(newValue);
    }

    public void Next()
    {
        if (this.currentIndex < this.values.Count)
            this.currentIndex++;
    }

    public bool EOF()
    {
        return this.currentIndex >= this.values.Count;
    }

    public void Reset()
    {
        this.currentIndex = -1;
    }

    public BasicValue CurrentValue()
    {
        if (this.EOF())
            throw new InvalidOperationException("End of list has been reached.");
        if (this.currentIndex == -1)
            this.currentIndex = 0;
        return this.values[this.currentIndex];
    }
    public int Count()
    {
        return this.values.Count;
    }
    public void JumpTo(int offset)
    {
        if (offset < 0 || offset >= this.values.Count)
            throw new ArgumentOutOfRangeException("Offset is out of range.");
        else
            this.currentIndex = offset;
    }

}
