using UnityEngine;
using System.Collections.Generic;

namespace VRSimTk
{
    public class ManyToManyRelationship : Relationship
    {
        public List<EntityData> subjectEntities = null;
        public List<EntityData> objectEntities = null;

        private List<EntityData> prevSubjectEntities = null;
        private List<EntityData> prevObjectEntities = null;

        public override void OnValidate()
        {
            CleanUp();
            UnlinkRemovedEntities();
            base.OnValidate();
        }

        public override void CleanUp()
        {
            CleanUpEntityList(subjectEntities);
            CleanUpEntityList(objectEntities);
        }

        public override bool EntityLinked(EntityData entity)
        {
            return entity != null && (
               (subjectEntities != null && subjectEntities.Contains(entity))
                || (objectEntities != null && objectEntities.Contains(entity)));
        }

        public override void LinkEntities()
        {
            LinkSubjectEntities(subjectEntities);
            LinkObjectEntities(objectEntities);
        }

        public override void UnlinkEntities()
        {
            UnlinkSubjectEntities(subjectEntities);
            UnlinkObjectEntities(objectEntities);
        }

        public override void RemoveSubjectEntity(EntityData subjEnt)
        {
            if(subjectEntities != null)
            {
                subjectEntities.RemoveAll(e => e==subjEnt);
            }
        }

        public override void RemoveObjectEntity(EntityData objEnt)
        {
            if(objectEntities!=null)
            {
                objectEntities.RemoveAll(e => e==objEnt);
            }
        }

        protected void UnlinkRemovedEntities()
        {
            UnlinkRemovedObjectEntities(ref prevObjectEntities, objectEntities);
            UnlinkRemovedSubjectEntities(ref prevSubjectEntities, subjectEntities);
        }
    }
}
