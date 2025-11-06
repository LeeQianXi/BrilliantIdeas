using System.Diagnostics.Contracts;
using System.Text;
using NetUtility.Graph;

namespace TestMap;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var map = new TechMap();
        Dictionary<int, Guid> paral = [];
        foreach (var i in Enumerable.Range(0, 100))
        {
            var cu = paral[i] = map.AddNode(new TechMap.TechNodeData(i.ToString()));
            foreach (var j in GetProperFactors(i))
            {
                if (j is 1) continue;
                map.AddChild(paral[j], cu);
            }
        }

        map = new TechMap(map.Nodes);

        map.Print();
    }

    /// <summary>
    /// 获取一个数的所有正因数（包括1和自身）
    /// </summary>
    /// <param name="number">要获取因数的数字</param>
    /// <returns>所有正因数的有序集合</returns>
    public static IEnumerable<int> GetAllFactors(int number)
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

    /// <summary>
    /// 获取一个数的所有因数（包括负因数）
    /// </summary>
    /// <param name="number">要获取因数的数字</param>
    /// <returns>所有因数的有序集合</returns>
    public static IEnumerable<int> GetAllFactorsIncludingNegatives(int number)
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

    /// <summary>
    /// 获取一个数的所有真因数（不包括自身）
    /// </summary>
    /// <param name="number">要获取因数的数字</param>
    /// <returns>所有真因数的有序集合</returns>
    public static IEnumerable<int> GetProperFactors(int number)
    {
        var allFactors = GetAllFactors(number);
        return allFactors.Where(f => f != Math.Abs(number));
    }

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