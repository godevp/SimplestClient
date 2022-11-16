using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayButton : MonoBehaviour
{
    public List<int> whoMoved;
    public List<int> whereMoved;

    private void Start()
    {
        whoMoved = new List<int>();
        whereMoved = new List<int>();
    }
}
