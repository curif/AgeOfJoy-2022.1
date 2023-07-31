using System;
using System.Collections.Generic;
using System.IO;

public class TokenConsumer
{
    private string[] tokens;
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

    public TokenConsumer(string[] tokens)
    {
        this.tokens = tokens;
    }

    public bool TheNextIs(string expected)
    {
        if (Remains() < 1)
            return false;
        return tokens[pointer + 1] == expected;
    }

    public string ConsumeIf(string expected)
    {
        if (tokens[pointer] != expected)
            return null;
        return Next();
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

/*
    public List<string> Rest()
    {
        if (Remains() < 1)
            return new List<string>();

        return tokens.GetRange(pointer, tokens.Length - 1);
    }
*/
    //how many tokens left? without count the token pointed by pointer.
    public int Remains()
    {
        if (tokens == null || pointer >= tokens.Length)
        {
            return -1;
        }

        return tokens.Length - pointer - 1;
    }

    public static TokenConsumer operator ++(TokenConsumer tokens)
    {
        tokens.Next();
        return tokens;
    }

    public override string ToString()
    {
        string str = "";
        int idx;
        for (idx = 0; idx < tokens.Length; idx++)
        {
            if (idx == pointer)
                str += " >>" + tokens[idx] + "<<";
            else
                str += "∙" + tokens[idx];
        }
        str += $" [{tokens.Length} tokens]";
        return str;
    }

}