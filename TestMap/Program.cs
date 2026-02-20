using System.Security.Cryptography;
using NetUtility;
using TestMap;

var f = Utility.Utf8Hash;
Console.WriteLine("Hello, World!" >> f);
Console.WriteLine("Hello, World!" >> Encoding.UTF8.GetBytes >> SHA256.HashData >> Convert.ToHexStringLower);

static IEnumerable<int> GetAllFactors(int number)
{
    if (number == 0)
        return [];

    // 处理负数，取其绝对值
    var absNumber = Math.Abs(number);

    // 特殊处理1
    if (absNumber is 1)
        return [1];

    var factors = new SortedSet<int>();

    // 遍历到平方根即可
    var sqrt = (int)Math.Sqrt(absNumber);

    for (var i = 1; i <= sqrt; i++)
    {
        if (absNumber % i != 0) continue;
        factors.Add(i);
        factors.Add(absNumber / i);
    }

    return factors;
}


static IEnumerable<int> GetAllFactorsIncludingNegatives(int number)
{
    var positiveFactors = GetAllFactors(number).ToArray();
    // 如果是0，返回空集合
    if (positiveFactors.Length == 0)
        return [];
    // 对于每个正因数，添加对应的负因数
    return positiveFactors
        .SelectMany(f => number < 0 ? new[] { -f, f } : new[] { f, -f })
        .OrderBy(f => f);
}


static IEnumerable<int> GetProperFactors(int number)
{
    var allFactors = GetAllFactors(number);
    return allFactors.Where(f => f != Math.Abs(number));
}

internal static class Ext
{
    extension(TechMap map)
    {
        public void Print()
        {
            map.RootNode.Print(map);
        }
    }

    extension(IGadNode<Guid, TechMap.TechNodeData> node)
    {
        public void Print(TechMap map, int space = 0)
        {
            Console.WriteLine("| ".Repeat(space) + $"+-{node.Data.Title}");
            foreach (var nodeChildKey in node.Children) map[nodeChildKey].Print(map, space + 1);
        }
    }

    extension(string @string)
    {
        [Pure]
        public string Repeat(int count)
        {
            StringBuilder sb = new();
            for (var i = 0; i < count; i++) sb.Append(@string);

            return sb.ToString();
        }
    }
}