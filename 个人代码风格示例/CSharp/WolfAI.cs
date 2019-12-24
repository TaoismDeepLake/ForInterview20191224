using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    Action
}

//为阅卷方便，未抽象基类
public class WolfAI : MonoBehaviour
{
    [SerializeField] [Range(0, Mathf.Infinity)] float maxDetectDistanceMoving = 2f;
    [SerializeField] [Range(1, Mathf.Infinity)] float standStillRangeModifier = 2f;

    [SerializeField] [Range(0, Mathf.Infinity)] float minGiveUpDistance = 7f;
    [SerializeField] [Range(0, Mathf.Infinity)] float maxKillDistance = 0.1f;

    [SerializeField] [Range(0, Mathf.Infinity)] float movementSpeedHunt = 0.5f;
    [SerializeField] [Range(0, Mathf.Infinity)] float movementSpeedWander = 0.5f;
    [SerializeField] [Range(0, Mathf.Infinity)] float wanderMinTime = 0.5f;
    [SerializeField] [Range(0, Mathf.Infinity)] float standStillMinTime = 0.5f;

    [SerializeField] [Range(0, 1f)] float howlChance = 1f;
    [SerializeField] [Range(0, Mathf.Infinity)] float howlLength = 1f;

    float backModifier = 0.5f;

    public AIState state = AIState.Idle;

    //只要求考虑一只兔子，所以这里没有设置list
    Transform rabbitTransform;

    //剩余嚎叫时间
    float howlCounter = 0f;
    //
    float wanderCounter = 0f;
    float wanderDegree = 0f;
    //
    float standStillCounter = 0f;

    // Use this for initialization
    void Start()
    {
        FindRabbit();
    }

    void FindRabbit()
    {
        if (rabbitTransform == null)
        {
            RabbitAI ai = FindObjectOfType<RabbitAI>();
            if (ai != null)
            {
                rabbitTransform = ai.transform;
            }
        }
    }

    void Update()
    {
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
                if (howlCounter > 0)
                {
                    //嚎叫中
                    howlCounter -= Time.deltaTime;
                }
                else
                {
                    transform.LookAt(rabbitTransform);

                    Vector3 targetPos = rabbitTransform.position;
                    Vector3 myPos = transform.position;
                    Vector3 relativePos = targetPos - myPos;

                    if (relativePos.magnitude <= maxKillDistance)
                    {
                        Catch(rabbitTransform);
                        return;
                    }
                    transform.Translate(relativePos.normalized * Time.deltaTime * movementSpeedHunt, Space.World);
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
            FindRabbit();
            if (rabbitTransform != null)
            {
                Vector3 targetPos = rabbitTransform.position;
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
            FindRabbit();
            if (rabbitTransform != null)
            {
                Vector3 targetPos = rabbitTransform.position;
                Vector3 myPos = transform.position;
                Vector3 relativePos = targetPos - myPos;

                if (relativePos.magnitude > minGiveUpDistance)
                {
                    state = AIState.Idle;
                    Debug.Log("Wolf give up for distance.");
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

    void Detected()
    {
        state = AIState.Action;
        if (rabbitTransform != null)
        {
            transform.LookAt(rabbitTransform);
        }
        TryHowl();
    }

    //嚎叫
    void TryHowl()
    {
        if (Random.Range(0f, 1f) < howlChance)
        {
            Howl();
            //TODO: rabbit force detect;
            if (rabbitTransform != null)
            {
                RabbitAI ai = rabbitTransform.GetComponent<RabbitAI>();
                if (ai != null)
                {
                    ai.Detected();
                }
            }
        }
    }

    void Howl()
    {
        howlCounter = howlLength;
        Debug.Log("Wolf start howling.");
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
