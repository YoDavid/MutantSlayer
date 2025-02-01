using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LinqFirstPractice : MonoBehaviour
{
    private List<string> NamesList = new List<string> { "David", "Dan", "Ethan", "Roman", "Alexa", "Dor" };

    void Start()
    {
        var namesStartingWithD = NamesList.Where(name => name.StartsWith("D")).ToList();

        Debug.Log("Names starting with 'D':");
        foreach (var name in namesStartingWithD)
        {
            Debug.Log(name);
        }
    }
}
