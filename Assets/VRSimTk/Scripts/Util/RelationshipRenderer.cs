using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace VRSimTk
{
    public class RelationshipConnection
    {
        public LineRenderer lineRenderer = null;
        public Transform startTransform = null;
        public Transform endTransform = null;
    }

    public class RelationshipRenderer : MonoBehaviour
    {
        public Relationship relationshipComponent = null;

        private OneToOneRelationship relationshipOneToOne = null;
        private OneToManyRelationship relationshipOneToMany = null;
        private ManyToManyRelationship relationshipManyToMany = null;

        private List<RelationshipConnection> connectionList = new List<RelationshipConnection>();
        private GameObject linesObject = null;

        private void Awake()
        {
            if (relationshipComponent == null)
            {
                relationshipComponent = GetComponent<Relationship>();
            }
            if (relationshipComponent == null)
            {
                Debug.LogError(GetType() + " component needs a relationship component defined");
            }
        }

        private void Start()
        {
            ParseRelationship();
        }

        private void ParseRelationship()
        {
            connectionList.Clear();
            if (linesObject)
            {
                Destroy(linesObject);
            }
            linesObject = new GameObject(relationshipComponent.id + "_Lines");
            linesObject.transform.parent = transform;
            relationshipOneToOne = relationshipComponent as OneToOneRelationship;
            relationshipOneToMany = relationshipComponent as OneToManyRelationship;
            relationshipManyToMany = relationshipComponent as ManyToManyRelationship;

            if (relationshipOneToOne)
            {
                RelationshipConnection conn = new RelationshipConnection();
                var lr = linesObject.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.startColor = Color.magenta;
                lr.endColor = Color.magenta;
                lr.material = Resources.Load("Materials/Line") as Material;
                //lr.material.color = Color.magenta;
                conn.lineRenderer = lr;
                conn.startTransform = relationshipOneToOne.subjectEntity.transform;
                conn.endTransform = relationshipOneToOne.objectEntity.transform;
                connectionList.Add(conn);
            }

            if (relationshipOneToMany)
            {
                foreach (var entity in relationshipOneToMany.objectEntities)
                {
                    GameObject line = new GameObject("LineTo" + entity.name);
                    line.transform.parent = linesObject.transform;
                    RelationshipConnection conn = new RelationshipConnection();
                    LineRenderer lr = line.AddComponent<LineRenderer>();
                    lr.material = Resources.Load("Materials/Line") as Material;
                    //lr.material.color = Color.magenta;
                    lr.startWidth = 0.1f;
                    lr.endWidth = 0.1f;
                    lr.startColor = Color.yellow;
                    lr.endColor = Color.red;

                    conn.lineRenderer = lr;
                    conn.startTransform = relationshipOneToMany.subjectEntity.transform;
                    conn.endTransform = entity.transform;
                    connectionList.Add(conn);
                }
            }

            if (relationshipManyToMany)
            {
                foreach (var entity in relationshipManyToMany.subjectEntities)
                {
                    GameObject line = new GameObject("LineFrom" + entity.name);
                    line.transform.parent = linesObject.transform;
                    RelationshipConnection conn = new RelationshipConnection();
                    LineRenderer lr = line.AddComponent<LineRenderer>();
                    lr.material = Resources.Load("Materials/Line") as Material;
                    lr.startWidth = 0.1f;
                    lr.endWidth = 0.1f;
                    lr.startColor = Color.blue;
                    lr.endColor = Color.cyan;

                    conn.lineRenderer = lr;
                    conn.startTransform = entity.transform;
                    conn.endTransform = relationshipManyToMany.ownerEntity.transform;
                    connectionList.Add(conn);
                }

                foreach (var entity in relationshipManyToMany.objectEntities)
                {
                    GameObject line = new GameObject("LineTo" + entity.name);
                    line.transform.parent = linesObject.transform;
                    RelationshipConnection conn = new RelationshipConnection();
                    LineRenderer lr = line.AddComponent<LineRenderer>();
                    lr.material = Resources.Load("Materials/Line") as Material;
                    lr.startWidth = 0.1f;
                    lr.endWidth = 0.1f;
                    lr.startColor = Color.yellow;
                    lr.endColor = Color.red;

                    conn.lineRenderer = lr;
                    conn.startTransform = relationshipManyToMany.ownerEntity.transform;
                    conn.endTransform = entity.transform;
                    connectionList.Add(conn);
                }
            }
        }

        private bool ReleationShipChanged()
        {
            if (relationshipOneToOne)
            {
                if (relationshipOneToOne.subjectEntity == null && connectionList.Count > 0)
                {
                    return true;
                }
                if (relationshipOneToOne.subjectEntity)
                {
                    if (connectionList.Count == 0)
                    {
                        return true;
                    }
                    if (relationshipOneToOne.subjectEntity.transform != connectionList[0].startTransform)
                    {
                        return true;
                    }
                }
                if (relationshipOneToOne.objectEntity == null && connectionList.Count > 0)
                {
                    return true;
                }
                if (relationshipOneToOne.objectEntity)
                {
                    if (connectionList.Count != 0)
                    {
                        return true;
                    }
                    if (relationshipOneToOne.objectEntity.transform != connectionList[0].endTransform)
                    {
                        return true;
                    }
                }
                return false;
            }

            if (relationshipOneToMany)
            {
                if (relationshipOneToMany.subjectEntity == null && connectionList.Count > 0)
                {
                    return true;
                }
                if (relationshipOneToMany.objectEntities.Count + 1 != connectionList.Count)
                {
                    return true;
                }
                if (relationshipOneToMany.subjectEntity)
                {
                    if (connectionList.Count != 0)
                    {
                        return true;
                    }
                    if (relationshipOneToMany.subjectEntity.transform != connectionList[0].startTransform)
                    {
                        return true;
                    }
                }
                for (int i = 0; i < relationshipOneToMany.objectEntities.Count; i++)
                {
                    var entity = relationshipOneToMany.objectEntities[i];
                    if (entity.transform != connectionList[i + 1].startTransform)
                    {
                        return true;
                    }
                }
            }

            if (relationshipManyToMany)
            {
                if (relationshipManyToMany.subjectEntities.Count + relationshipManyToMany.objectEntities.Count != connectionList.Count)
                {
                    return true;
                }
                for (int i = 0; i < relationshipManyToMany.subjectEntities.Count; i++)
                {
                    var entity = relationshipManyToMany.subjectEntities[i];
                    if (entity.transform != connectionList[i].startTransform)
                    {
                        return true;
                    }
                }
                int n = relationshipManyToMany.subjectEntities.Count;
                for (int i = 0; i < relationshipManyToMany.objectEntities.Count; i++)
                {
                    var entity = relationshipManyToMany.objectEntities[i];
                    if (entity.transform != connectionList[i + n].endTransform)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Update()
        {

            connectionList.ForEach(c =>
                {
                    if (c.startTransform == null || c.endTransform == null)
                    {
                        Destroy(c.lineRenderer);
                        c.lineRenderer = null;
                    }
                });
            //if (ReleationShipChanged())
            //{
            //    ParseRelationship();
            //}

            connectionList.RemoveAll(c => c.lineRenderer == null);
            foreach (var conn in connectionList)
            {
                conn.lineRenderer.SetPosition(0, conn.startTransform.position);
                conn.lineRenderer.SetPosition(1, conn.endTransform.position);
            }
        }
    }
}
