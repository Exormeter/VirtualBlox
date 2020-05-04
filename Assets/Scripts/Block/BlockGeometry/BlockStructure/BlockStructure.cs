using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public abstract class BlockStructure
    {
        /// <summary>
        /// Color of the Block
        /// </summary>
        public Color BlockColor { get; set; }
        public IGenerationStrategie GenerationStrategie { protected get;  set; }

        public BlockStructure(Color color, IGenerationStrategie generationStrategie)
        {
            BlockColor = color;
            GenerationStrategie = generationStrategie;
        }

        public abstract GameObject GenerateBlock();
       
    }
}