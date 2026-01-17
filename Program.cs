using PicoGK;

namespace PicoGKExamples
{
    class Program
    {
        static void Main()
        {
            // Race2Space 15kN Engine - using 0.4mm voxels for fine detail
            PicoGK.Library.Go(
                0.4f,  // Finer resolution for proper mesh generation
                () =>
                {
                    Race2SpaceEngine engine = new Race2SpaceEngine();
                    engine.Task();
                }
            );
        }
    }
}