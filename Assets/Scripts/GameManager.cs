using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MapGenerator mapGenerator;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        int[,] map = mapGenerator.map;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
