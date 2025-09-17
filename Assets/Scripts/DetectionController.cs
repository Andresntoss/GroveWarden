using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionController : MonoBehaviour
{
    public string _tagTergetDetection = "Player";

    public List<Collider2D> detectedObjs = new List<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision) 
    {
        if(collision.gameObject.tag == _tagTergetDetection)
        {
            detectedObjs.Add(collision);
        }    
    }

    private void OnTriggerExit2D(Collider2D collision) 
    {
        if(collision.gameObject.tag == _tagTergetDetection)
        {
            detectedObjs.Remove(collision);
        }    
    }
}
