using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class DragHandler : MonoBehaviour, IDragHandler
{

    public Image handleSprite;
    public int edgeLowX;
    public int edgeHighX;
    public int edgeLowY;
    public int edgeHighY;

    private MatrixController matrixController;
    private int startingRows;
    private int startingCols;
    private int currentRows;
    private int currentCols;
    private float toggleHeight;
    private float toggleWidth;
    

    void Awake()
    {
        matrixController = GetComponent<MatrixController>();
        startingRows = matrixController.Rows;
        startingCols = matrixController.Columns;
        currentCols = startingCols;
        currentRows = startingRows;
        toggleWidth = matrixController.Toggle.GetComponent<RectTransform>().sizeDelta.x;
        toggleHeight = matrixController.Toggle.GetComponent<RectTransform>().sizeDelta.y;

    }
    // Start is called before the first frame update
    void Start()
    {
        handleSprite.rectTransform.localPosition = new Vector3(startingCols * toggleWidth, -startingRows * toggleHeight, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        float positionDeltaX = 0;
        float positionDeltaY = 0;
        
        if(eventData.position.x < edgeHighX && eventData.position.x > edgeLowX)
        {
            positionDeltaX = eventData.position.x;
        }

        if(eventData.position.y < edgeHighY && eventData.position.y > edgeLowY)
        {
            positionDeltaY = eventData.position.y;
        }

        handleSprite.rectTransform.localPosition += new Vector3(positionDeltaX, positionDeltaY, 0);

        checkCols(handleSprite.rectTransform.localPosition);
        checkRows(handleSprite.rectTransform.localPosition);

        
    }

    public void checkRows(Vector3 currentPointerPosition)
    {
        if (currentPointerPosition.x > (currentRows + 1) * toggleHeight)
        {
            matrixController.AddRow();
            currentRows++;
        }

        else if (currentPointerPosition.x < (currentRows - 1) * toggleHeight)
        {
            matrixController.RemoveRow();
            currentRows--;
        }
    }

    public void checkCols(Vector3 currentPointerPosition)
    {
        if (currentPointerPosition.x > (currentRows + 1) * toggleHeight)
        {
            matrixController.AddCol();
            currentRows++;
        }

        else if (currentPointerPosition.x < (currentRows - 1) * toggleHeight)
        {
            matrixController.RemoveCol();
            currentRows--;
        }
    }
}
