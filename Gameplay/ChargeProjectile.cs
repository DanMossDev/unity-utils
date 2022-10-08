using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
This component depends on the Unity InputSystem, there should be an event being broadcast when a button is pressed (OnFire) and when it is released (OnReleaseFire)

You can rename these functions however you wish to align with your current input system setup, it just needs to get the event - the input from said events is irrelevant

It also assumes that it is the child of a transform from which it should spawn projectiles, if this transform is the player and they have collision, be sure that the player's
projectiles do not collide with the player, or there will be an instant collision

Logic for the projectiles themselves should be added to their prefabs
*/

[RequireComponent(typeof(LineRenderer))]
public class ShootProjectile : MonoBehaviour
{
    [Header("Game Feel")]
    [Tooltip("Maximum speed of the project")]
    [SerializeField] float maxProjectileSpeed = 5f;
    [Tooltip("Rate at which the projectile charges (increase in projectile speed per second while charging)")]
    [SerializeField] float projectileChargeSpeed = 3f;
    [Tooltip("Cooldown between shots")]
    [SerializeField] float shotCD = 0.5f;
    [Tooltip("Rate at which the projectile rotates while flying")]
    [SerializeField] float turnRate = 150;
    [Space]
    [Header("Prefabs and Game Objects")]
    [Tooltip("A prefab of the projectile")]
    [SerializeField] GameObject projectile;
    [Tooltip("If the projectile's prefab needs to be rotated, do so here in degrees")]
    [SerializeField] float offset;
    [Tooltip("The player animator component")]
    [SerializeField] Animator animator;
    Transform projectileSpawner;
    [Space]
    [Header("Audio")] //Not implemented here but can easily be added at appropriate times
    [SerializeField] AudioClip[] chargeProjectile;
    [SerializeField] AudioClip[] shootProjectile;

    //Used for logic
    float projectileSpeed = 1f;
    bool onCD = false;
    bool isShooting = false;
    float angle;

    //Cached component references
    LineRenderer lineRenderer;


    void OnEnable() 
    {
        lineRenderer = GetComponent<LineRenderer>();
        projectileSpawner = transform.parent;
    }


    void FixedUpdate() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        angle = Mathf.Rad2Deg * Mathf.Atan2(projectileSpawner.position.x - mousePos.x, mousePos.y - projectileSpawner.position.y);

        if (isShooting) {
            if (PlayerMovement.isDead) {lineRenderer.positionCount = 0; return;}

            Vector2 projectileVelocity = new Vector2(mousePos.x - projectileSpawner.position.x, mousePos.y - projectileSpawner.position.y).normalized * projectileSpeed;
            Vector3[] trajectory = Plot(projectile.GetComponent<Rigidbody2D>(), projectileSpawner.position, projectileVelocity, 2000);
            lineRenderer.SetPositions(trajectory);

            if (projectileSpeed == maxProjectileSpeed) return;
            projectileSpeed += Time.deltaTime * projectileChargeSpeed;
            if (projectileSpeed > maxProjectileSpeed) projectileSpeed = maxProjectileSpeed;
        }
    }

    public void OnFire()
    {
        if (PlayerMovement.isDead || onCD) return;

        projectileSpeed = 1;
        lineRenderer.positionCount = 2000;

        animator.ResetTrigger("Release");
        animator.SetTrigger("Charge");
        animator.SetBool("isAttacking", true);

        //Here is a got spot to add SFX for charging, i.e: audioSource.PlayOneShot(chargeProjectile);

        isShooting = true;
    }

    void OnReleaseFire()
    {
        lineRenderer.positionCount = 0;

        if (PlayerMovement.isDead || onCD || !isShooting) return;

        GameObject newProjectile = Instantiate(projectile, projectileSpawner.position, Quaternion.Euler(0, 0, angle + offset));

        onCD = true;
        isShooting = false;

        animator.SetTrigger("Release");
        animator.SetBool("isAttacking", false);

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Rigidbody2D projectileRB = newProjectile.GetComponent<Rigidbody2D>();
        projectileRB.velocity = new Vector2(mousePos.x - projectileSpawner.position.x, mousePos.y - projectileSpawner.position.y).normalized * projectileSpeed;
        projectileSpeed = 1;

        //Here is a got spot to add SFX for releasing, i.e: {audioSource.Stop(); audioSource.PlayOneShot(shootProjectile);

        StartCoroutine(RotateProjectile(newProjectile, projectileRB));
        StartCoroutine(ShotCooldown());
    }

    Vector3[] Plot(Rigidbody2D rigidbody, Vector2 pos, Vector2 velocity, int steps)
    {
        Vector3[] results = new Vector3[steps];

        float timestep = Time.deltaTime / Physics2D.velocityIterations;
        Vector2 gravityAcceleration = Physics2D.gravity * rigidbody.gravityScale * timestep * timestep;
        float drag = 1f - timestep * rigidbody.drag;
        Vector2 moveStep = velocity * timestep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAcceleration;
            moveStep *= drag;
            pos += moveStep;
            results[i] = pos;
        }

        return results;
    }

    IEnumerator RotateProjectile(GameObject projectile, Rigidbody2D projectileRB)
    {
        while (projectile != null)
        {
            yield return new WaitForSecondsRealtime(0.02f);
            if (projectile == null) yield break;
            float angle = Mathf.Atan2(projectileRB.velocity.y, projectileRB.velocity.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);
        }
    }

    IEnumerator ShotCooldown()
    {
        yield return new WaitForSeconds(shotCD);
        onCD = false;
    }
}
