using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ÆÌµØ°å£º
public class LayTheFloor : MonoBehaviour
{
    public Vector3 floorCount = Vector3.zero; //¼ÆÊý£»
    public Vector3 floorSize = Vector3.zero; //³ß´ç£»
    public List<Transform> floorTransforms = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("ReLay")]
    private void ReLay()
    {
        floorTransforms.Clear();
        var childrenTransforms = this.GetComponentsInChildren<Transform>();
        for (int i = 1; i < childrenTransforms.Length; ++i)
        {
            if (childrenTransforms[i].gameObject.CompareTag("FloorUnit"))
            {
                floorTransforms.Add(childrenTransforms[i]);
            }
        }

        int currentIndex = 0;
        for (int x = 0; x < floorCount.x; ++x)
        {
            for (int z = 0; z < floorCount.z; ++z)
                if (currentIndex < floorTransforms.Count)
                {
                    Vector3 position = new Vector3(x * floorSize.x, 0, z * floorSize.z);
                    floorTransforms[currentIndex].localPosition = position;
                    ++currentIndex;
                }
        }
    }
}
