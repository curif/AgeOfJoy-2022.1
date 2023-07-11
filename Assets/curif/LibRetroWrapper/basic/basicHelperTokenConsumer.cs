using System;
using System.Collections.Generic;
using System.IO;

public class TokenConsumer
{
    private List<string> tokens;
    private int pointer = 0;
    private int consumed = 0;

    public int Consumed
    {
        get
        {
            return consumed;
        }
    }

    public string Token
    {
        get
        {
            return tokens[pointer];
        }
    }

    public int position
    {
        get
        {
            return pointer;
        }
    }

    public TokenConsumer(List<string> tokens)
    {
        this.tokens = tokens;
    }

    public bool TheNextIs(string expected)
    {
        if (Remains() < 1)
            return false;
        return tokens[pointer + 1] == expected;
    }

    public string Next(string expected = null)
    {
        if (Remains() < 1)
            return null; //end

        if (expected != null && tokens[pointer + 1] != expected)
            return null;

        pointer++;
        consumed++;

        return tokens[pointer];
    }

    public string Actual()
    {
        return this.Token;
    }

    public List<string> Rest()
    {
        if (Remains() < 1)
            return new List<string>();

        return tokens.GetRange(pointer, tokens.Count - 1);
    }

    //how many tokens left? without count the token pointed by pointer.
    public int Remains()
    {
        if (tokens == null || pointer >= tokens.Count)
        {
            return -1;
        }

        return tokens.Count - pointer - 1;
    }

    public static TokenConsumer operator ++(TokenConsumer tokens)
    {
        tokens.Next();
        return tokens;
    }

    public override string ToString()
    {
        return string.Join(" ", tokens);
    }

}