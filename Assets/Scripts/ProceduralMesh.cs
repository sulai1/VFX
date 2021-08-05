using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    [RequireComponent(typeof(MeshFilter))]
    public class ProceduralMesh : MonoBehaviour
    {
        private MeshFilter meshFilter;

        public Mesh Mesh
        {
            get { return meshFilter?.sharedMesh; }
            set { meshFilter.sharedMesh = value; }
        }


        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

    }
}
