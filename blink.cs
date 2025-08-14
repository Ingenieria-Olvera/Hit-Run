using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkScript : MonoBehaviour
{
    public int Uses; // The amount of uses we can have
    public float cooldown, distance, speed, destinationMultiplier, cameraHeight; // I have no fucking lcue
    public Text UIText; // This dispklays the amount of uses we have left
    public Transform cam; // reference to camera
    public LayerMask layerMask; // If we hit a wall then this layer mask thingy will do its thing

    int maxUses; // the max number of uses we can have
    float cooldownTimer; // a float timer for the cool down
    bool blinking = false; // A bool to hold out differnt states
    Vector3 destination; // To hold our blink distance
    ParticleSystem trail; // To reference the particle trail thingy

    void Start()
    {
        trail = transform.Find("Trail").GetComponent<ParticleSystem>(); // to find the trail thingy
        maxUses = Uses;
        cooldownTimer = cooldown;
        UIText.text = Uses.ToString(); // Pretty sure this displays the numnber of uses since it says to string
    }
   
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0)) // so pretty simple if it gest left mouse click it blinks
        {
            Blink();
        }

        if(Uses < maxUses) 
        {
            if(cooldownTimer > 0) cooldownTimer -= Time.deltaTime; // If cool down timer is bigger then zero we lower using time.deltatime
            else { Uses += 1; cooldownTimer = cooldown; UIText.text = Uses.ToString(); } // This adds uses and displays it if cooldownTimer is below zero
        
        }
        if(blinking)
        {
            var dist = Vector3.Distance(transform.position, destination); // Checks our positoin to make sure we arent at the destination
            if(dist > 0.5f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed); // this move us to our destination I think
            }
            else{ blinking = false; }
        }
    }

    void Blink()
    {
        if(Uses > 0)
        {
            Uses -=1;
            UIText.text = Uses.ToString(); // updates the UIText
            trail.Play(); // tells the trail to play

            RaycastHit hit;
            if(Physics.Raycast(cam.position, cam.forward, out hit, distance, layerMask)) // I tyhink this moves the camera forward
            {
                destination = hit.point * destinationMultiplier; // this is fi we hit something  // this avoid blinking into anything we hit
            }
            else // this is if the raycast dosent hit anything
            {
                destination = (cam.position + cam.forward.normalized * distance) * destinationMultiplier;
                // this calculates the distance with the cameras position our distance and the destination multiplier
            }
            destination.y += cameraHeight;
            blinking = true;
        }
    }
}
