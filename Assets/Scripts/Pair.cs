public struct Pair<L, R>
{
    public readonly L Left;
    public readonly R Right;

    public Pair(L left, R right)
    {
        Left = left;
        Right = right;
    }

    public override int GetHashCode()
    {
        return Left.GetHashCode() + Right.GetHashCode();
    }

    public override string ToString()
    {
        return "(" + Left.ToString() + ", " + Right.ToString() + ")";
    }
}
