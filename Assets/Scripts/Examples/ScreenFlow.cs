using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScreenFlow : MonoBehaviour
{
    [SerializeField]private GameObject screen1;
    [SerializeField]private GameObject screen2;
    
    
    
    [SerializeField]private TMP_Text display_text;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        display_text.text = "Game Has Started";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene()
    {
        
    }

    public void ShowTestInfo()
    {

        if (screen1.activeSelf == false)
        {
            screen1.SetActive(true);
        }
        else if (screen1.activeSelf == true)
        {
            screen1.SetActive(false);
        }
        
    }


    public void UpdateText()
    {


        display_text.text = "Its the start of a new job searching journey";



    }
    
    
    
    
}
