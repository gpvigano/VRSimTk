using UnityEngine;
using System;

namespace VRSimTk
{
    /// <summary>
    /// Simulation executor, controlling an object according to its history (see <see cref="SimController"/>).
    /// </summary>
    /// <remarks>Objects from this class are automatically created by SimController</remarks>
    public class SimExecutor : MonoBehaviour
    {
        public DateTime simulationDateTime;

        public float simulationTime = 0;

        public Transform targetTransform = null;

        public EntityHistory entityHistory = null;

        public bool isRunning = false;

        public bool UpdateTarget()
        {
            if (entityHistory.entityStates.Length == 0 || targetTransform==null)
            {
                return false;
            }
            EntityState prev_status;
            EntityState next_status;

            EntityState status = entityHistory.FindKeyFrame(simulationDateTime, out prev_status, out next_status);

            EntityState curr_status = null;
            if (status != null) curr_status = status;
            else if (prev_status != null) curr_status = prev_status;
            else if (next_status != null) curr_status = next_status;
#if UNITY_EDITOR
            foreach (var item in entityHistory.entityStates)
            {
                item.state = "(inactive)";
            }
            if (prev_status != null)
            {
                prev_status.state = " <-- Previous";
            }
            if (next_status != null)
            {
                next_status.state = " --> Next";
            }
            if (status != null)
            {
                status.state = ">Current<";
            }
#endif

            if (curr_status == null)
            {
                return false;
            }

            // set parent
            if (curr_status.parentTransform != null)
            {
                targetTransform.parent = curr_status.parentTransform;
            }
            bool anim = status == null &&
                ( prev_status!=null && next_status != null);
            if (!anim)
            {
                targetTransform.localPosition = curr_status.position;
                targetTransform.localRotation = curr_status.rotation;
            }

            // set representation
            //SimKeyFrame curr_repr_status = status;
            //if (curr_repr_status!=null) curr_repr_status = prev_status;

            //if (curr_repr_status && curr_repr_status->Representation)
            //{
            //    //LoadRepresentation(curr_status->Representation);
            //    SelectRepresentation(curr_repr_status->Representation->Name);
            //}
            //else
            //{
            //    // if no status is defined before this one reset representation
            //    RestoreRepresentation();
            //}

            if (!anim)
            {
                //ResetAnimation();
                return true;
            }

            Vector3 start_pos = prev_status.position;
            Quaternion start_rot = prev_status.rotation;
            Vector3 end_pos = next_status.position;
            Quaternion end_rot = next_status.rotation;

            if (prev_status.parentTransform != next_status.parentTransform)
            {
                Matrix4x4 m1 = Matrix4x4.identity;
                if (prev_status.parentTransform != null)
                {
                    m1 = prev_status.parentTransform.worldToLocalMatrix;
                }
                Matrix4x4 m2 = Matrix4x4.identity;
                if (next_status.parentTransform != null)
                {
                    EntityState next_status2 = null;
                    EntityHistory next_status_parent_hist = next_status.parentTransform.GetComponent<EntityHistory>();
                    if (next_status_parent_hist)
                    {
                        EntityState tmp1;
                        EntityState tmp2;
                        next_status2 = next_status_parent_hist.FindKeyFrame(next_status.startTime, out tmp1, out tmp2);
                    }
                    if (next_status2 != null)
                    {
                        Vector3 end_pos2 = next_status2.position;
                        Quaternion end_rot2 = next_status2.rotation;
                        if (next_status.parentTransform.parent != null)
                        {
                            m2 = next_status.parentTransform.parent.localToWorldMatrix;
                        }
                        Matrix4x4 mTmp = Matrix4x4.TRS(end_pos2, end_rot2, next_status.parentTransform.parent.lossyScale);
                        m2 *= mTmp;
                    }
                    else
                    {
                        m2 = next_status.parentTransform.localToWorldMatrix;
                    }
                }
                Matrix4x4 m = m1 * m2;
                // pos2 to the same local space as pos1
                end_pos = m.MultiplyPoint(next_status.position);
                // rot2 to the same local space as rot1
                end_rot = MatUtil.MatrixToQuaternion(m) * next_status.rotation;
            }
            float t = 0;
            if (prev_status.endTime != next_status.startTime)
            {
                t = (float)((simulationDateTime - prev_status.endTime).TotalSeconds
                    / (next_status.startTime - prev_status.endTime).TotalSeconds);
            }
            targetTransform.parent = prev_status.parentTransform;
            targetTransform.localPosition = Vector3.Lerp(start_pos, end_pos, t);
            targetTransform.localRotation = Quaternion.Slerp(start_rot, end_rot, t);

            return true;
        }

        void Start()
        {/*
            anim = gameObject.GetComponent<Animation>();

            if (anim == null)
            {
                anim = gameObject.AddComponent<Animation>();
            }
            AnimationCurve curve_pos_x = new AnimationCurve();
            AnimationCurve curve_pos_y = new AnimationCurve();
            AnimationCurve curve_pos_z = new AnimationCurve();
            foreach (var simKf in simKeyFrames)
            {
                float t = simKf.Key * timeScaling;
                curve_pos_x.AddKey(new Keyframe(t, simKf.Value.position.x, 0, 0));
                curve_pos_y.AddKey(new Keyframe(t, simKf.Value.position.y, 0, 0));
                curve_pos_z.AddKey(new Keyframe(t, simKf.Value.position.z, 0, 0));
            }
            AnimationClip clip = new AnimationClip();
            clip.name = "position";
            clip.legacy = true;
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve_pos_x);
            clip.SetCurve("", typeof(Transform), "localPosition.y", curve_pos_y);
            clip.SetCurve("", typeof(Transform), "localPosition.z", curve_pos_z);
            anim.wrapMode = WrapMode.Loop;
            //anim.clip = clip;
            //anim.Play(PlayMode.StopAll);
            anim.AddClip(clip, clip.name);
            anim.Play(clip.name, PlayMode.StopAll);
            */
            //startTime = Time.time;
        }

        void Update()
        {
            if (isRunning)
            {
                UpdateTarget();
            }
        }
    }
}
