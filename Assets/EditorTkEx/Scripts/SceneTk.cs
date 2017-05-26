using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorTkEx
{
    public class SceneTk : MonoBehaviour
    {
        /// <summary>
        /// Create a grid of objects based on a sample object
        /// </summary>
        /// <param name="sampleObject">sample object used as template</param>
        /// <param name="xCount">number of copies along the x axis</param>
        /// <param name="yCount">number of copies along the y axis</param>
        /// <param name="zCount">number of copies along the z axis</param>
        /// <param name="gridSpacing">spacing in the grid along the 3 axis</param>
        /// <param name="duplicateSampleObject">if true a copy of the sample object is placed overlapped on it</param>
        /// <returns>List of created objects (if duplicateSampleObject is false the sample object is put at index 0)</returns>
        /// <remarks>Not yet tested.</remarks>
        public static GameObject[] CreateGridOfObjects(GameObject sampleObject,
            int xCount, int yCount, int zCount, Vector3 gridSpacing, bool duplicateSampleObject = false)
        {
            if(xCount<1||yCount<1||yCount<1)
            {
                return null;
            }
            GameObject[] objArray = new GameObject[xCount * yCount * zCount];
            int objCount = 0;
            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int z = 0; z < zCount; z++)
                    {
                        if (objCount == 0 && !duplicateSampleObject)
                        {
                            objArray[0] = sampleObject;
                        }
                        else
                        {
                            Vector3 pos = sampleObject.transform.position;
                            pos.x += gridSpacing.x * x;
                            pos.y += gridSpacing.y * y;
                            pos.z += gridSpacing.z * z;
                            GameObject go = Instantiate(sampleObject);
                            go.transform.SetParent(sampleObject.transform.parent, false);
                            go.transform.position = pos;
                            go.transform.rotation = sampleObject.transform.rotation;
                            go.name = sampleObject.name + "_" + x + "_" + y + "_" + z;
                            objArray[objCount] = go;
                        }
                        objCount++;
                    }
                }
            }
            return objArray;
        }
    }
}
