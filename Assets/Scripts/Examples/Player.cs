using UnityEngine;

public class Player : MonoBehaviour
{


    [SerializeField]
    private float moveSpeed;
    
    
    
    [SerializeField] private Rigidbody2D rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb =  GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        //Debug.Log("Hello Worlding!");


        if (Input.GetKeyDown(KeyCode.D))
        {
            //transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
            
            //rb.AddForce(Vector2.right * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            
            Debug.Log("D");



            transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);

        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            //transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
            
            //rb.AddForce(Vector2.left * moveSpeed * Time.deltaTime, ForceMode2D.Force);
            
            Debug.Log("A");
            
            transform.position += new Vector3(-moveSpeed * Time.deltaTime, 0, 0);
            
        }
        
        
    }
    
    
    
    
}
