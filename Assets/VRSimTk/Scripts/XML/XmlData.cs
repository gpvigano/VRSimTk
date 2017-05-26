using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace VRSimTk
{

    [System.Serializable]
    [XmlType(TypeName = "relationship")]
    [XmlInclude(typeof(VrXmlOneToOneRelationship))]
    [XmlInclude(typeof(VrXmlOneToManyRelationship))]
    [XmlInclude(typeof(VrXmlManyToManyRelationship))]
    [XmlInclude(typeof(VrXmlInclusionRelationship))]
    [XmlInclude(typeof(VrXmlCompositionRelationship))]
    public class VrXmlRelationship
    {
        public string id = string.Empty;
        public string typeName = null;
        public string ownerEntityId = string.Empty;
    }

    [System.Serializable]
    [XmlType(TypeName = "one-to-one-relationship")]
    public class VrXmlOneToOneRelationship : VrXmlRelationship
    {
        public string subjectEntityId = string.Empty;
        public string objectEntityId = string.Empty;
    }

    [System.Serializable]
    [XmlType(TypeName = "one-to-many-relationship")]
    public class VrXmlOneToManyRelationship : VrXmlRelationship
    {
        public string subjectEntityId = string.Empty;
        [XmlArray("objectEntities")]
        [XmlArrayItem(typeof(string), ElementName = "entityId")]
        public List<string> objectEntitiesId = new List<string>();
    }

    [System.Serializable]
    [XmlType(TypeName = "many-to-many-relationship")]
    public class VrXmlManyToManyRelationship : VrXmlRelationship
    {
        [XmlArray("subjectEntities")]
        [XmlArrayItem(typeof(string), ElementName = "entityId")]
        public List<string> subjectEntitiesId = new List<string>();
        [XmlArray("objectEntities")]
        [XmlArrayItem(typeof(string), ElementName = "entityId")]
        public List<string> objectEntitiesId = new List<string>();
    }

    [System.Serializable]
    [XmlType(TypeName = "composition-relationship")]
    public class VrXmlCompositionRelationship : VrXmlOneToManyRelationship
    {
    }

    [System.Serializable]
    [XmlType(TypeName = "inclusion-relationship")]
    public class VrXmlInclusionRelationship : VrXmlOneToManyRelationship
    {
    }

    /// <summary>
    /// Auxiliary vector for Euler angles serialization
    /// </summary>
    /// <remarks>Values of the Euler angles are not kept as they are
    /// by Unity, in particular when calling Quaternin methods, so this
    /// class is here to round values and discard precision errors,
    /// otherwise these errors accumulate over each read and write
    /// iteration.
    /// In Unity Editor values will often be affected by these errors.
    /// </remarks>
    /// <see cref="http://answers.unity3d.com/questions/1156322/unity-editor-rotation-accuracy-89999-etc.html"/>
    [System.Serializable]
    public class VrXmlVector3
    {
        public string x = "0";
        public string y = "0";
        public string z = "0";
        ////public static VrXmlVector3 zero = new VrXmlVector3(Vector3.zero,0);
        ////public static VrXmlVector3 right = new VrXmlVector3(Vector3.right,0);
        ////public static VrXmlVector3 up = new VrXmlVector3(Vector3.up,0);
        ////public static VrXmlVector3 forward = new VrXmlVector3(Vector3.forward,0);

        public VrXmlVector3()
        {
        }

        public VrXmlVector3(Vector3 vec, int precision = -1)
        {
            Set(vec, precision);
        }

        public void Set(Vector3 vec, int precision)
        {
            if (vec == Vector3.zero)
            {
                precision = 0;
            }
            string format = "{0:F" + (precision > 0 ? precision.ToString() : string.Empty) + "}";
            x = string.Format(format, vec.x);
            y = string.Format(format, vec.y);
            z = string.Format(format, vec.z);
        }

        public Vector3 Get()
        {
            Vector3 vec = Vector3.zero;
            vec.x = float.Parse(x);
            vec.y = float.Parse(y);
            vec.z = float.Parse(z);
            return vec;
        }

        ////static public implicit operator string (XmlVector3 xmlVec)
        ////{
        ////    return xmlVec.value;
        ////}
        static public explicit operator VrXmlVector3(Vector3 vec)
        {
            return new VrXmlVector3(vec);
        }
        static public explicit operator VrXmlVector3(Vector4 vec)
        {
            return new VrXmlVector3(vec);
        }
        public static explicit operator Vector3(VrXmlVector3 xmlVec)
        {
            return xmlVec.Get();
        }
    }

    /// <summary>
    /// Auxiliary rotation matrix for rotation serialization
    /// </summary>
    /// <remarks>Values of the rotation matrix are not kept as they are
    /// by Unity, in particular when calling Quaternin methods, so this
    /// class is here to round values and discard precision errors,
    /// otherwise these errors accumulate over each read and write
    /// iteration.
    /// In Unity Editor values will often be affected by these errors.
    /// </remarks>
    /// <see cref="http://answers.unity3d.com/questions/1156322/unity-editor-rotation-accuracy-89999-etc.html"/>
    [System.Serializable]
    [XmlType(TypeName = "rotation")]
    public class VrXmlRotationMatrix
    {
        public string row1 = "1 0 0";
        public string row2 = "0 1 0";
        public string row3 = "0 0 1";
        ////public static VrXmlRotationMatrix identity = new VrXmlRotationMatrix(Matrix4x4.identity);

        public VrXmlRotationMatrix()
        {
        }

        public VrXmlRotationMatrix(Matrix4x4 mat)
        {
            FromMatrix4x4(mat);
        }

        public void FromMatrix4x4(Matrix4x4 mat)
        {
            Vector3ToRow(mat.GetRow(0), ref row1);
            Vector3ToRow(mat.GetRow(1), ref row2);
            Vector3ToRow(mat.GetRow(2), ref row3);
        }

        public Matrix4x4 ToMatrix4x4()
        {
            Matrix4x4 mat = new Matrix4x4();
            mat.SetRow(0, RowToVector3(row1));
            mat.SetRow(1, RowToVector3(row2));
            mat.SetRow(2, RowToVector3(row3));
            return mat;
        }

        static public implicit operator VrXmlRotationMatrix(Matrix4x4 mat)
        {
            return new VrXmlRotationMatrix(mat);
        }

        static public explicit operator Matrix4x4(VrXmlRotationMatrix rotMat)
        {
            return rotMat.ToMatrix4x4();
        }

        public static void Vector3ToRow(Vector3 vec, ref string row)
        {
            // cut off calculation errors keepeng only the first decimal digits
            row = string.Format("{0:F6} {1:F6} {2:F6}", vec.x, vec.y, vec.z);
        }

        public static Vector3 RowToVector3(string row)
        {
            Vector3 vec = Vector3.zero;
            char[] sep = { ' ' };
            string[] values = row.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            vec.x = float.Parse(values[0]);
            vec.y = float.Parse(values[1]);
            vec.z = float.Parse(values[2]);
            return vec;
        }
    }

    [System.Serializable]
    [XmlType(TypeName = "localTransform")]
    public class VrXmlLocalTransform
    {
        [Tooltip("Identifier of the entity to which this transform is relative")]
        public string relToId = string.Empty;

        [Tooltip("Position\n(relative to parent transform)")]
        public VrXmlVector3 position = new VrXmlVector3(Vector3.zero);

        [Tooltip("Use rotation matrix instead of Euler angles")]
        public bool useRotationMatrix = false;

        [Tooltip("Translation and rotation matrix\n(Y-up, right-handed)")]
        public VrXmlRotationMatrix rotationMatrix = new VrXmlRotationMatrix(Matrix4x4.identity);

        [Tooltip("Euler angles\n(relative to parent transform)")]
        public VrXmlVector3 eulerAngles = new VrXmlVector3(Vector3.zero);

        [Tooltip("Scaling\n(1 = no rescaling)")]
        public VrXmlVector3 scale = new VrXmlVector3(Vector3.one);

        public void FromTransform(Transform unityTransform, bool zUp)
        {
            position.Set( CsConv.VecToVecRL(unityTransform.localPosition, zUp),3);
            ////if(unityTransform.useRotationMatrix)
            ////{
            ////    rotationMatrix.FromMatrix4x4(CsConv.QuatToRotMat(unityTransform.localRotation));
            ////}
            ////else
            ////{
            ////    eulerAngles = unityTransform.localEulerAngles;
            ////}
            useRotationMatrix = false;
            rotationMatrix.FromMatrix4x4(MatUtil.QuaternionToMatrix(unityTransform.localRotation));
            eulerAngles.Set(unityTransform.localEulerAngles, 2);
            scale.Set( unityTransform.localScale,3);
            if (unityTransform.parent)
            {
                EntityData parentEntity = unityTransform.parent.GetComponent<EntityData>();
                if (parentEntity)
                {
                    relToId = unityTransform.parent.GetComponent<EntityData>().id;
                }
                else
                {
                    Debug.LogErrorFormat("Transformation {0} is relative to non-entity object {1} (hierarchy will be lost)",
                        unityTransform.name, unityTransform.parent.name);
                }
            }
            else
            {
                relToId = string.Empty;
            }
        }
    }

    /// <summary>
    /// Representation of an entity
    /// 
    /// </summary>
    [System.Serializable]
    [XmlType(TypeName = "representation")]
    public class VrXmlRepresentation
    {
        public enum AssetType
        {
            None,
            Prefab,
            AssetBundle,
            Sphere,
            Cube,
            Cylinder,
            Capsule,
            Quad,
            Plane,
            Model
        }

        [Tooltip("Identifier of the representation")]
        public string id = string.Empty;
        [Tooltip("Representation type")]
        public AssetType assetType = AssetType.None;
        [Tooltip("Representation asset bundle")]
        public string assetBundleName = null;
        [Tooltip("Representation asset")]
        public string assetName = null;
        [Tooltip("Representation is visible")]
        public bool isVisible = true;

        [Header("Transform")]
        public VrXmlLocalTransform localTransform = new VrXmlLocalTransform();

        [Tooltip("Model import options")]
        public AsImpL.ImportOptions importOptions = null;
    }

    [System.Serializable]
    [XmlType(TypeName = "entity")]
    public class VrXmlEntityData
    {
        public string name = string.Empty;

        [Tooltip("Identifier of the entity")]
        public string id = string.Empty;

        public string type = string.Empty;

        public string description = string.Empty;

        [Header("Transform")]
        public VrXmlLocalTransform localTransform = new VrXmlLocalTransform();

        [Header("Representation")]
        public VrXmlRepresentation representation = null;

        // Default constructor needed by XmlSerializer
        public VrXmlEntityData()
        {
        }
    }

    [System.Serializable]
    [XmlType(TypeName = "history")]
    public class VrXmlHistoryData
    {
        [Tooltip("Identifier of the related entity")]
        public string entityId = string.Empty;
        public bool upAxisIsZ = false;
        public string fileName = string.Empty;
    }

    [System.Serializable]
    [XmlType(TypeName = "simulation")]
    public class VrXmlSimulationData
    {
        public string historyUri;
        public string historyName;
        public string description;
        public string details;
        public bool autoStart = false;
        [XmlArray("histories")]
        public List<VrXmlHistoryData> historyList = new List<VrXmlHistoryData>();
    }

    [System.Serializable]
    [XmlType(TypeName = "scenario")]
    public class VrXmlSceneData
    {
        [XmlArray]
        [XmlArrayItem(typeof(string), ElementName = "variant")]
        public List<string> activeVariants = new List<string>();
        public bool upAxisIsZ = false;
        [XmlArray("entities")]
        public List<VrXmlEntityData> entityList = new List<VrXmlEntityData>();
        public VrXmlSimulationData simulation = null;
        [XmlArray("relationships")]
        public List<VrXmlRelationship> relationshipList = new List<VrXmlRelationship>();
        // Default constructor needed by XmlSerializer
        public VrXmlSceneData() : base()
        {
            // list of variants (aka representation contexts)
            // if the first one is not available try the second one and so on
            ////activeVariants.Add( "context2" );
            ////activeVariants.Add( "context1");
        }
    }
}
