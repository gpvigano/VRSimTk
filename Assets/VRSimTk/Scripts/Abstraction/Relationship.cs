using UnityEngine;
using System.Collections.Generic;

namespace VRSimTk
{
    public abstract class Relationship : MonoBehaviour
    {
        public string id;
        public string typeName = null;
        public EntityData ownerEntity = null;

        public virtual void OnValidate()
        {
            ////Debug.Log(GetType().Name + " Validate " + name);
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<EntityData>();
            }
            if (string.IsNullOrEmpty(id))
            {
                id = DataUtil.CreateNewId(this, typeName);
            }
            LinkEntities();
        }

        public virtual void OnDestroy()
        {
            UnlinkEntities();
        }

        public abstract void LinkEntities();

        public abstract void UnlinkEntities();

        public abstract bool EntityLinked(EntityData entity);

        public virtual void CleanUp()
        {
        }

        public abstract void RemoveSubjectEntity(EntityData subjEnt);

        public abstract void RemoveObjectEntity(EntityData subjEnt);

        protected void CleanUpEntityList(List<EntityData> entityList)
        {
            if (entityList != null)
            {
                entityList.RemoveAll(ent => ent == null);
            }
        }

        protected void LinkSubjectEntity(EntityData subjEnt)
        {
            if (subjEnt.relationshipsOut == null)
            {
                subjEnt.relationshipsOut = new List<Relationship>();
            }
            if (!subjEnt.relationshipsOut.Contains(this))
            {
                subjEnt.relationshipsOut.Add(this);
            }
        }

        protected void LinkSubjectEntities(List<EntityData> entityList)
        {
            if (entityList != null)
            {
                foreach (var subjEnt in entityList)
                {
                    LinkSubjectEntity(subjEnt);
                }
            }
        }

        protected void LinkObjectEntity(EntityData objEnt)
        {
            if (objEnt.relationshipsIn == null)
            {
                objEnt.relationshipsIn = new List<Relationship>();
            }
            if (!objEnt.relationshipsIn.Contains(this))
            {
                objEnt.relationshipsIn.Add(this);
            }
        }

        protected void LinkObjectEntities(List<EntityData> entityList)
        {
            if (entityList != null)
            {
                foreach (var objEnt in entityList)
                {
                    LinkObjectEntity(objEnt);
                }
            }
        }

        protected void UnlinkSubjectEntity(EntityData ent)
        {
            if (ent.relationshipsOut != null)
            {
                ent.relationshipsOut.RemoveAll(rel => rel == this);
            }
        }

        protected void UnlinkObjectEntity(EntityData ent)
        {
            if (ent.relationshipsIn != null)
            {
                ent.relationshipsIn.RemoveAll(rel => rel == this);
            }
        }

        protected void UnlinkSubjectEntities(List<EntityData> entityList)
        {
            if (entityList != null)
            {
                foreach (var ent in entityList)
                {
                    UnlinkSubjectEntity(ent);
                }
            }
        }

        protected void UnlinkObjectEntities(List<EntityData> entityList)
        {
            if (entityList != null)
            {
                foreach (var ent in entityList)
                {
                    UnlinkObjectEntity(ent);
                }
            }
        }

        protected void UnlinkRemovedSubjectEntity(ref EntityData prevEnt, EntityData currEnt)
        {
            if (prevEnt != null)
            {
                if (prevEnt != currEnt && prevEnt.relationshipsIn != null)
                {
                    prevEnt.relationshipsOut.RemoveAll(rel => rel == this);
                }
            }
            prevEnt = currEnt;
        }

        protected void UnlinkRemovedObjectEntity(ref EntityData prevEnt, EntityData currEnt)
        {
            if (prevEnt != null)
            {
                if (prevEnt != currEnt && prevEnt.relationshipsIn != null)
                {
                    prevEnt.relationshipsIn.RemoveAll(rel => rel == this);
                }
            }
            prevEnt = currEnt;
        }

        protected void UnlinkRemovedObjectEntities(ref List<EntityData> prevList, List<EntityData> currList)
        {
            if (prevList != null)
            {
                foreach (var objEnt in prevList)
                {
                    if (!currList.Contains(objEnt) && objEnt.relationshipsIn != null)
                    {
                        objEnt.relationshipsIn.RemoveAll(rel => rel == this);
                    }
                }
                prevList.Clear();
                prevList.AddRange(currList);
            }
            else
            {
                if (currList != null)
                {
                    prevList = new List<EntityData>(currList);
                }
            }
        }

        protected void UnlinkRemovedSubjectEntities(ref List<EntityData> prevList, List<EntityData> currList)
        {
            if (prevList != null)
            {
                foreach (var objEnt in prevList)
                {
                    if (!currList.Contains(objEnt) && objEnt.relationshipsIn != null)
                    {
                        objEnt.relationshipsIn.RemoveAll(rel => rel == this);
                    }
                }
                prevList.Clear();
                prevList.AddRange(currList);
            }
            else
            {
                if (currList != null)
                {
                    prevList = new List<EntityData>(currList);
                }
            }
        }
    }
}
