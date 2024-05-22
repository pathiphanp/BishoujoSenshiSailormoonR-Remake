using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DescriptionData
{
    public int number;
    [TextArea(5, 10)] public string[] description;
}
