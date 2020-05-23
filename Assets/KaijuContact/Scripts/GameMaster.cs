using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject[] spawners = new GameObject[8];
    private int wave = -1;
    private int waveCounted = 0;
    [SerializeField] private int[] waveSizes = { 2, 3, 5 };
    [SerializeField] private Text waveCounter;
    private int enemiesAlive = 0;

    void Update() {
        if (enemiesAlive == 0) {
            if (wave < waveSizes.Length) {
                wave++;
            }
            waveCounted++;
            SpawnEnemies();
        }
        waveCounter.text = "Wave: " + waveCounted;
    }

    private void SpawnEnemies() {
        int[] spawnSelection = new int[waveSizes[wave]];
        for (int i = 0; i < waveSizes[wave]; i++) {
            bool unique = false;
            while (!unique) {
                unique = true;
                spawnSelection[i] = Random.Range(0, 8);
                if (i > 0) {
                    for (int j = 0; j < i; j++) {
                        if (spawnSelection[i] == spawnSelection[j]) {
                            unique = false;
                        }
                    }
                }
            }
            Debug.Log(spawnSelection[i]);
            Instantiate(enemyPrefab, spawners[spawnSelection[i]].transform.position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            enemiesAlive++;
        }
    }

    public void EnemyDestroyed() {
        enemiesAlive--;
    }
}
