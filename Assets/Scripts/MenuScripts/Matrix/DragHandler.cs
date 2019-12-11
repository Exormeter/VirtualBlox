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
        matrixController = GetComponentInParent<MatrixController>();
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
        handleSprite.rectTransform.localPosition = new Vector3((startingCols - 0.5f) * toggleWidth, -(startingRows - 0.5f) * toggleHeight, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        Vector3 pointerPosition;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(handleSprite.rectTransform, eventData.position, eventData.pressEventCamera, out pointerPosition))
        {
            handleSprite.rectTransform.position = pointerPosition;
        }

        if (handleSprite.rectTransform.localPosition.x > edgeHighX)
        {
            handleSprite.rectTransform.localPosition = new Vector3(edgeHighX, handleSprite.rectTransform.localPosition.y, handleSprite.rectTransform.localPosition.z);
        }

        if (handleSprite.rectTransform.localPosition.x < edgeLowX)
        {
            handleSprite.rectTransform.localPosition = new Vector3(edgeLowX, handleSprite.rectTransform.localPosition.y, handleSprite.rectTransform.localPosition.z);
        }

        if (handleSprite.rectTransform.localPosition.y > edgeHighY)
        {
            handleSprite.rectTransform.localPosition = new Vector3(handleSprite.rectTransform.localPosition.x, edgeHighY, handleSprite.rectTransform.localPosition.z);
        }

        if (handleSprite.rectTransform.localPosition.y < edgeLowY)
        {
            handleSprite.rectTransform.localPosition = new Vector3(handleSprite.rectTransform.localPosition.x, edgeLowY, handleSprite.rectTransform.localPosition.z);
        }

        CheckCols(handleSprite.rectTransform.localPosition);
        CheckRows(handleSprite.rectTransform.localPosition);


    }

    public void CheckRows(Vector3 currentPointerPosition)
    {
        if (Math.Abs(currentPointerPosition.y) > (currentRows + 0.5) * toggleHeight)
        {
            matrixController.AddRow();
            currentRows++;
        }

        else if (Math.Abs(currentPointerPosition.y) < (currentRows - 0.5) * toggleHeight)
        {
            matrixController.RemoveRow();
            currentRows--;
        }
    }

    public void CheckCols(Vector3 currentPointerPosition)
    {
        if (currentPointerPosition.x > (currentCols + 0.5) * toggleHeight)
        {
            matrixController.AddCol();
            currentCols++;
        }

        else if (currentPointerPosition.x < (currentCols - 0.5) * toggleHeight)
        {
            matrixController.RemoveCol();
            currentCols--;
        }
    }

    public void UpdateHandlerPosition(int rows, int columns)
    {
        currentCols = columns;
        currentRows = rows;
        handleSprite.rectTransform.localPosition = new Vector3((currentCols - 0.5f) * toggleWidth, -(currentRows - 0.5f) * toggleHeight, 0);
    }
}
