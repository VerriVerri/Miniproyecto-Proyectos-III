using UnityEngine;

public static class Mathematics
{
    public static Vector3 Equal(this Vector3 vector, float value)
    {
        return new Vector3(value, value, value);
    }
}
