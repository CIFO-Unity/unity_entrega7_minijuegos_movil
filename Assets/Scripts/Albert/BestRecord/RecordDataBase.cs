using UnityEngine;

public class RecordDataBase : MonoBehaviour
{
    [SerializeField]
    private RecordAlbert r;
    private int record;

    void Start()
    {
        record = r.Record;
    }

    // Guardar el récord si es mejor que el récord actual
    public void GuardarRecord(int nuevoRecord)
    {
        if (nuevoRecord > r.Record)
        {
            r.Record = nuevoRecord;
        }
    }
    
    public int RetornarRecord()
    {
        return record;
    }
}