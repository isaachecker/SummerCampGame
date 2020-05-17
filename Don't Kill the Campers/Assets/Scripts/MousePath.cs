using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePath : MonoBehaviour
{ 
    private SimplePF2D.Path path;
    // Start is called before the first frame update
    void Start()
    {
        SimplePathFinding2D pf = GameObject.Find("Grid").GetComponent<SimplePathFinding2D>();
        path = new SimplePF2D.Path(pf);
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
