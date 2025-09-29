// Assets/Scripts/Combat/Projectile.cs
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public long damage;
    public float speed = 16f;
    public Transform target;       // mục tiêu tại thời điểm bắn
    public float maxLife = 3f;     // tránh bay mãi

    float _life;

    void OnEnable()
    {
        _life = 0f;
    }

    void Update()
    {
        _life += Time.deltaTime;
        if (_life >= maxLife || target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 tpos = target.position;
        Vector3 dir = (tpos - transform.position);
        float dist = dir.magnitude;

        if (dist <= speed * Time.deltaTime) // chạm mục tiêu
        {
            HitTarget();
            return;
        }

        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        // optional: xoay đầu đạn hướng theo dir
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, ang);
    }

    void HitTarget()
    {
        var enemy = target ? target.GetComponent<Enemy>() : null;
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        gameObject.SetActive(false);
    }
}
