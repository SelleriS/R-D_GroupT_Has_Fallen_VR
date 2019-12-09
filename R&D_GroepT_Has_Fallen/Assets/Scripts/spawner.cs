using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    public Transform[] spawnLocations;
    public GameObject[] whatToSpawnPrefab;
    public GameObject[] whatToSpawnClone;

    private void Start()
    {
        StartCoroutine(waitOneSecond());
    }

    void spawnSomethingAwesomePlease1() {
        whatToSpawnClone[0] = Instantiate(whatToSpawnPrefab[0], spawnLocations[0].transform.position, Quaternion.Euler(0, 0, 0))
        as GameObject;
    }

    void spawnSomethingAwesomePlease2() {
    whatToSpawnClone[1] = Instantiate(whatToSpawnPrefab[1], spawnLocations[1].transform.position, Quaternion.Euler(0, 0, 0))
        as GameObject;
    }

    void spawnSomethingAwesomePlease3() {
        whatToSpawnClone[2] = Instantiate(whatToSpawnPrefab[2], spawnLocations[2].transform.position, Quaternion.Euler(0, 0, 0))
        as GameObject;
    }

    IEnumerator waitOneSecond()
    {
        yield return new WaitForSeconds(1);
        spawnSomethingAwesomePlease1();
        yield return new WaitForSeconds(2);
        spawnSomethingAwesomePlease2();
        yield return new WaitForSeconds(1);
        spawnSomethingAwesomePlease3();
    }
}
