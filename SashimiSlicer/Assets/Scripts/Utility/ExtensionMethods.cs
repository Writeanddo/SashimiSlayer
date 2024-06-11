public static class ExtensionMethods
{
    public static bool IsIndexInFlag(this int enumAsInt, int index)
    {
        return (enumAsInt & (1 << index)) != 0;
    }
}