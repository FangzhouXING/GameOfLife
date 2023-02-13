using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOfLife : MonoBehaviour
{
    public ComputeShader golShader; // Game Of Life Shader
    public RenderTexture renderTexture;

    public ComputeBuffer inBuffer;
    public ComputeBuffer outBuffer;

    public ComputeShader rngShader;

    const int resolutionX = 800;
    const int resolutionY = 560;
    const int threadGroupX = 8;
    const int threadGroupY = 8;
    // Start is called before the first frame update
    void Start()
    {
        inBuffer = new ComputeBuffer(resolutionX * resolutionY, sizeof(int));
        outBuffer = new ComputeBuffer(resolutionX * resolutionY, sizeof(int));

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(resolutionX, resolutionY, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        GenerateRng();
        Debug.Log("Start Function Run");

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateRng();
        }

        int kernelIndex = golShader.FindKernel("CSMain");
        golShader.SetTexture(kernelIndex, "Result", renderTexture);
        golShader.SetBuffer(kernelIndex, "InTileStates", inBuffer);
        golShader.SetBuffer(kernelIndex, "OutTileStates", outBuffer);
        golShader.SetInt("ResolutionX", resolutionX);
        golShader.SetInt("ResolutionY", resolutionY);
        golShader.Dispatch(kernelIndex, resolutionX / threadGroupX, resolutionY / threadGroupY, 1);

        //GL.Flush();
        ComputeBuffer temp = inBuffer; inBuffer = outBuffer; outBuffer = temp;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(renderTexture, destination);
    }
    
    private void GenerateRng()
    {
        int rngKernel = rngShader.FindKernel("CSMain");

        rngShader.SetBuffer(rngKernel, "Result", inBuffer);
        rngShader.SetInt("ResolutionX", resolutionX);
        rngShader.SetInt("ResolutionY", resolutionY);
        rngShader.SetFloat("time", UnityEngine.Random.Range(0.0f, 100000.0f));

        rngShader.Dispatch(rngKernel, resolutionX / threadGroupX, resolutionY / threadGroupY, 1);

    }
}