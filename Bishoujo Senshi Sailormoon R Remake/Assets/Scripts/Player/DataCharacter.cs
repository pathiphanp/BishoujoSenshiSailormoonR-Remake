using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "DataCharacter")]
public class DataCharacter : ScriptableObject
{
    public int hp;
    public int damage;
    public int damageSpecial;
    public Sprite icon;
    public Hptye hptye;
}
