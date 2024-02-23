using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

[Serializable]
public class CabinetSpaceType
{
    public string OccupiedSpace = "1x1x2"; // Real occupied space (width x length x high)
    public string MaxAllowedSpace = "1x1x2"; // Max space allowed
    List<int> MaxAllowedSpaceAsInt;

    // Dictionary to store valid combinations of occupied and maximum allowed spaces
    private static Dictionary<string, HashSet<string>> validCombinations = new Dictionary<string, HashSet<string>>()
    {
        {"2x2x2", new HashSet<string>{"2x2x2"}},
        {"1x2x2", new HashSet<string>{"1x2x2", "2x2x2"}},
        {"2x1x2", new HashSet<string>{"2x1x2", "2x2x2"}},
        {"1x1x2", new HashSet<string>{"1x1x2", "1x2x2", "2x1x2", "2x2x2"}},
        {"2x2x1", new HashSet<string>{"2x2x1", "2x2x2"}},
        {"2x1x1", new HashSet<string>{"2x1x1", "2x1x2", "2x2x1", "2x2x2"}},
        {"1x2x1", new HashSet<string>{"1x2x1", "1x2x2", "2x2x1", "2x2x2"}},
        {"1x1x1", new HashSet<string>{"1x1x1", "1x1x2", "1x2x1", "1x2x2", "2x1x1", "2x1x2", "2x2x1", "2x2x2"}}
    };


    // Method to check if the given occupied space fits within the maximum allowed space
    public bool Fit(string occupiedSpace)
    {
        if (validCombinations.ContainsKey(occupiedSpace))
            return validCombinations[occupiedSpace].Contains(MaxAllowedSpace);
        else
            return false;
    }

    // Method to check if the occupied space fits within the maximum allowed space
    public bool Fit()
    {
        return validCombinations[OccupiedSpace].Contains(MaxAllowedSpace);
    }

    // Method to check if the given space parameter exists in the valid combinations
    public static bool IsValidSpace(string space)
    {
        return validCombinations.ContainsKey(space);
    }

    public static string GetValidSpaceTypes()
    {
        return string.Join(", ", validCombinations.Keys);
    }

    public int BestFit(List<string> cabinetsSpace)
    {
        if (MaxAllowedSpaceAsInt == null)
            MaxAllowedSpaceAsInt = MaxAllowedSpace.Split("x").Select(c => int.Parse(c)).ToList();

        int maxFittingDims = 0;
        int idxMaxFittingDims = -1;
        for (int idx = 0; idx < cabinetsSpace.Count; idx++)
        {
            if (Fit(cabinetsSpace[idx]))
            {
                List<int> occu = cabinetsSpace[idx].Split("x").Select(c => int.Parse(c)).ToList();
                int countDims = 0;
                for (int pos = 0; pos < 3; pos++)
                {
                    if (occu[pos] == MaxAllowedSpaceAsInt[pos])
                        countDims++;
                }
                if (countDims == 3)
                    return idx;
                if (countDims > maxFittingDims)
                {
                    maxFittingDims = countDims;
                    idxMaxFittingDims = idx;
                }
            }
        }
        return idxMaxFittingDims;
    }

}