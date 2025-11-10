using UnityEngine;

public class Earth : MonoBehaviour
{
    [SerializeField]
    private GameObject explosions;

    [SerializeField]
    private GameObject soundExplosionEarth;

    [SerializeField]
    private GameObject accumulator;

    private int indexExplosionsArray;

    void Start()
    {
        indexExplosionsArray = 0;
    }

    private void HideExplosion()
    {
        for (int i = 0; i < explosions.gameObject.transform.childCount; i++)
        {
            explosions.gameObject.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignorar colisiones con misiles y naves (locales y remotas)
        string tag = other.gameObject.tag;
        if (tag == "MissilePlayer" || tag == "Player" || tag == "Albert" || tag == "Lorena")
        {
            return; // Ignorar estas colisiones
        }

        soundExplosionEarth.gameObject.GetComponent<AudioSource>().Play();
        explosions.gameObject.transform.GetChild(indexExplosionsArray).transform.position = other.gameObject.transform.position;
        explosions.gameObject.transform.GetChild(indexExplosionsArray).gameObject.SetActive(true);

        Invoke("HideExplosion", 0.5f);

        indexExplosionsArray++;
        if (indexExplosionsArray >= explosions.transform.childCount)
            indexExplosionsArray = 0;

        //Destroy(other.gameObject);
        //Destroy(this.gameObject);

        //accumulator.gameObject.GetComponent<UIController>().GameOver();
    }
}
