using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SSI2Showcase_TargetCreator : MonoBehaviour
{
    public Transform targetPrefab;
    public int targetCount = 10;
    public float xRange = 15;
    public float yRange = 5;
    public float zRange = 15;

    int targetsDestroyed = 0;
    public Text scoreText;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < targetCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(-xRange, xRange), 5 + Random.Range(0, yRange), Random.Range(-zRange, zRange));
            Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
            Transform targetClone = Instantiate(targetPrefab, randomPos, Quaternion.Euler(randomRot));
            targetClone.GetComponent<SSI2Showcase_Target>().creator = transform;
        }
    }

    void Dead()
    {
        targetsDestroyed++;
        scoreText.text = $"Score: {targetsDestroyed}";
        Vector3 randomPos = new Vector3(Random.Range(-xRange, xRange), 5 + Random.Range(0, yRange), Random.Range(-zRange, zRange));
        Vector3 randomRot = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
        Transform targetClone = Instantiate(targetPrefab, randomPos, Quaternion.Euler(randomRot));
        targetClone.GetComponent<SSI2Showcase_Target>().creator = transform;
    }
}
