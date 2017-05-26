using UnityEngine;
using System.Collections.Generic;

namespace VRSimTk
{
    public class OneToOneRelationship : Relationship
    {
        public EntityData subjectEntity;
        public EntityData objectEntity;

        private EntityData prevObjectEntity;
        private EntityData prevSubjectEntity;

        public override void OnValidate()
        {
            if (subjectEntity == null)
            {
                subjectEntity = GetComponent<EntityData>();
            }
            base.OnValidate();
        }

        public override bool EntityLinked(EntityData entity)
        {
            return entity != null && (subjectEntity == entity || objectEntity == entity);
        }

        public override void LinkEntities()
        {
            if (subjectEntity != null)
            {
                    LinkSubjectEntity(subjectEntity);
            }
            if (objectEntity != null)
            {
                    LinkObjectEntity(objectEntity);
            }
        }

        public override void UnlinkEntities()
        {
            UnlinkSubjectEntity(subjectEntity);
            UnlinkObjectEntity(objectEntity);
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
            if(objectEntity==objEnt)
            {
                objectEntity = null;
            }
        }

        protected void UnlinkRemovedEntities()
        {
            UnlinkRemovedSubjectEntity(ref prevSubjectEntity, subjectEntity);
            UnlinkRemovedObjectEntity(ref prevObjectEntity, objectEntity);
        }
    }
}
