using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// from http://gyanendushekhar.com/2019/11/11/move-canvas-ui-mouse-drag-unity-3d-drag-drop-ui/

public class PlayerMove : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //cr�ation des variables
    private Vector2 lastMousePosition;

    bool victory = false;
    bool dragActive = false;
    Vector2 diff;
    private float update;
    private Vector3 previousPlayerPos;

    public GameObject victoryText;
    RectTransform rect;

    GameObject translationFromPlayer;
    RectTransform rectTranslationFromPlayer;
    BoxCollider colliderTranslationFromPlayer;

    private Vector3 canvaScale;
    private Vector2 playerSize;

    public bool dragAvailable = false;

    // Start is called before the first frame update
    private void Start()
    {
        //Etre s�r que l'affichage du text de victoire est bien d�sactiv�
        victoryText.SetActive(false);

        //R�cup�re le scaleFactor du canva lors de l'adaptation � la taille de la fen�tre
        canvaScale = gameObject.transform.parent.parent.localScale;

        //R�cup�ration du RectTransform du player
        rect = GetComponent<RectTransform>();

        playerSize = new Vector3(rect.sizeDelta.x * canvaScale.x, rect.sizeDelta.y * canvaScale.y);

        
    }

    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragAvailable)
        {
            Debug.Log("Begin Drag");
            lastMousePosition = eventData.position;
            dragActive = true;

            //Cr�ation d'un objet permettant de r�cup�rer le trajet effectu� par le joueur
            //Si cet objet est en collision avec un mur, le joueur ne se d�placera pas
            translationFromPlayer = new GameObject("TranslationFromPlayer");
            translationFromPlayer.transform.parent = this.gameObject.transform;
            translationFromPlayer.transform.localPosition = transform.localPosition;
            translationFromPlayer.AddComponent<BoxCollider>();
            translationFromPlayer.AddComponent<RectTransform>();

            previousPlayerPos = rect.anchoredPosition;
            rectTranslationFromPlayer = translationFromPlayer.GetComponent<RectTransform>();
            colliderTranslationFromPlayer = translationFromPlayer.GetComponent<BoxCollider>();
            rectTranslationFromPlayer.anchoredPosition = previousPlayerPos;
        }
    }

    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (dragAvailable)
        {
            update += Time.deltaTime;
            if (update > 1.0f / 60f)
            {
                //Le d�placement est possible jusqu'� ce que le joueur atteigne l'arriv�e
                if (!victory)
                {
                    //R�cup�ration des d�placements effectu�s par la souri
                    Vector2 currentMousePosition = eventData.position;
                    diff = currentMousePosition - lastMousePosition;

                    //On applique ces d�placement � l'objet permettant de suivre la translation du joueur
                    //translationFromPlayer.transform.localPosition = transform.localPosition + new Vector3(diff.x / 2f, diff.y / 2f);
                    rectTranslationFromPlayer.anchoredPosition = new Vector2(diff.x / canvaScale.x / 2f, diff.y / canvaScale.y / 2f);
                    rectTranslationFromPlayer.sizeDelta = new Vector2(Mathf.Abs(diff.x / canvaScale.x), Mathf.Abs(diff.y / canvaScale.y)) + playerSize;
                    colliderTranslationFromPlayer.size = new Vector2(Mathf.Abs(diff.x / canvaScale.x), Mathf.Abs(diff.y / canvaScale.y)) + playerSize;
                    //colliderTranslationFromPlayer.transform.Translate(-diff);
                    rectTranslationFromPlayer.anchoredPosition = -new Vector2(diff.x / canvaScale.x / 2f, diff.y / canvaScale.y / 2f);

                    //R�cup�re la position du joueur avant son d�placement
                    previousPlayerPos = rect.anchoredPosition;

                    //On d�place le joueur (reviendra en arri�re si il rencontre un objet dans la m�thode OnTriggerStay)
                    rect.anchoredPosition += new Vector2(diff.x / canvaScale.x, diff.y / canvaScale.y);

                    lastMousePosition = currentMousePosition;
                }
                else if (victory)
                {
                    StartCoroutine(EndMiniGame());
                }
            }
        }
    }

    IEnumerator EndMiniGame()
    {
        Debug.Log("victory");
        victoryText.SetActive(true);
        yield return new WaitForSeconds(1);
        gameObject.transform.parent.parent.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// This method will be called at the end of mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragAvailable)
        {
            Debug.Log("End Drag");
            dragActive = false;

            //D�truit l'objet permettant de r�cup�rer le trajet effectu� par le joueur
            Destroy(translationFromPlayer);
        }
    }

    //Appel� lorsque le joueur rentre en contact avec un autre collider
    private void OnTriggerStay(Collider other)
    {
        //Si le joueur rentre en contact avec l'arriv�e, le mini-jeu est termin�
        //Si le joueur retourne � la position qu'il avait avant sa derni�re translation
        if (other.name == "Arrive" && rect.name == "Player")
        {
            victory = true;
        }
        else
        {
            rect.anchoredPosition = previousPlayerPos;
        }
    }

    private void Update()
    {
        update += Time.deltaTime;
        if (update > 1.0f/40f)
        {
            update = 0.0f;
            if (dragActive == false && victory == false)
            {
                //R�cup�re  la position du joueur avant son d�placement
                previousPlayerPos = rect.anchoredPosition;
                int moveSpeed = 10;

                //On regarde les touches s�lectionn�es par le joueur et on d�place l'objet player en cons�quence
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    //rect.transform.localPosition += new Vector3(0, moveSpeed, 0);
                    rect.anchoredPosition += new Vector2(0, moveSpeed * canvaScale.y);
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    //rect.transform.localPosition += new Vector3(0, -moveSpeed, 0);
                    rect.anchoredPosition += new Vector2(0, -moveSpeed * canvaScale.y);
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    //rect.transform.localPosition += new Vector3(moveSpeed, 0, 0);
                    rect.anchoredPosition += new Vector2(moveSpeed * canvaScale.x, 0);
                }
                else if (Input.GetKey(KeyCode.LeftArrow))
                {
                    //rect.transform.localPosition += new Vector3(-moveSpeed, 0, 0);
                    rect.anchoredPosition += new Vector2(-moveSpeed * canvaScale.x, 0);
                }
            }
            if (victory == true && !victoryText.activeSelf)
            {
                StartCoroutine(EndMiniGame());
            }
        }
    } 
}
