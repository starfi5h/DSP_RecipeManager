using System;
using System.Collections.Generic;

namespace RecipeManager
{
    [Serializable]
    public class DictWrapper
    {
        public List<int> recipeId;
        public List<int> gridIndex;

        public void FromDict(Dictionary<int, int> dict)
        {
            recipeId = new();
            gridIndex = new();
            foreach (var pair in dict)
            {
                recipeId.Add(pair.Key);
                gridIndex.Add(pair.Value);
            }
        }

        public Dictionary<int, int> ToDict()
        {
            var dict = new Dictionary<int, int>();
            if (recipeId == null || gridIndex == null) return dict;
            var length = Math.Min(recipeId.Count, gridIndex.Count);
            for (int i = 0; i < length; i++)
            {
                dict.Add(recipeId[i], gridIndex[i]);
            }
            return dict;
        }
    }
}
