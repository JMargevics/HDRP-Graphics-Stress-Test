[System.Serializable]
public class CircularBuffer
{
    int headIndex = 0;
    float[] buffer;
    public int size
    {
        get
        {
            return buffer.Length;
        }
    }
    public CircularBuffer(int size)
    {
        buffer = new float[size];
    }

    //Adds an element to the front
    public void insert(float element)
    {
        buffer[headIndex] = element;
        headIndex = (headIndex + 1) % (buffer.Length);
    }

    //Get the index relative to head, so index=0 is the latest element and len-1 is the oldest
    public float get(int index)
    {
        int pos = headIndex - index - 1;
        if (pos < 0)
        {
            pos = buffer.Length + pos; //remember pos is negative, addition is correct
        }
        return buffer[pos];
    }
}
