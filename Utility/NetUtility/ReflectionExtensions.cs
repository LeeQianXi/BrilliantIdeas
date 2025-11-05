// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8603 // 可能返回 null 引用。

namespace NetUtility;

public static partial class Extension
{
    private const BindingFlags DeclaredFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                               BindingFlags.Static | BindingFlags.DeclaredOnly;

    public static Type[] EmptyTypes = [];

    public static PropertyInfo GetDeclaredProperty(this Type type, string propertyName)
    {
        var props = GetDeclaredProperties(type);

        return props.FirstOrDefault(t => t.Name == propertyName);
    }

    public static MethodInfo GetDeclaredMethod(this Type type, string methodName)
    {
        var methods = GetDeclaredMethods(type);

        return methods.FirstOrDefault(t => t.Name == methodName);
    }

    public static ConstructorInfo GetDeclaredConstructor(this Type type, Type[] parameters)
    {
        var ctors = GetDeclaredConstructors(type);

        foreach (var ctor in ctors)
        {
            var ctorParams = ctor.GetParameters();

            if (parameters.Length != ctorParams.Length) continue;

            for (var j = 0; j < ctorParams.Length; ++j)
                // require an exact match
                if (ctorParams[j].ParameterType != parameters[j])
                {
                }

            return ctor;
        }

        return null;
    }

    public static ConstructorInfo[] GetDeclaredConstructors(this Type type)
    {
        return type.GetConstructors(DeclaredFlags & ~BindingFlags.Static); // LUDIQ: Exclude static constructors
    }

    public static MemberInfo[] GetFlattenedMember(this Type type, string memberName)
    {
        var result = new List<MemberInfo>();

        while (type != null)
        {
            var members = GetDeclaredMembers(type);

            result.AddRange(members.Where(t => t.Name == memberName));

            type = type.Resolve().BaseType;
        }

        return result.ToArray();
    }

    public static MethodInfo GetFlattenedMethod(this Type type, string methodName)
    {
        while (type != null)
        {
            var methods = GetDeclaredMethods(type);

            foreach (var t in methods)
                if (t.Name == methodName)
                    return t;

            type = type.Resolve().BaseType;
        }

        return null;
    }

    public static IEnumerable<MethodInfo> GetFlattenedMethods(this Type type, string methodName)
    {
        while (type != null)
        {
            var methods = GetDeclaredMethods(type);

            foreach (var t in methods)
                if (t.Name == methodName)
                    yield return t;

            type = type.Resolve().BaseType;
        }
    }

    public static PropertyInfo GetFlattenedProperty(this Type type, string propertyName)
    {
        while (type != null)
        {
            var properties = GetDeclaredProperties(type);

            foreach (var t in properties)
                if (t.Name == propertyName)
                    return t;

            type = type.Resolve().BaseType;
        }

        return null;
    }

    public static MemberInfo GetDeclaredMember(this Type type, string memberName)
    {
        var members = GetDeclaredMembers(type);

        return members.FirstOrDefault(t => t.Name == memberName);
    }

    public static MethodInfo[] GetDeclaredMethods(this Type type)
    {
        return type.GetMethods(DeclaredFlags);
    }

    public static PropertyInfo[] GetDeclaredProperties(this Type type)
    {
        return type.GetProperties(DeclaredFlags);
    }

    public static FieldInfo[] GetDeclaredFields(this Type type)
    {
        return type.GetFields(DeclaredFlags);
    }

    public static MemberInfo[] GetDeclaredMembers(this Type type)
    {
        return type.GetMembers(DeclaredFlags);
    }

    public static MemberInfo AsMemberInfo(this Type type)
    {
        return type;
    }

    public static bool IsType(this MemberInfo member)
    {
        return member is Type;
    }

    public static Type AsType(this MemberInfo member)
    {
        return (Type)member;
    }

    public static Type Resolve(this Type type)
    {
        return type;
    }

    #region Attribute Queries

    /// <summary>
    ///     Returns true if the given attribute is defined on the given element.
    /// </summary>
    public static bool HasAttribute<TAttribute>(this MemberInfo element)
    {
        return HasAttribute(element, typeof(TAttribute));
    }

    /// <summary>
    ///     Returns true if the given attribute is defined on the given element.
    /// </summary>
    public static bool HasAttribute<TAttribute>(this MemberInfo element, bool shouldCache)
    {
        return HasAttribute(element, typeof(TAttribute), shouldCache);
    }

    /// <summary>
    ///     Returns true if the given attribute is defined on the given element.
    /// </summary>
    public static bool HasAttribute(this MemberInfo element, Type attributeType)
    {
        return HasAttribute(element, attributeType, true);
    }

    /// <summary>
    ///     Returns true if the given attribute is defined on the given element.
    /// </summary>
    public static bool HasAttribute(this MemberInfo element, Type attributeType, bool shouldCache)
    {
        // LAZLO / LUDIQ FIX
        // Inheritance on property overrides. MemberInfo.IsDefined ignores the inherited parameter.
        // https://stackoverflow.com/questions/2520035
        return Attribute.IsDefined(element, attributeType, true);
        //return element.IsDefined(attributeType, true);
    }

    /// <summary>
    ///     Fetches the given attribute from the given MemberInfo. This method
    ///     applies caching and is allocation free (after caching has been
    ///     performed).
    /// </summary>
    /// <param name="element">
    ///     The MemberInfo the get the attribute from.
    /// </param>
    /// <param name="attributeType">The type of attribute to fetch.</param>
    /// <param name="shouldCache"></param>
    /// <returns>The attribute or null.</returns>
    public static Attribute GetAttribute(this MemberInfo element, Type attributeType, bool shouldCache)
    {
        var query = new AttributeQuery
        {
            MemberInfo = element,
            AttributeType = attributeType
        };

        if (_cachedAttributeQueries.TryGetValue(query, out var attribute)) return attribute;
        // LAZLO / LUDIQ FIX
        // Inheritance on property overrides. MemberInfo.IsDefined ignores the inherited parameter
        //var attributes = element.GetCustomAttributes(attributeType, /*inherit:*/ true).ToArray();
        var attributes = Attribute.GetCustomAttributes(element, attributeType, true);

        if (attributes.Length > 0) attribute = attributes[0];

        if (shouldCache) _cachedAttributeQueries[query] = attribute;

        return attribute;
    }

    /// <summary>
    ///     Fetches the given attribute from the given MemberInfo.
    /// </summary>
    /// <typeparam name="TAttribute">
    ///     The type of attribute to fetch.
    /// </typeparam>
    /// <param name="element">
    ///     The MemberInfo to get the attribute from.
    /// </param>
    /// <param name="shouldCache">
    ///     Should this computation be cached? If this is the only time it will
    ///     ever be done, don't bother caching.
    /// </param>
    /// <returns>The attribute or null.</returns>
    public static TAttribute GetAttribute<TAttribute>(this MemberInfo element, bool shouldCache)
        where TAttribute : Attribute
    {
        return (TAttribute)GetAttribute(element, typeof(TAttribute), shouldCache);
    }

    public static TAttribute GetAttribute<TAttribute>(this MemberInfo element)
        where TAttribute : Attribute
    {
        return GetAttribute<TAttribute>(element, /*shouldCache:*/ true);
    }

    private struct AttributeQuery
    {
        public MemberInfo MemberInfo;
        public Type AttributeType;
    }

    private static readonly IDictionary<AttributeQuery, Attribute> _cachedAttributeQueries =
        new Dictionary<AttributeQuery, Attribute>(new AttributeQueryComparator());

    private class AttributeQueryComparator : IEqualityComparer<AttributeQuery>
    {
        public bool Equals(AttributeQuery x, AttributeQuery y)
        {
            return
                x.MemberInfo == y.MemberInfo &&
                x.AttributeType == y.AttributeType;
        }

        public int GetHashCode(AttributeQuery obj)
        {
            return
                obj.MemberInfo.GetHashCode() +
                17 * obj.AttributeType.GetHashCode();
        }
    }

    #endregion Attribute Queries
}