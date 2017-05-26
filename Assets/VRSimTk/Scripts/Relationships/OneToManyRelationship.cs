using UnityEngine;
using System.Collections.Generic;

namespace VRSimTk
{
    public class OneToManyRelationship : Relationship
    {
        public EntityData subjectEntity = null;
        public List<EntityData> objectEntities = null;

        private EntityData prevSubjectEntity = null;
        private List<EntityData> prevObjectEntities = null;

        public override void OnValidate()
        {
            if (subjectEntity == null)
            {
                subjectEntity = GetComponent<EntityData>();
            }
            CleanUp();
            UnlinkRemovedEntities();
            base.OnValidate();
        }

        public override void CleanUp()
        {
            CleanUpEntityList(objectEntities);
        }

        public override bool EntityLinked(EntityData entity)
        {
            return entity != null && (subjectEntity == entity
                || (objectEntities != null && objectEntities.Contains(entity)));
        }

        public override void LinkEntities()
        {
            if (subjectEntity != null)
            {
                    LinkSubjectEntity(subjectEntity);
            }
            LinkObjectEntities(objectEntities);
        }

        public override void RemoveSubjectEntity(EntityData subjEnt)
        {
            if(subjectEntity==subjEnt)
            {
                subjectEntity = null;
            }
        }

        public override void RemoveObjectEntity(EntityData objEnt)
        {
            if(objectEntities!=null)
            {
                objectEntities.RemoveAll(e => e==objEnt);
            }
        }

        public override void UnlinkEntities()
        {
            UnlinkSubjectEntity(subjectEntity);
            UnlinkObjectEntities(objectEntities);
        }

        protected void UnlinkRemovedEntities()
        {
            UnlinkRemovedSubjectEntity(ref prevSubjectEntity, subjectEntity);
            UnlinkRemovedObjectEntities(ref prevObjectEntities, objectEntities);
        }
    }
}
