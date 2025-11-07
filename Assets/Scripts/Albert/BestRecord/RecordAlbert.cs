using UnityEngine;

[CreateAssetMenu(fileName = "RecordAlbert", menuName = "Scriptable Objects/RecordAlbert")]
public class RecordAlbert : ScriptableObject
{
    [SerializeField] private int record;
    public int Record
    {
        get => record;
        set => record = value;
    }
}
