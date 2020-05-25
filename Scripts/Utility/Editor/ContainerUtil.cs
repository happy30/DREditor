
namespace DREditor.Utility
{
    /// <summary>
    /// Class with shorthand methods for Editor GUI layout.
    /// </summary>
    public static class ContainerUtil
    {
        public static int[] Iota(int size, int value = 0)
        {
            int[] values = new int[size];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = value++;
            }
            return values;
        }

        public static T[] PrependedList<T>(T[] list, T firstElement)
        {
            T[] newList = new T[list.Length + 1];
            newList[0] = firstElement;
            for (int i = 0; i < list.Length; i++)
            {
                newList[i + 1] = list[i];
            }
            return newList;
        }
    }
}
