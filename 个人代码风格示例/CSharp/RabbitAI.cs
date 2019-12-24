using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitAI : MonoBehaviour {

    [SerializeField] [Range(0, Mathf.Infinity)] float maxDetectDistanceMoving = 2f;//前方侦测距离（漫步中）
    [SerializeField] [Range(0,1)] float standStillRangeModifier = 0.5f;//静止侦测距离修正系数

    [SerializeField] [Range(0, Mathf.Infinity)] float minGiveUpDistance = 8f;//忽视目标距离

    [SerializeField] [Range(0, Mathf.Infinity)] float movementSpeedHunt = 0.5f;//全速行进速度
    [SerializeField] [Range(0, Mathf.Infinity)] float movementSpeedWander = 0.5f; //空闲漫步速度
    [SerializeField] [Range(0, Mathf.Infinity)] float wanderMinTime = 0.5f;//空闲漫步时长
    [SerializeField] [Range(0, Mathf.Infinity)] float standStillMinTime = 0.5f;//空闲站桩时长


    float backModifier = 0.5f;

    public AIState state = AIState.Idle;

    //只要求考虑一只兔子，所以这里没有设置list
    Transform targetTransform;

    float wanderCounter = 0f;
    float wanderDegree = 0f;
    //
    float standStillCounter = 0f;

    // Use this for initialization
    void Start()
    {
        FindTarget();
    }

    void FindTarget()
    {
        if (targetTransform == null)
        {
            WolfAI ai = FindObjectOfType<WolfAI>();
            if (ai != null)
            {
                targetTransform = ai.transform;
            }
        }
    }

    void Update()
    {
        //Debug.Log(Time.deltaTime);
        if (state == AIState.Idle)
        {
            //侦查
            if (TryDetectTarget())
            {
                Detected();
                return;
            }

            //漫步
            if (wanderCounter > 0)
            {
                Vector3 moveBase = new Vector3(Time.deltaTime * Mathf.Cos(wanderDegree), 0, Time.deltaTime * Mathf.Sin(wanderDegree)) * movementSpeedWander;
                transform.Translate(moveBase, Space.World);
                wanderCounter -= Time.deltaTime;
            }
            else
            {
                //原地发呆
                if (standStillCounter > 0)
                {
                    standStillCounter -= Time.deltaTime;
                    if (standStillCounter <= 0f)
                    {
                        wanderCounter = wanderMinTime;
                        wanderDegree = Random.Range(0f, Mathf.PI * 2);
                        transform.forward = new Vector3(Mathf.Cos(wanderDegree),0, Mathf.Sin(wanderDegree));
                    }
                }
                else
                {
                    standStillCounter = standStillMinTime;
                }
            }

            
        }
        else //if (state == AIState.Action)
        {
            if (TryDetectTarget())
            {
                Vector3 targetPos = targetTransform.position;
                Vector3 myPos = transform.position;
                Vector3 relativePos = targetPos - myPos;

                transform.Translate(-relativePos.normalized * movementSpeedHunt * Time.deltaTime, Space.World);
                if (relativePos.magnitude > 0)
                {
                    transform.forward = -relativePos.normalized;
                }
            }
            else
            {
                //因兔子超出距离，或被其他狼捕杀等，目标丢失
                state = AIState.Idle;
            }
        }

    }

    void Catch(Transform target)
    {
        Destroy(target.gameObject);
        state = AIState.Idle;
        Debug.Log("Catched");
    }

    //侦查
    bool TryDetectTarget()
    {
        if (state == AIState.Idle)
        {
            FindTarget();
            if (targetTransform != null)
            {
                Vector3 targetPos = targetTransform.position;
                Vector3 myPos = transform.position;
                Vector3 relativePos = targetPos - myPos;

                float directionModifier = GetDirectionModifierForPos(targetPos);
                if (maxDetectDistanceMoving * directionModifier > relativePos.magnitude)
                {
                    return true;
                }
            }
        }
        else//if (state == AIState.Action)
        {
            FindTarget();
            if (targetTransform != null)
            {
                Vector3 targetPos = targetTransform.position;
                Vector3 myPos = transform.position;
                Vector3 relativePos = targetPos - myPos;

                if (relativePos.magnitude > minGiveUpDistance)
                {
                    state = AIState.Idle;
                    Debug.Log("Rabbit give up for distance.");
                    return false;
                }
                else
                {
                    //Still in range
                    return true;
                }
            }
        }
        return false;
    }

    float GetDirectionModifierForPos(Vector3 targetPos)
    {
        Vector3 myPos = transform.position;
        Vector3 relativePos = targetPos - myPos;

        float directionModifier = (1 - backModifier) + backModifier * Vector3.Dot((relativePos).normalized, transform.forward);
        if (standStillCounter > 0f)
        {
            directionModifier *= standStillRangeModifier;
        }

        return directionModifier;
    }

    public void Detected()
    {
        state = AIState.Action;
    }

    static float drawAngle = 0f;
    private void OnDrawGizmos()
    {
        for (drawAngle = 0f; drawAngle <= 360f; drawAngle += 5f)
        {
            Vector3 drawDir = new Vector3(Mathf.Cos(drawAngle), 0f, Mathf.Sin(drawAngle));
            float directionModifier = (1 - backModifier) + backModifier * Vector3.Dot(drawDir, transform.forward);
            if (standStillCounter > 0f)
            {
                directionModifier *= standStillRangeModifier;
            }

            float length = directionModifier * maxDetectDistanceMoving;
            Gizmos.DrawLine(transform.position, transform.position + length * drawDir);
        }
    }
}
