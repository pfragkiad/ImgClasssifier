using System.Text.RegularExpressions;

namespace ImgClasssifier.Rating;

public class RatingIndex(int rating, int index) : IComparable<RatingIndex>
{
    public int Rating { get; } = rating;
    public int Index { get; } = index;

    public static RatingIndex? FromFilename(string filename)
    {
        Match m = Regex.Match(filename, @"^(?<rating>\d{3})_(?<index>\d{4})\.");
        if (!m.Success) return null;

        return new RatingIndex(
            rating: int.Parse(m.Groups["rating"].Value),
            index: int.Parse(m.Groups["index"].Value));
    }

    public string ToFilename(string extensionWithPeriod) =>
        $"{this}{extensionWithPeriod}";

    public override string ToString()
    {
        return $"{rating:000}_{index:0000}";
    }

    public int CompareTo(RatingIndex? other)
    {
        if (other is null) return 0;

        if (Rating < other.Rating) return -1;
        if (Rating > other.Rating) return 1;
        return Index - other.Index;
    }

    public static bool operator <(RatingIndex? left, RatingIndex? right)
    {
        if (right is null || left is null) return false;
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(RatingIndex? left, RatingIndex? right)
    {
        return left == right || left < right;
    }


    public static bool operator >=(RatingIndex? left, RatingIndex? right)
    {
        return left == right || left > right;
    }


    public static bool operator >(RatingIndex? left, RatingIndex? right)
    {
        if (right is null || left is null) return false;
        return left.CompareTo(right) > 0;

    }

    public static bool operator ==(RatingIndex? left, RatingIndex? right)
    {
        if (right is null && left is null) return true;
        if (right is null) return false;
        if (left is null) return false;

        return left.CompareTo(right) == 0;
    }

    public static bool operator !=(RatingIndex? left, RatingIndex? right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj is null)
        {
            return false;
        }

        if (obj as RatingIndex is null) return false;

        return this == (obj as RatingIndex)!;
    }

    public override int GetHashCode()
    {
        return Rating.GetHashCode() ^ (10 * Index).GetHashCode();
    }

    //public static bool operator <=(RatingIndex left, RatingIndex right)
    //{
    //    return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
    //}

    //public static bool operator >=(RatingIndex left, RatingIndex right)
    //{
    //    return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
    //}
}
