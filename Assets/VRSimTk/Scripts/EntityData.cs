using UnityEngine;
using System.Collections.Generic;

namespace VRSimTk
{
    /// <summary>
    /// Additional information stored along with each object
    /// </summary>
    /// <remarks>Any information already available in other components of a game object should be omitted here.</remarks>
    public class EntityData : MonoBehaviour
    {
        [Tooltip("Unique identifier")]
        public string id = string.Empty;
        [Tooltip("Type of entity")]
        public string type = string.Empty;
        [Tooltip("Brief description")]
        [Multiline]
        public string description = string.Empty;
        [Tooltip("Representation")]
        public EntityRepresentation activeRepresentation;

        [Header("Asset info")]
        [Tooltip("Representation asset bundle")]
        public string assetBundleName = string.Empty;
        [Tooltip("Representation asset")]
        public string assetName = string.Empty;

        [Header("Relationships")]
        public List<Relationship> relationshipsIn;
        public List<Relationship> relationshipsOut;

        public void OnValidate()
        {
            List<EntityData> entities = new List<EntityData>(FindObjectsOfType<EntityData>());
            EntityData found = entities.Find(ent => ent != this && ent.id == id);
            if (found != null)
            {
                string newId = DataUtil.CreateNewId(this, id);
                Debug.LogFormat("{0} with id = {1} already exists, changed to {2}", GetType().Name, id, newId);
                id = newId;
            }
            // clean up relationships references
            if (relationshipsIn != null)
            {
                relationshipsIn.RemoveAll(rel => rel == null || !rel.EntityLinked(this));
            }
            if (relationshipsOut != null)
            {
                relationshipsOut.RemoveAll(rel => rel == null || !rel.EntityLinked(this));
            }
        }

        public virtual void OnDestroy()
        {
            if (relationshipsIn != null)
            {
                foreach (var rel in relationshipsIn)
                {
                    rel.CleanUp();
                }
            }
            if (relationshipsOut != null)
            {
                foreach (var rel in relationshipsOut)
                {
                    rel.CleanUp();
                    ////rel.RemoveObjectEntity(this);
                }
            }
        }

        public void UnlinkRelationships()
        {
            Debug.Log("UnlinkRelationships");
            if (relationshipsIn != null)
            {
                foreach (var rel in relationshipsIn)
                {
                    rel.RemoveObjectEntity(this);
                    if (rel.ownerEntity == this)
                    {
                        rel.ownerEntity = null;
                    }
                }
            }
            if (relationshipsOut != null)
            {
                foreach (var rel in relationshipsOut)
                {
                    rel.RemoveSubjectEntity(this);
                    if (rel.ownerEntity == this)
                    {
                        rel.ownerEntity = null;
                    }
                }
            }
        }
    }
}
