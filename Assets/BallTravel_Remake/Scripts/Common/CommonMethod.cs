namespace BallTravel_Remake.Scripts.Common
{
    public class CommonMethod
    {
        public static void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public static float ParsePercent(float current, float min, float max)
        {
            return (current - min) / (max - min);
        }
    }
}
